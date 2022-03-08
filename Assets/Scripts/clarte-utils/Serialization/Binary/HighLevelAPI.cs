using System;
using System.Collections;
using System.Threading.Tasks;

namespace CLARTE.Serialization
{
	/// <summary>
	/// Binary serializer. It provide a fast and memory efficient way to serialize data into binary representation.
	/// </summary>
	/// <remarks>This class is pure C# and is compatible with all platforms, including hololens.</remarks>
	public partial class Binary
	{
		/// <summary>
		/// Delegate used to pass user defined serialization logic.
		/// </summary>
		/// <param name="serializer">The serializer to use.</param>
		/// <param name="buffer">The buffer to use.</param>
		/// <returns>The number of bytes written.</returns>
		public delegate uint SerializationCallback(Binary serializer, ref Buffer buffer);

		/// <summary>
		/// Delegate used to pass user defined deserialization logic.
		/// </summary>
		/// <param name="serializer">The serializer to use.</param>
		/// <param name="buffer">The buffer to use.</param>
		/// <returns>The number of bytes read.</returns>
		public delegate uint DeserializationCallback(Binary serializer, Buffer buffer);

		/// <summary>
		/// Helper class used for wrapping serialized / deserialized value when using simple one value serialization logic. 
		/// </summary>
		/// <remarks>Sadly, this must be a class and not a struct, otherwise the data object value will not be propagated from the deserializer to the caller.</remarks>
		protected class DefaultSerializationCallbacks
		{
			#region Members
			/// <summary>
			/// The serialized / deserialized value.
			/// </summary>
			public object data;
			#endregion

			#region Constructors
			/// <summary>
			/// Constructor.
			/// </summary>
			public DefaultSerializationCallbacks()
			{
				data = null;
			}

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="data">The value to serialize.</param>
			public DefaultSerializationCallbacks(object data)
			{
				this.data = data;
			}
			#endregion

			#region Public methods
			/// <summary>
			/// Callback used to pass simple one value serialization logic.
			/// </summary>
			/// <param name="serializer">The serializer to use.</param>
			/// <param name="buffer">The buffer to use.</param>
			/// <returns>The number of bytes written.</returns>
			public uint SerializationCallback(Binary serializer, ref Buffer buffer)
			{
				return serializer != null && buffer != null ? serializer.ToBytes(ref buffer, 0, data) : 0;
			}

			/// <summary>
			/// Callback used to pass simple one value deserialization logic.
			/// </summary>
			/// <param name="serializer">The serializer to use.</param>
			/// <param name="buffer">The buffer to use.</param>
			/// <returns>The number of bytes read.</returns>
			public uint DeserializationCallback(Binary serializer, Buffer buffer)
			{
				data = null;

				return serializer.FromBytes(buffer, 0, out data);
			}
			#endregion
		}

		#region Members
		/// <summary>
		/// Serialization buffer of 10 Mo by default.
		/// </summary>
		public const uint defaultSerializationBufferSize = 1024 * 1024 * 10;

		private static readonly TimeSpan progressRefresRate = new TimeSpan(0, 0, 0, 0, 40);
		#endregion

		#region Public serialization methods
		/// <summary>
		/// Serialize an object to a byte array asynchronously, using user defined logic.
		/// </summary>
		/// <param name="serialization_callback">The callback used to serialize the data once the context is set.</param>
		/// <param name="callback">A callback called once the data is serialized to get the result byte array and serialized size.</param>
		/// <param name="progress">A callback to get progress notifications.</param>
		/// <param name="default_buffer_size">The default size to use for serialization buffer.</param>
		/// <returns>An enumerator to wait for the serialization completion.</returns>
		public IEnumerator Serialize(SerializationCallback serialization_callback, Action<Buffer> callback, Action<float> progress = null, uint default_buffer_size = defaultSerializationBufferSize)
		{
			Buffer buffer = null;

			DateTime time = DateTime.Now + progressRefresRate;
			float progress_percentage = 0f;

			buffer = GetBuffer(default_buffer_size, p => progress_percentage = p);

			Task<uint> result = Task.Run(() => serialization_callback(this, ref buffer));

			while (!result.IsCompleted)
			{
				if (progress != null && DateTime.Now >= time)
				{
					progress(progress_percentage);

					time = DateTime.Now + progressRefresRate;
				}

				yield return null;
			}

			if (result.Exception != null)
			{
				throw new SerializationException("An error occured during serialization.", result.Exception);
			}

			buffer.Size = result.Result;

			callback?.Invoke(buffer);
		}

		/// <summary>
		/// Serialize an object to a byte array asynchronously.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="callback">A callback called once the data is serialized to get the result byte array and serialized size.</param>
		/// <param name="progress">A callback to get progress notifications.</param>
		/// <param name="default_buffer_size">The default size to use for serialization buffer.</param>
		/// <returns>An enumerator to wait for the serialization completion.</returns>
		public IEnumerator Serialize(object value, Action<Buffer> callback, Action<float> progress = null, uint default_buffer_size = defaultSerializationBufferSize)
		{
			return Serialize(new DefaultSerializationCallbacks(value).SerializationCallback, callback, progress, default_buffer_size);
		}

		/// <summary>
		/// Serialize an object to a file asynchronously, using user defined logic.
		/// </summary>
		/// <param name="serialization_callback">The callback used to serialize the data once the context is set.</param>
		/// <param name="filename">The name of the file where to save the serialized data.</param>
		/// <param name="callback">A callback called once the data is serialized to know if the serialization was a success.</param>
		/// <param name="progress">A callback to get progress notifications.</param>
		/// <param name="default_buffer_size">The default size to use for serialization buffer.</param>
		/// <returns>An enumerator to wait for the serialization completion.</returns>
		public IEnumerator Serialize(SerializationCallback serialization_callback, string filename, Action<bool> callback = null, Action<float> progress = null, uint default_buffer_size = defaultSerializationBufferSize)
		{
			return Serialize(serialization_callback, b => WriteToFile(b, filename, callback), progress, default_buffer_size);
		}

		/// <summary>
		/// Serialize an object to a file asynchronously.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="filename">The name of the file where to save the serialized data.</param>
		/// <param name="callback">A callback called once the data is serialized to know if the serialization was a success.</param>
		/// <param name="progress">A callback to get progress notifications.</param>
		/// <param name="default_buffer_size">The default size to use for serialization buffer.</param>
		/// <returns>An enumerator to wait for the serialization completion.</returns>
		public IEnumerator Serialize(object value, string filename, Action<bool> callback = null, Action<float> progress = null, uint default_buffer_size = defaultSerializationBufferSize)
		{
			return Serialize(value, b => WriteToFile(b, filename, callback), progress, default_buffer_size);
		}

		/// <summary>
		/// Serialize objects to a buffer synchronously, using user defined logic.
		/// </summary>
		/// <remarks>Serialized data size is store in buffer Size property. Returned buffer must be disposed after use.</remarks>
		/// <param name="serialization_callback">The callback used to serialize the data once the context is set.</param>
		/// <param name="default_buffer_size">The default size to use for serialization buffer.</param>
		/// <returns>The serialized data.</returns>
		public Buffer Serialize(SerializationCallback serialization_callback, uint default_buffer_size = defaultSerializationBufferSize)
		{
			Buffer buffer = null;

			try
			{
				buffer = GetBuffer(default_buffer_size);

				buffer.Size = serialization_callback(this, ref buffer);
			}
			catch (Exception e)
			{
				throw new SerializationException("An error occured during serialization.", e);
			}

			return buffer;
		}

		/// <summary>
		/// Serialize an object to a buffer synchronously.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="default_buffer_size">The default size to use for serialization buffer.</param>
		/// <returns>The serialized data.</returns>
		public Buffer Serialize(object value, uint default_buffer_size = defaultSerializationBufferSize)
		{
			return Serialize(new DefaultSerializationCallbacks(value).SerializationCallback, default_buffer_size);
		}

		/// <summary>
		/// Deserialize an object from a byte array asynchronously, using user defined logic.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="deserialization_callback">The callback used to deserialize the data once the context is set.</param>
		/// <param name="progress">A callback to get progress notifications.</param>
		/// <returns>An enumerator to wait for the deserialization completion.</returns>
		public IEnumerator Deserialize(Buffer buffer, DeserializationCallback deserialization_callback, Action<float> progress = null)
		{
			DateTime time = DateTime.Now + progressRefresRate;
			float progress_percentage = 0f;

			using (buffer)
			{
				if (buffer.Context.progress == null)
				{
					buffer.Context.progress = p => progress_percentage = p;
				}

				Task<uint> result = Task.Run(() => deserialization_callback(this, buffer));

				while (!result.IsCompleted)
				{
					if (progress != null && DateTime.Now >= time)
					{
						progress(progress_percentage);

						time = DateTime.Now + progressRefresRate;
					}

					yield return null;
				}

				if (result.Exception != null)
				{
					throw new DeserializationException("An error occured during deserialization.", result.Exception);
				}
				else if (result.Result != buffer.Size)
				{
					throw new DeserializationException("Not all available data was used.", null);
				}
			}
		}

		/// <summary>
		/// Deserialize an object from a buffer asynchronously.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="callback">A callback to get the deserialized object.</param>
		/// <param name="progress">A callback to get progress notifications.</param>
		/// <returns>An enumerator to wait for the deserialization completion.</returns>
		public IEnumerator Deserialize(Buffer buffer, Action<object> callback, Action<float> progress = null)
		{
			DefaultSerializationCallbacks context = new DefaultSerializationCallbacks();

			IEnumerator it = Deserialize(buffer, context.DeserializationCallback, progress);

			while (it.MoveNext())
			{
				yield return it.Current;
			}

			callback?.Invoke(context.data);
		}

		/// <summary>
		/// Deserialize an object from a byte array asynchronously, using user defined logic.
		/// </summary>
		/// <param name="data">The byte array containing the serialized data.</param>
		/// <param name="deserialization_callback">The callback used to deserialize the data once the context is set.</param>
		/// <param name="callback">A callback to get the deserialized object.</param>
		/// <param name="progress">A callback to get progress notifications.</param>
		/// <returns>An enumerator to wait for the deserialization completion.</returns>
		public IEnumerator Deserialize(byte[] data, DeserializationCallback deserialization_callback, Action<float> progress = null)
		{
			return Deserialize(GetBufferFromExistingData(data, false), deserialization_callback, progress);
		}

		/// <summary>
		/// Deserialize an object from a byte array asynchronously.
		/// </summary>
		/// <param name="data">The byte array containing the serialized data.</param>
		/// <param name="callback">A callback to get the deserialized object.</param>
		/// <param name="progress">A callback to get progress notifications.</param>
		/// <returns>An enumerator to wait for the deserialization completion.</returns>
		public IEnumerator Deserialize(byte[] data, Action<object> callback, Action<float> progress = null)
		{
			return Deserialize(GetBufferFromExistingData(data, false), callback, progress);
		}

		/// <summary>
		/// Deserialize an object from a file asynchronously, using user defined logic.
		/// </summary>
		/// <param name="filename">The name of the file where to get the deserialized data.</param>
		/// /// <param name="deserialization_callback">The callback used to deserialize the data once the context is set.</param>
		/// <param name="progress">A callback to get progress notifications.</param>
		/// <returns>An enumerator to wait for the deserialization completion.</returns>
		public IEnumerator Deserialize(string filename, DeserializationCallback deserialization_callback, Action<float> progress = null)
		{
			return Deserialize(ReadFromFile(filename), deserialization_callback, progress);
		}

		/// <summary>
		/// Deserialize an object from a file asynchronously.
		/// </summary>
		/// <param name="filename">The name of the file where to get the deserialized data.</param>
		/// <param name="callback">A callback to get the deserialized object.</param>
		/// <param name="progress">A callback to get progress notifications.</param>
		/// <returns>An enumerator to wait for the deserialization completion.</returns>
		public IEnumerator Deserialize(string filename, Action<object> callback, Action<float> progress = null)
		{
			return Deserialize(ReadFromFile(filename), callback);
		}

		/// <summary>
		/// Deserialize an object from a buffer synchronously, using user defined logic.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="deserialization_callback">The callback used to deserialize the data once the context is set.</param>
		public void Deserialize(Buffer buffer, DeserializationCallback deserialization_callback)
		{
			using (buffer)
			{
				try
				{
					uint read = deserialization_callback(this, buffer);

					if (read != buffer.Size)
					{
						throw new DeserializationException("Not all available data was used.", null);
					}
				}
				catch (Exception e)
				{
					throw new DeserializationException("An error occured during deserialization.", e);
				}
			}
		}

		/// <summary>
		/// Deserialize an object from a buffer synchronously.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <returns>The deserialized object.</returns>
		public object Deserialize(Buffer buffer)
		{
			DefaultSerializationCallbacks context = new DefaultSerializationCallbacks();

			Deserialize(buffer, context.DeserializationCallback);

			return context.data;
		}

		/// <summary>
		/// Deserialize an object from a byte array synchronously, using user defined logic.
		/// </summary>
		/// <param name="data">The byte array containing the serialized data.</param>
		/// <param name="deserialization_callback">The callback used to deserialize the data once the context is set.</param>
		public void Deserialize(byte[] data, DeserializationCallback deserialization_callback)
		{
			Deserialize(GetBufferFromExistingData(data, false), deserialization_callback);
		}

		/// <summary>
		/// Deserialize an object from a byte array synchronously.
		/// </summary>
		/// <param name="data">The byte array containing the serialized data.</param>
		/// <returns>The deserialized object.</returns>
		public object Deserialize(byte[] data)
		{
			return Deserialize(GetBufferFromExistingData(data, false));
		}
		#endregion

		#region Internal methods
		/// <summary>
		/// Write serialized content to file.
		/// </summary>
		/// <param name="buffer">Buffer containing serialized data.</param>
		/// <param name="filename">Filename where to save serialized data.</param>
		/// <param name="callback">Callback used to notify if serialized data was properly saved to file.</param>
		protected void WriteToFile(Buffer buffer, string filename, Action<bool> callback)
		{
			bool success = false;

			try
			{
				if (buffer != null && buffer.Data != null && buffer.Data.Length > 0)
				{
					if (buffer.Size > int.MaxValue)
					{
						throw new NotSupportedException("Can not write files bigger than 2GB.");
					}

					using (System.IO.FileStream fs = System.IO.File.Open(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
					{
						fs.Write(buffer.Data, 0, (int) buffer.Size);
					}

					success = true;
				}
			}
			catch(Exception e)
            {
				throw new SerializationException(string.Format("An error occured while saving serialized data to '{0}'.", filename), e);
			}
			finally
			{
				callback?.Invoke(success);
			}
		}

		/// <summary>
		/// Read content of file into buffer.
		/// </summary>
		/// <param name="filename">The file to read.</param>
		/// <returns>A buffer containing the data read from file.</returns>
		protected Buffer ReadFromFile(string filename)
        {
			Buffer buffer = null;

			System.IO.FileInfo file = new System.IO.FileInfo(filename);

			if(file.Exists && file.Length >= 0)
            {
				if(file.Length > int.MaxValue)
                {
					throw new NotSupportedException("Can not read files bigger than 2GB.");
                }

				buffer = GetBuffer((uint) file.Length);

				using (System.IO.FileStream fs = new System.IO.FileStream(file.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
				{
					fs.Read(buffer.Data, 0, (int) file.Length);
				}

				buffer.Size = (uint) file.Length;
			}

			return buffer;
		}
		#endregion
	}
}
