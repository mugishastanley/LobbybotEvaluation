using System;
using System.Collections;
using System.Collections.Generic;

namespace CLARTE.Serialization
{
	/// <summary>
	/// Binary serializer. It provide a fast and memory efficient way to serialize data into binary representation.
	/// </summary>
	/// <remarks>This class is pure C# and is compatible with all platforms, including hololens.</remarks>
	public partial class Binary
	{
		#region Lists
		/// <summary>
		/// Deserialize a list of supported objects.
		/// </summary>
		/// <typeparam name="T">The type of objects in the list.</typeparam>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized object.</param>
		/// <param name="list">The deserialized list.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes<T>(Buffer buffer, uint start, out List<T> list)
		{
			IList l;

			uint read = FromBytes(buffer, start, out l);

			list = (List<T>) l;

			return read;
		}

		/// <summary>
		/// Deserialize a list of supported objects.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized object.</param>
		/// <param name="list">The deserialized list.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out IList list)
		{
			uint list_size, read;
			Type element_type;

			list = null;

			CheckDeserializationParameters(buffer, start);

			// Read number of elements in list
			read = FromBytes(buffer, start, out list_size);

			if(read != uintSize)
			{
				throw new FormatException(string.Format("The number of read bytes does not match the expected count. Read {0} bytes instead of {1}.", read, uintSize));
			}

			if(list_size > 0)
			{
				object value;

				// Read the type of the list's elements
				read += FromBytes(buffer, start + read, out element_type);

				if(element_type == null)
				{
					throw new FormatException("The type of elements in the list is not defined.");
				}

				// Create the final destination list (with correct capacity)
				list = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(element_type), (int) list_size);

				// Get the correct type overload to use
				SupportedTypes type = GetSupportedType(element_type);

				// Read each element one after another
				for(uint i = 0; i < list_size; ++i)
				{
					read += FromBytesWrapper(buffer, start + read, out value, type);

					// Save the correctly type value in the output list
					list.Add(value);
				}
			}

			return read;
		}

		/// <summary>
		/// Serialize a list of supported objects.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="list">The list to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, IList list)
		{
			uint written;

			CheckSerializationParameters(buffer, start);

			// If list is not defined, just write the length = 0 to the stream
			if(list == null || list.Count <= 0)
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

				uint list_size = (uint) list.Count;

				Type[] params_types = GetGenericParametersTypes(list.GetType());
				Type element_type = (params_types != null && params_types.Length >= 1 ? params_types[0] : null);

				// Get the correct type overload to use
				SupportedTypes type = GetSupportedType(element_type);

				if(type == SupportedTypes.NONE)
				{
					throw new ArgumentException(string.Format("Unsupported list type '{0}'. Either list type or values type are unsupported.", list.GetType()), "list");
				}

				// Write the length of the list in the buffer
				written = ToBytes(ref buffer, start, list_size);

				if(written != uintSize)
				{
					throw new FormatException(string.Format("The number of written bytes does not match the expected count. Wrote {0} bytes instead of {1}.", written, uintSize));
				}

				// Write the type of the list's elements
				written += ToBytes(ref buffer, start + written, element_type);

				if(sizes.TryGetValue(type, out type_size)) // If the type size is not defined, we will need to use on-the-fly buffer resizing, which is less effective.
				{
					// Check wether our buffer is large enough to get all data
					ResizeBuffer(ref buffer, start + written + list_size * type_size);
				}

				// Write all data in the buffer
				for(uint i = 0; i < list_size; ++i)
				{
					written += ToBytesWrapper(ref buffer, start + written, list[(int) i], type);
				}
			}

			return written;
		}
		#endregion
	}
}
