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
		#region Dictionaries
		/// <summary>
		/// Deserialize a dictionary of supported objects.
		/// </summary>
		/// <typeparam name="T">The type of keys in the dictionary.</typeparam>
		/// <typeparam name="U">The type of values in the dictionary.</typeparam>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized object.</param>
		/// <param name="dict">The deserialized dictionary.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes<T, U>(Buffer buffer, uint start, out Dictionary<T, U> dict)
		{
			IDictionary d;

			uint read = FromBytes(buffer, start, out d);

			dict = (Dictionary<T, U>) d;

			return read;
		}

		/// <summary>
		/// Deserialize a dictionary of supported objects.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized object.</param>
		/// <param name="dict">The deserialized dictionary.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out IDictionary dict)
		{
			uint nb_elements;
			Type key_element_type, value_element_type;

			CheckDeserializationParameters(buffer, start);

			uint read = FromBytes(buffer, start, out nb_elements);

			if(read != uintSize)
			{
				throw new FormatException(string.Format("The number of read bytes does not match the expected count. Read {0} bytes instead of {1}.", read, uintSize));
			}

			if(nb_elements > 0)
			{
				// Read the type of the dictionary's keys
				read += FromBytes(buffer, start + read, out key_element_type);

				if(key_element_type == null)
				{
					throw new FormatException("The type of keys in the dictionary is not defined.");
				}

				// Read the type of the dictionary's values
				read += FromBytes(buffer, start + read, out value_element_type);

				if(value_element_type == null)
				{
					throw new FormatException("The type of values in the dictionary is not defined.");
				}

				dict = (IDictionary) Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(key_element_type, value_element_type), (int) nb_elements);

				// Get the correct type overloads to use
				SupportedTypes type_key = GetSupportedType(key_element_type);
				SupportedTypes type_value = GetSupportedType(value_element_type);

				object key;
				object value;

				for(uint i = 0; i < nb_elements; ++i)
				{
					read += FromBytesWrapper(buffer, start + read, out key, type_key);
					read += FromBytesWrapper(buffer, start + read, out value, type_value);

					dict.Add(key, value);
				}
			}
			else
			{
				dict = null;
			}

			return read;
		}

		/// <summary>
		/// Serialize a dictionary of supported objects.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="dict">The dictionary to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, IDictionary dict)
		{
			uint written, type_size;

			CheckSerializationParameters(buffer, start);

			if(dict == null || dict.Count <= 0)
			{
				written = ToBytes(ref buffer, start, 0u);

				if(written != uintSize)
				{
					throw new FormatException(string.Format("The number of written bytes does not match the expected count. Wrote {0} bytes instead of {1}.", written, uintSize));
				}
			}
			else
			{
				uint size = uintSize;

				uint nb_elements = (uint) dict.Count;

				Type[] params_types = GetGenericParametersTypes(dict.GetType());
				Type key_element_type = (params_types != null && params_types.Length >= 1 ? params_types[0] : null);
				Type value_element_type = (params_types != null && params_types.Length >= 2 ? params_types[1] : null);

				// Get the correct type overloads to use
				SupportedTypes type_key = GetSupportedType(key_element_type);
				SupportedTypes type_value = GetSupportedType(value_element_type);

				if(type_key == SupportedTypes.NONE | type_value == SupportedTypes.NONE)
				{
					throw new ArgumentException(string.Format("Unsupported dictionary type '{0}'. Either dictionnary type or keys / values types are unsupported.", dict.GetType()), "dict");
				}

				if(sizes.TryGetValue(type_key, out type_size)) // If the type size is not defined, we will need to use on-the-fly buffer resizing, which is less effective.
				{
					size += nb_elements * type_size;
				}

				if(sizes.TryGetValue(type_value, out type_size)) // If the type size is not defined, we will need to use on-the-fly buffer resizing, which is less effective.
				{
					size += nb_elements * type_size;
				}

				ResizeBuffer(ref buffer, start + size);

				written = ToBytes(ref buffer, start, nb_elements);

				if(written != uintSize)
				{
					throw new FormatException(string.Format("The number of written bytes does not match the expected count. Wrote {0} bytes instead of {1}.", written, uintSize));
				}

				// Write the type of the dictionary's keys and values
				written += ToBytes(ref buffer, start + written, key_element_type);
				written += ToBytes(ref buffer, start + written, value_element_type);

				foreach(DictionaryEntry pair in dict)
				{
					written += ToBytesWrapper(ref buffer, start + written, pair.Key, type_key);
					written += ToBytesWrapper(ref buffer, start + written, pair.Value, type_value);
				}
			}

			return written;
		}
		#endregion

		#region Parameters checks
		protected static Type[] GetGenericParametersTypes(Type type)
		{
#if NETFX_CORE
			Type[] element_types = (type.GetTypeInfo().IsGenericType ? type.GetTypeInfo().GetGenericArguments() : null);
#else
			Type[] element_types = (type.IsGenericType ? type.GetGenericArguments() : null);
#endif

			return element_types;
		}
		#endregion
	}
}
