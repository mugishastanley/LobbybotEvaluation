using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CLARTE.Serialization
{
	/// <summary>
	/// Binary serializer using custom type mapping scheme. It provide a fast and memory efficient way
	/// to serialize data into binary representation. The custom mapping scheme is usefull when the
	/// type can be infered deterministically from an integer id. This way, the complete type description
	/// does not have to be serialized with the data. However, a TypeMapper must be provided to the
	/// serialization class manually for this to work. 
	/// </summary>
	/// <remarks>This class is pure C# and is compatible with all platforms, including hololens.</remarks>
	public partial class Binary
    {
		#region Members
		protected static readonly IReadOnlyDictionary<Type, uint> typeToId;
		protected static readonly IReadOnlyDictionary<uint, Type> idToType;
		#endregion

		#region Constructors
		private static void InitTypeMapper(out IReadOnlyDictionary<Type, uint> typeToId, out IReadOnlyDictionary<uint, Type> idToType)
		{
			Dictionary<Type, uint> type2Id = new Dictionary<Type, uint>();
			Dictionary<uint, Type> id2Type = new Dictionary<uint, Type>();

			// Get all the (non-abstract) classes that implement the interface IExecutable
			IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => typeof(IBinaryTypeMapped).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

			using(MD5 md5 = new MD5CryptoServiceProvider())
			{
				foreach(Type type in types)
				{
					const uint id_byte_size = sizeof(uint);

					byte[] hash = new byte[id_byte_size];

					// Get the first 4 bytes of the md5 hash of the fully qualified type name
					Array.Copy(md5.ComputeHash(Encoding.UTF8.GetBytes(type.ToString())), hash, id_byte_size);

					// Test endianness
					if(!BitConverter.IsLittleEndian)
					{
						Array.Reverse(hash);
					}

					// Convert the 4 bytes into an unsigned 32 bits integer value
					uint type_id = BitConverter.ToUInt32(hash, 0);

					// Check that no duplicate ids exists
					if(id2Type.ContainsKey(type_id))
					{
						throw new ApplicationException(string.Format("The types '{0}' and '{1}' share the same unique ID. Change name of at last one of the types to fix the ambiguity.", type, id2Type[type_id]));
					}

					// Register the association between the type and the computed id
					type2Id.Add(type, type_id);
					id2Type.Add(type_id, type);
				}
			}

			typeToId = type2Id;
			idToType = id2Type;
		}
		#endregion

		#region IBinaryTypeMapped implementation
		/// <summary>
		/// Deserialize a IBinaryTypeMapped object.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized object.</param>
		/// <param name="value">The deserialized object.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out IBinaryTypeMapped value)
		{
			uint type_id;

			CheckDeserializationParameters(buffer, start);

			uint read = FromBytes(buffer, start, out type_id);

			Type type;

			if(type_id != 0 && idToType.TryGetValue(type_id, out type))
			{
				CallDefaultConstructor(type, out value);

				read += value.FromBytes(this, buffer, start + read);
			}
			else
			{
				value = null;
			}

			return read;
		}

		/// <summary>
		/// Deserialize into an existing IBinaryTypeMapped object.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized object.</param>
		/// <param name="value">The deserialized object.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytesOverwrite(Buffer buffer, uint start, IBinaryTypeMapped value)
		{
			uint type_id;

			CheckDeserializationParameters(buffer, start);

			uint read = FromBytes(buffer, start, out type_id);

			Type type;

			if(type_id != 0 && idToType.TryGetValue(type_id, out type))
			{
				if(!type.IsAssignableFrom(value.GetType()))
				{
					throw new FormatException("The type of IBinaryTypeMapped object does not match the provided object.");
				}

				read += value.FromBytes(this, buffer, start + read);
			}
			else
			{
				// Nothing to do: we got nothing to deserialize and the value already exists, so returning null is not an option.
				// It would be great to notify the user, but we do have a mecanism for that, and raising an exception would stop
				// the deserialization, but this should just be a warning.
			}

			return read;
		}

		/// <summary>
		/// Serialize a IBinaryTypeMapped object.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The object to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, IBinaryTypeMapped value)
		{
			uint written;

			CheckSerializationParameters(buffer, start);

			uint type_id;

			if(value != null)
			{
				if(typeToId.TryGetValue(value.GetType(), out type_id) && type_id != 0)
				{
					CheckDefaultConstructor(value.GetType());

					written = ToBytes(ref buffer, start, type_id);

					written += value.ToBytes(this, ref buffer, start + written);
				}
				else
				{
					throw new SerializationException(string.Format("Type '{0}' does not have a valid mapped ID", value.GetType()), new Exception());
				}
			}
			else
			{
				written = ToBytes(ref buffer, start, (uint) 0);
			}

			return written;
		}
		#endregion

		#region Public methods
		public static uint GetTypeMappedId(IBinaryTypeMapped instance)
		{
			return GetTypeMappedId(instance?.GetType());
		}

		public static uint GetTypeMappedId(Type type)
		{
			if(type != null && typeof(IBinaryTypeMapped).IsAssignableFrom(type) && typeToId.TryGetValue(type, out uint id))
			{
				return id;
			}

			throw new ArgumentException("The given type is not a valid IBinaryTypeMapped.", "type");
		}

		public static Type GetTypeMappedType(uint id)
		{
			if(idToType.TryGetValue(id, out Type type))
			{
				return type;
			}

			throw new ArgumentException("The given ID does not reference an existing IBinaryTypeMapped.", "id");
		}
		#endregion

		#region Reflection methods
		protected static void CallDefaultConstructor(Type type, out IBinaryTypeMapped value)
		{
			value = (IBinaryTypeMapped) CheckDefaultConstructor(type).Invoke(emptyParameters);
		}
		#endregion
	}
}
