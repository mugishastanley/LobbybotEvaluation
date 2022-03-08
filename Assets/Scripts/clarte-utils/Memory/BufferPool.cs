using System;
using System.Collections.Generic;

namespace CLARTE.Memory
{
	/// <summary>
	/// Buffer pool. It provide an efficient way to get reusable and resizable buffers to limit allocation and garbage collection.
	/// </summary>
	/// <remarks>This class is pure C# and is compatible with all platforms, including hololens.</remarks>
	public class BufferPool
	{
		/// <summary>
		/// A buffer of bytes.
		/// </summary>
		public class Buffer : IDisposable
		{
			#region Members
			private object referencesLock;
			private uint referencesCount;
			#endregion

			#region Getter / Setter
			/// <summary>
			/// The buffer pool managing this buffer.
			/// </summary>
			protected BufferPool Manager { get; private set; }

			/// <summary>
			/// Get the buffer bytes data.
			/// </summary>
			public byte[] Data { get; private set; }

			/// <summary>
			/// Get the buffer occupied size.
			/// </summary>
			public uint Size { get; set; }

			/// <summary>
			/// The number of times this buffer have been resized.
			/// </summary>
			protected uint ResizeCount { get; set; }
			#endregion

			#region Constructors
			/// <summary>
			/// Create a new buffer.
			/// </summary>
			/// <remarks>This is the shared constructors code. This constructor should never be called by itself.</remarks>
			/// <param name="manager">The associated buffer pool.</param>
			protected Buffer(BufferPool manager)
			{
				Manager = manager;
				Data = null;
				Size = 0;
				ResizeCount = 0;

				referencesLock = new object();
				referencesCount = 1;
			}

			/// <summary>
			/// Create a new buffer from an existing one and get ownership of data.
			/// </summary>
			/// <param name="other">The other buffer to construct from.</param>
			protected Buffer(Buffer other) : this(other.Manager)
            {
				Transfert(other);
            }

			/// <summary>
			/// Create a new buffer of at least min_size bytes.
			/// </summary>
			/// <remarks>The buffer can potentially be bigger, depending on the available allocated resources.</remarks>
			/// <param name="manager">The associated buffer pool.</param>
			/// <param name="min_size">The minimal size of the buffer.</param>
			public Buffer(BufferPool manager, uint min_size) : this(manager)
			{
				Data = manager.Grab(min_size);
			}

			/// <summary>
			/// Create a new buffer from existing data.
			/// </summary>
			/// <param name="manager">The associated buffer pool.</param>
			/// <param name="existing_data">The existing data.</param>
			/// <param name="add_to_pool">True if the data array must be kept and added to the pool once the buffer is disposed.</param>
			public Buffer(BufferPool manager, byte[] existing_data, bool add_to_pool = false) : this(add_to_pool ? manager : null)
			{
				Size = (uint)existing_data.Length;

				Data = existing_data;
			}
			#endregion

			#region Destructor
			// Make sure that internal data get released to the buffer pool
			~Buffer()
			{
				Dispose(true);
			}
			#endregion

			#region IDisposable implementation
			/// <summary>
			/// Dispose of buffer and return associated ressources to the pool.
			/// </summary>
			/// <param name="disposing">If true, release the memory to the pool.</param>
			/// <returns>True if the object was disposed, false otherwise.</returns>
			protected virtual bool Dispose(bool disposing)
			{
				bool dispose = false;

				lock (referencesLock)
				{
					if (referencesCount > 0)
					{
						referencesCount--;

						if (referencesCount == 0)
						{
							dispose = true;
						}
					}
				}

				if(dispose)
				{
					if (disposing)
					{
						// TODO: delete managed state (managed objects).
						Manager?.Release(Data);
					}

					// TODO: free unmanaged resources (unmanaged objects) and replace finalizer below.

					// TODO: set fields of large size with null value.
					ResizeCount = 0;
					Size = 0;
					Data = null;
					Manager = null;
				}

				return dispose;
			}

			/// <summary>
			/// Dispose of the buffer. Release the allocated memory to the buffer pool for futur use.
			/// </summary>
			public void Dispose()
			{
				// Pass true in dispose method to clean managed resources too and say GC to skip finalize in next line.
				if(Dispose(true))
				{
					// If dispose is called already then say GC to skip finalize on this instance.
					// TODO: uncomment next line if finalizer is replaced above.
					GC.SuppressFinalize(this);
				}
			}
			#endregion

			#region References count
			public void SetReferencesCount(uint count)
			{
				lock(referencesLock)
				{
					if (referencesCount != 0)
					{
						referencesCount = Math.Max(count, 1);
					}
				}
			}

			public void IncrementReferencesCount()
			{
				lock (referencesLock)
				{
					if (referencesCount != 0)
					{
						referencesCount = (uint)Math.Min(((long)referencesCount) + 1, uint.MaxValue);
					}
				}
			}

			public void DecrementReferencesCount()
			{
				lock (referencesLock)
				{
					if (referencesCount != 0)
					{
						referencesCount = Math.Max(referencesCount - 1, 1);
					}
				}
			}
			#endregion

			#region Mutations
			/// <summary>
			/// Transfert data ownership from other buffer to us.
			/// </summary>
			/// <remarks>After calling this method, the other buffer is disposed automatically and ownership of data is transfered to us.</remarks>
			/// <param name="other">Other buffer to get data from.</param>
			protected void Transfert(Buffer other)
            {
				Manager = other.Manager;
				Data = other.Data;
				Size = other.Size;
				ResizeCount = other.ResizeCount;

				other.Data = null;

				lock (other.referencesLock)
				{
					lock (referencesLock)
					{
						referencesCount = other.referencesCount;
					}

					other.referencesCount = 1;
				}

				// Release other buffer without releasing memory, as ownership is transfered to us.
				other.Dispose(false);
				GC.SuppressFinalize(other);
			}
			#endregion

			#region Resize
			/// <summary>
			/// Resize a buffer to a new size of at least min_size.
			/// </summary>
			/// <remarks>The buffer can potentially be bigger, depending on the available allocated resources. After calling this method, the current buffer is disposed automatically and ownership of data is transfered to the new buffer.</remarks>
			/// <param name="min_size">The new minimal size of the new buffer.</param>
			/// <param name="buffer_factory">A function that create a new buffer of the requested min size.</param>
			protected Buffer Resize(uint min_size, Func<uint, Buffer> buffer_factory)
			{
				if (Data.Length >= min_size)
				{
					return this;
				}
				else // Buffer too small: resize
				{
					// Get how much memory we need. The idea is to reduce the need of further resizes down the road
					// for buffers that are frequently resized, while avoiding to get too much memory for buffers
					// of relatively constant size. Therefore, we allocate at least the size needed, plus an offset
					// that depends on the number of times this buffer has been resized, as well as the relative
					// impact of this resize (to avoid allocating huge amount of memory if a resize increase drastically
					// the size of the buffer. Hopefully, this algorithm should allow a fast convergence to the
					// ideal buffer size. However, keep in mind that resizes should be a last resort and should be avoided
					// when possible.
					uint current_size = (uint)Data.Length;
					float growth = Math.Max(1f - ((float)min_size) / current_size, minResizeOffset);
					uint new_size = min_size + (uint)(ResizeCount * growth * min_size);

					// Get a new buffer of sufficient size
					Buffer new_buffer = buffer_factory(new_size);

					// Increment resize count
					new_buffer.ResizeCount = ResizeCount + 1;

					// Copy old buffer content into new one
					Array.Copy(Data, new_buffer.Data, Data.Length);
					new_buffer.Size = Size;

					// Release old buffer
					// Actually, do not call dispose for this buffer! If we do, it will be added back to the pool
					// of available buffers and the allocated memory could increase drastically over time.
					// Instead, we purposefully ignore to release it. Therefore, the memory will be released when
					// the buffer gets out of scope, i.e. at the end of this function.
					Dispose(false);
					GC.SuppressFinalize(this);

					// Switch buffers
					return new_buffer;
				}
			}

			/// <summary>
			/// Resize a buffer to a new size of at least min_size.
			/// </summary>
			/// <remarks>The buffer can potentially be bigger, depending on the available allocated resources. After calling this method, the current buffer is disposed automatically and ownership of data is transfered to the new buffer.</remarks>
			/// <param name="min_size">The new minimal size of the new buffer.</param>
			public Buffer Resize(uint min_size)
			{
				return Resize(min_size, s => new Buffer(Manager, s));
			}
			#endregion
		}

		/// <summary>
		/// A buffer of bytes.
		/// </summary>
		public class Buffer<T> : Buffer
		{
			#region Getter / Setter
			/// <summary>
			/// Get the context associated with the buffer.
			/// </summary>
			public T Context { get; private set; }
			#endregion

			#region Constructors
			/// <summary>
			/// Create a new buffer.
			/// </summary>
			/// <remarks>This is the shared constructors code. This constructor should never be called by itself.</remarks>
			/// <param name="manager">The associated buffer pool.</param>
			/// <param name="context">The associated context.</param>
			protected Buffer(BufferPool manager, T context) : base(manager)
			{
				Context = context;
			}

			/// <summary>
			/// Create a new buffer from an existing one and get ownership of data.
			/// </summary>
			/// <param name="other">The other buffer to construct from.</param>
			/// <param name="context">The context associated with the buffer.</param>
			public Buffer(Buffer other, T context) : base((BufferPool) null)
			{
				Context = context;

				Transfert(other);
			}

			/// <summary>
			/// Create a new buffer from an existing one and get ownership of data.
			/// </summary>
			/// <param name="other">The other buffer to construct from.</param>
			public Buffer(Buffer<T> other) : base(other.Manager)
			{
				Context = other.Context;

				other.Context = default(T);

				Transfert(other);
			}

			/// <summary>
			/// Create a new buffer of at least min_size bytes.
			/// </summary>
			/// <remarks>The buffer can potentially be bigger, depending on the available allocated resources.</remarks>
			/// <param name="manager">The associated buffer pool.</param>
			/// <param name="context">The context associated with the buffer.</param>
			/// <param name="min_size">The minimal size of the buffer.</param>
			public Buffer(BufferPool manager, T context, uint min_size) : base(manager, min_size)
			{
				Context = context;
			}

			/// <summary>
			/// Create a new buffer from existing data.
			/// </summary>
			/// <param name="manager">The associated buffer pool.</param>
			/// <param name="context">The context associated with the buffer.</param>
			/// <param name="existing_data">The existing data.</param>
			/// <param name="add_to_pool">True if the data array must be kept and added to the pool once the buffer is disposed.</param>
			public Buffer(BufferPool manager, T context, byte[] existing_data, bool add_to_pool = false) : base(manager, existing_data, add_to_pool)
			{
				Context = context;
			}
			#endregion

			#region IDisposable implementation
			/// <summary>
			/// Dispose of buffer and return associated ressources to the pool.
			/// </summary>
			/// <param name="disposing">If true, release the memory to the pool.</param>
			/// <returns>True if the object was disposed, false otherwise.</returns>
			protected override bool Dispose(bool disposing)
			{
				bool dispose = base.Dispose(disposing);

				if (dispose)
				{
					if (disposing)
					{
						// TODO: delete managed state (managed objects).
					}

					// TODO: free unmanaged resources (unmanaged objects) and replace finalizer below.
					(Context as IDisposable)?.Dispose();

					// TODO: set fields of large size with null value.
					Context = default(T);
				}

				return dispose;
			}
			#endregion

			#region Mutations
			/// <summary>
			/// Transform current buffer into another buffer with same data but different context.
			/// </summary>
			/// <remarks>After calling this method, the current buffer is disposed automatically and ownership of data is transfered to the new buffer.</remarks>
			/// <typeparam name="U">The type of the new context.</typeparam>
			/// <param name="context">The new context to use.</param>
			/// <returns></returns>
			public Buffer<U> Mutate<U>(U context)
			{
				Buffer<U> result = new Buffer<U>(Manager, context);

				result.Transfert(this);

				return result;
			}

			/// <summary>
			/// Transform current buffer into another buffer with same data but different context.
			/// </summary>
			/// <remarks>After calling this method, the current buffer is disposed automatically and ownership of data is transfered to the new buffer.</remarks>
			/// <typeparam name="U">The type of the new context.</typeparam>
			/// <param name="context_converter">A function to convert the old context into the new one.</param>
			/// <returns></returns>
			public Buffer<U> Mutate<U>(Func<T, U> context_converter)
			{
				return Mutate(context_converter != null ? context_converter(Context) : default(U));
			}
			#endregion

			#region Resize
			/// <summary>
			/// Resize a buffer to a new size of at least min_size.
			/// </summary>
			/// <remarks>The buffer can potentially be bigger, depending on the available allocated resources. After calling this method, the current buffer is disposed automatically and ownership of data is transfered to the new buffer.</remarks>
			/// <param name="min_size">The new minimal size of the new buffer.</param>
			public new Buffer<T> Resize(uint min_size)
			{
				return (Buffer<T>)Resize(min_size, s => new Buffer<T>(Manager, Context, s));
			}
			#endregion
		}

		#region Members
		public const float minResizeOffset = 0.1f;

		private LinkedList<byte[]> available = new LinkedList<byte[]>();
		#endregion

		#region Buffer handling
		/// <summary>
		/// Get a buffer of at least min_size.
		/// </summary>
		/// <remarks>The buffer can potentially be bigger, depending on the available allocated resources.</remarks>
		/// <typeparam name="T">The type of the context associated with the buffer.</typeparam>
		/// <param name="context">The context associated to the buffer.</param>
		/// <param name="min_size">The minimal size of the buffer.</param>
		/// <returns>A buffer.</returns>
		public Buffer<T> GetBuffer<T>(T context, uint min_size)
		{
			return new Buffer<T>(this, context, min_size);
		}

		/// <summary>
		/// Get a buffer from existing data.
		/// </summary>
		/// <typeparam name="T">The type of the context associated with the buffer.</typeparam>
		/// <param name="context">The context associated to the buffer.</param>
		/// <param name="data">The existing data.</param>
		/// <param name="add_to_pool">True if the data array must be kept and added to the pool once the buffer is disposed.</param>
		/// <returns>A buffer.</returns>
		public Buffer<T> GetBufferFromExistingData<T>(T context, byte[] data, bool add_to_pool = false)
		{
			return new Buffer<T>(this, context, data, add_to_pool);
		}

		/// <summary>
		/// Resize a buffer to a new size of at least min_size.
		/// </summary>
		/// <remarks>The buffer can potentially be bigger, depending on the available allocated resources.</remarks>
		/// <typeparam name="T">The type of the context associated with the buffer.</typeparam>
		/// <param name="buffer">The buffer to resize.</param>
		/// <param name="min_size">The new minimal size of the buffer.</param>
		public void ResizeBuffer<T>(ref Buffer<T> buffer, uint min_size)
		{
			if(buffer == null)
			{
				throw new ArgumentNullException("buffer", "Can not resize undefined buffer.");
			}
			
			buffer = buffer.Resize(min_size);
		}

		private byte[] Grab(uint min_size)
		{
			byte[] buffer = null;

			lock(available)
			{
				// Is their some available buffer ?
				if(available.Count > 0)
				{
					// Get the first buffer of sufficient size
					for(LinkedListNode<byte[]> it = available.First; it != null; it = it.Next)
					{
						if(it.Value.Length >= min_size)
						{
							buffer = it.Value;

							available.Remove(it);

							break;
						}
					}

					// No buffer wide enough ? Resize the smallest one to fit
					if(buffer == null)
					{
						// Avoid creating too many buffers that would ultimately pollute the pool
						available.RemoveFirst();

						// The actual buffer will be created later to avoid doing it in the lock scope
					}
				}
				// else no buffer available : create a new one. But we will do it later, out of the lock scope.
			}

			if(buffer == null)
			{
				// Buffer still null ? We need to create it of sufficient size. It may be that no buffer is available,
				// or that we are resizing the smallest one.
				buffer = new byte[min_size];
			}

			return buffer;
		}

		private void Release(byte[] buffer)
		{
			if(buffer != null)
			{
				lock(available)
				{
					int size = buffer.Length;

					// Store the buffer back in the sorted list of available buffers
					if(available.Count <= 0 || available.Last.Value.Length <= size)
					{
						// Either no buffer in list or buffer bigger than the bigger one : store it at the end
						available.AddLast(buffer);
					}
					else
					{
						// Add it before the first element of same size or bigger
						for(LinkedListNode<byte[]> it = available.First; it != null; it = it.Next)
						{
							if(it.Value.Length >= size)
							{
								available.AddBefore(it, buffer);

								break;
							}
						}
					}
				}
			}
		}
		#endregion
	}
}
