using System;

namespace CLARTE.Serialization
{
	/// <summary>
	/// Binary serializer. It provide a fast and memory efficient way to serialize data into binary representation.
	/// </summary>
	/// <remarks>This class is pure C# and is compatible with all platforms, including hololens.</remarks>
	public partial class Binary
	{
		#region Arrays
		/// <summary>
		/// Deserialize an array of supported objects.
		/// </summary>
		/// <typeparam name="T">The type of objects in the array.</typeparam>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized object.</param>
		/// <param name="array">The deserialized array.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes<T>(Buffer buffer, uint start, out T[] array)
		{
			Array a;

			uint read = FromBytes(buffer, start, out a);

			array = (T[]) a;

			return read;
		}

		public uint FromBytes(Buffer buffer, uint start, out Array array)
		{
			uint array_size, read;
			Type element_type;

			array = null;

			CheckDeserializationParameters(buffer, start);

			// Read number of elements in array
			read = FromBytes(buffer, start, out array_size);

			if(read != uintSize)
			{
				throw new FormatException(string.Format("The number of read bytes does not match the expected count. Read {0} bytes instead of {1}.", read, uintSize));
			}

			if(array_size > 0)
			{
				object value;

				// Read the type of the array's elements
				read += FromBytes(buffer, start + read, out element_type);

				if(element_type == null)
				{
					throw new FormatException("The type of elements in the array is not defined.");
				}

				// Create the final destination array
				array = Array.CreateInstance(element_type, array_size);

				// Get the correct type overload to use
				SupportedTypes type = GetSupportedType(element_type);

				// Read each element one after another
				for(uint i = 0; i < array_size; ++i)
				{
					read += FromBytesWrapper(buffer, start + read, out value, type);

					// Save the correctly type value in the output array
					array.SetValue(value, i);
				}
			}

			return read;
		}

		/// <summary>
		/// Deserialize an array of bytes.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized object.</param>
		/// <param name="array">The deserialized array.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out byte[] array)
		{
			uint array_size, read;
			Type element_type;

			array = null;

			CheckDeserializationParameters(buffer, start);

			// Read number of elements in array
			read = FromBytes(buffer, start, out array_size);

			if(read != uintSize)
			{
				throw new FormatException(string.Format("The number of read bytes does not match the expected count. Read {0} bytes instead of {1}.", read, uintSize));
			}

			if(array_size > 0)
			{
				uint array_bytes_size = array_size * byteSize;

				if(start + array_bytes_size > buffer.Data.Length)
				{
					throw new ArgumentException(string.Format("Buffer too small. {0} bytes required, only {1} bytes available.", array_bytes_size, buffer.Data.Length - start));
				}

				// Read the type of the array's elements
				read += FromBytes(buffer, start + read, out element_type);

				if(element_type == null)
				{
					throw new FormatException("The type of elements in the array is not defined.");
				}

				if(element_type != typeof(byte))
				{
					throw new FormatException("The type of elements in the array is not 'byte' as expected.");
				}

				// Create the final destination array
				array = new byte[array_size];

				// Copy elements as fast as possible
				Array.Copy(buffer.Data, (int) (start + read), array, 0, (int) array_bytes_size);

				read += array_bytes_size;
			}

			buffer.Progress(start + read);

			return read;
		}

		/// <summary>
		/// Serialize an array of supported objects.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="array">The array to serialize.</param>
		/// <param name="length">The length of the array to serialize. Default to the complete array.</param>
		/// <param name="offset">The offset of the first element to serialize. Default to the first element.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, Array array, int length = -1, int offset = 0)
		{
			uint written;

			CheckSerializationParameters(buffer, start);

			int array_physical_length = array != null ? array.Length : -1;

			// If array is not defined, just write the length = 0 to the stream
			if(array_physical_length <= 0 || length == 0 || offset >= array_physical_length)
			{
				written = ToBytes(ref buffer, start, 0u);

				if(written != uintSize)
				{
					throw new FormatException(string.Format("The number of written bytes does not match the expected count. Wrote {0} bytes instead of {1}.", written, uintSize));
				}
			}
			else
			{
				uint type_size;

				uint array_size = (uint) (length < 0 ? array_physical_length : length);

				if(array_size + offset > array_physical_length)
				{
					array_size = (uint) (array_physical_length - offset);
				}

				// Get the correct type overload to use
				Type element_type = array.GetType().GetElementType();
				SupportedTypes type = GetSupportedType(element_type);

				if(type == SupportedTypes.NONE)
				{
					throw new ArgumentException(string.Format("Unsupported array type '{0}'. Values type is unsupported.", array.GetType()), "array");
				}

				// Write the length of the array in the buffer
				written = ToBytes(ref buffer, start, array_size);

				if(written != uintSize)
				{
					throw new FormatException(string.Format("The number of written bytes does not match the expected count. Wrote {0} bytes instead of {1}.", written, uintSize));
				}

				// Write the type of the array's elements
				written += ToBytes(ref buffer, start + written, element_type);

				if(sizes.TryGetValue(type, out type_size)) // If the type size is not defined, we will need to use on-the-fly buffer resizing, which is less effective.
				{
					// Check wether our buffer is large enough to get all data
					ResizeBuffer(ref buffer, start + written + array_size * type_size);
				}

				// Write all data in the buffer
				for(uint i = 0; i < array_size; ++i)
				{
					written += ToBytesWrapper(ref buffer, start + written, array.GetValue(i + offset), type);
				}
			}

			return written;
		}

		/// <summary>
		/// Serialize an array of bytes.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="array">The array to serialize.</param>
		/// <param name="length">The length of the array to serialize. Default to the complete array.</param>
		/// <param name="offset">The offset of the first element to serialize. Default to the first element.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, byte[] array, int length = -1, int offset = 0)
		{
			uint written;

			CheckSerializationParameters(buffer, start);

			int array_physical_length = array != null ? array.Length : -1;

			// If array is not defined, just write the length = 0 to the stream
			if(array_physical_length <= 0 || length == 0 || offset >= array_physical_length)
			{
				written = ToBytes(ref buffer, start, 0u);

				if(written != uintSize)
				{
					throw new FormatException(string.Format("The number of written bytes does not match the expected count. Wrote {0} bytes instead of {1}.", written, uintSize));
				}
			}
			else
			{
				uint array_size = (uint) (length < 0 ? array_physical_length : length);

				if(array_size + offset > array_physical_length)
				{
					array_size = (uint) (array_physical_length - offset);
				}

				uint array_bytes_size = array_size * byteSize;

				// Write the length of the array in the buffer
				written = ToBytes(ref buffer, start, array_size);

				if(written != uintSize)
				{
					throw new FormatException(string.Format("The number of written bytes does not match the expected count. Wrote {0} bytes instead of {1}.", written, uintSize));
				}

				// Write the type of the array's elements
				written += ToBytes(ref buffer, start + written, typeof(byte));

				// Check wether our buffer is large enough to get all data
				ResizeBuffer(ref buffer, start + written + array_bytes_size);

				// Write all data in the buffer as fast as possible
				Array.Copy(array, offset, buffer.Data, (int) (start + written), (int) array_bytes_size);

				written += array_bytes_size;
			}

			buffer.Progress(start + written);

			return written;
		}
		#endregion
	}
}
