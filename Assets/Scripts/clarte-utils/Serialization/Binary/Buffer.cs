using System;

namespace CLARTE.Serialization
{
	/// <summary>
	/// Binary serializer. It provide a fast and memory efficient way to serialize data into binary representation.
	/// </summary>
	/// <remarks>This class is pure C# and is compatible with all platforms, including hololens.</remarks>
	public partial class Binary
    {
		protected static class SystemTypeSerializer
        {
			#region Type serialization callbacks
			public static uint FromBytes(Binary serializer, Buffer buffer, uint start, out Type value)
			{
				return serializer.FromBytes(buffer, start, out value);
			}

			public static uint ToBytes(Binary serializer, ref Buffer buffer, uint start, Type value)
			{
				return serializer.ToBytes(ref buffer, start, value);
			}
			#endregion
		}

		public class SerializationBufferContext
        {
            #region Members
            public IDMap<Type> types;
            public Action<float> progress;
            #endregion

            #region Constructors
			public SerializationBufferContext(Binary serializer, Action<float> progress = null)
            {
				this.progress = progress;

				types = new IDMap<Type>(serializer, SystemTypeSerializer.ToBytes, SystemTypeSerializer.FromBytes);
			}
            #endregion
        }

		public class Buffer : Memory.BufferPool.Buffer<SerializationBufferContext>
        {
			#region Constructors
			/// <summary>
			/// Create a new buffer.
			/// </summary>
			/// <remarks>This is the shared constructors code. This constructor should never be called by itself.</remarks>
			/// <param name="manager">The associated buffer pool.</param>
			/// <param name="min_size">The minimal size of the buffer.</param>
			/// <param name="context">The associated context.</param>
			protected Buffer(Memory.BufferPool manager, uint min_size, SerializationBufferContext context) : base(manager, context, min_size)
			{

			}

			/// <summary>
			/// Create a buffer from an existing internal buffer
			/// </summary>
			/// <param name="buffer">Existing internal buffer.</param>
			/// <param name="manager">The associated serializer.</param>
			/// <param name="progress">Callback to report progress in serialization.</param>
			public Buffer(Memory.BufferPool.Buffer buffer, Binary manager, Action<float> progress = null) : base(buffer, new SerializationBufferContext(manager, progress))
			{

			}

			/// <summary>
			/// Create a buffer from an existing internal buffer
			/// </summary>
			/// <param name="buffer">Existing internal buffer.</param>
			public Buffer(Memory.BufferPool.Buffer<SerializationBufferContext> buffer) : base(buffer)
            {

            }

			/// <summary>
			/// Create a new buffer of at least min_size bytes.
			/// </summary>
			/// <remarks>The buffer can potentially be bigger, depending on the available allocated resources.</remarks>
			/// <param name="manager">The associated serializer.</param>
			/// <param name="min_size">The minimal size of the buffer.</param>
			/// <param name="progress">Callback to report progress in serialization.</param>
			public Buffer(Binary manager, uint min_size, Action<float> progress = null) : base(manager.buffers, new SerializationBufferContext(manager, progress), min_size)
			{

			}

			/// <summary>
			/// Create a new buffer from existing data.
			/// </summary>
			/// <param name="manager">The associated serializer.</param>
			/// <param name="existing_data">The existing data.</param>
			/// <param name="add_to_pool">True if the data array must be kept and added to the pool once the buffer is disposed.</param>
			/// <param name="progress">Callback to report progress in serialization.</param>
			public Buffer(Binary manager, byte[] existing_data, bool add_to_pool = false, Action<float> progress = null) : base(manager.buffers, new SerializationBufferContext(manager, progress), existing_data, add_to_pool)
			{

			}
			#endregion

			#region Resize
			/// <summary>
			/// Resize a buffer to a new size of at least min_size.
			/// </summary>
			/// <remarks>The buffer can potentially be bigger, depending on the available allocated resources. After calling this method, the current buffer is disposed automatically and ownership of data is transfered to the new buffer.</remarks>
			/// <param name="min_size">The new minimal size of the new buffer.</param>
			public new Buffer Resize(uint min_size)
			{
				return (Buffer)Resize(min_size, s => new Buffer(Manager, s, Context));
			}
            #endregion

            #region Progress
            /// <summary>
            /// Update the progress notification.
            /// </summary>
            /// <param name="position">Position reached in the buffer.</param>
            public void Progress(uint position)
			{
				Context?.progress?.Invoke(Data != null ? ((float)position) / Data.Length : 1f);
			}
            #endregion
        }

        #region Members
        protected Memory.BufferPool buffers;
        #endregion

        #region Constructors
		public Binary()
        {
			buffers = new Memory.BufferPool();
		}

		public Binary(Memory.BufferPool buffers)
        {
			this.buffers = buffers ?? new Memory.BufferPool();
        }
        #endregion

        #region Buffer handling overloads
        /// <summary>
        /// Get a buffer of at least min_size.
        /// </summary>
        /// <remarks>The buffer can potentially be bigger, depending on the available allocated resources.</remarks>
        /// <param name="min_size">The minimal size of the buffer.</param>
        /// <param name="progress">Callback to report progress in serialization.</param>
        /// <returns>A buffer.</returns>
        public Buffer GetBuffer(uint min_size, Action<float> progress = null)
		{
			return new Buffer(this, min_size, progress);
		}

		/// <summary>
		/// Get a buffer from existing data.
		/// </summary>
		/// <param name="data">The existing data.</param>
		/// <param name="add_to_pool">True if the data array must be kept and added to the pool once the buffer is disposed.</param>
		/// <param name="progress">Callback to report progress in serialization.</param>
		/// <returns>A buffer.</returns>
		public Buffer GetBufferFromExistingData(byte[] data, bool add_to_pool = false, Action<float> progress = null)
		{
			return new Buffer(this, data, add_to_pool, progress);
		}

		/// <summary>
		/// Resize a buffer to a new size of at least min_size.
		/// </summary>
		/// <remarks>The buffer can potentially be bigger, depending on the available allocated resources.</remarks>
		/// <param name="buffer">The buffer to resize.</param>
		/// <param name="min_size">The new minimal size of the buffer.</param>
		public void ResizeBuffer(ref Buffer buffer, uint min_size)
		{
			buffer = buffer.Resize(min_size);
		}
		#endregion
	}
}
