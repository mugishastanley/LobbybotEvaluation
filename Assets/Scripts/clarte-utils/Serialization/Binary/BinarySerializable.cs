using System;
using System.Reflection;

namespace CLARTE.Serialization
{
	/// <summary>
	/// Binary serializer. It provide a fast and memory efficient way to serialize data into binary representation.
	/// </summary>
	/// <remarks>This class is pure C# and is compatible with all platforms, including hololens.</remarks>
	public partial class Binary
	{
		#region IBinarySerializable serialization
		/// <summary>
		/// Deserialize a IBinarySerializable object.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized object.</param>
		/// <param name="value">The deserialized object.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out IBinarySerializable value)
		{
			Type type;

			CheckDeserializationParameters(buffer, start);

			uint read = FromBytes(buffer, start, out type);

			if(type != null)
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
		/// Deserialize into an existing IBinarySerializable object.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized object.</param>
		/// <param name="value">The deserialized object.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytesOverwrite(Buffer buffer, uint start, IBinarySerializable value)
		{
			Type type;

			CheckDeserializationParameters(buffer, start);

			uint read = FromBytes(buffer, start, out type);

			if(type != null)
			{
				if(!type.IsAssignableFrom(value.GetType()))
				{
					throw new FormatException("The type of IBynarySerializable object does not match the provided object.");
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
		/// Serialize a IBinarySerializable object.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The object to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, IBinarySerializable value)
		{
			uint written;

			CheckSerializationParameters(buffer, start);

			if(value != null)
			{
				Type type = value.GetType();

				CheckDefaultConstructor(type);

				written = ToBytes(ref buffer, start, type);

				written += value.ToBytes(this, ref buffer, start + written);
			}
			else
			{
				written = ToBytes(ref buffer, start, (Type) null);
			}

			return written;
		}
		#endregion

		#region Reflection methods
		protected static ConstructorInfo CheckDefaultConstructor(Type type)
		{
			ConstructorInfo constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);

			if(constructor == null)
			{
				throw new ArgumentException(string.Format("Invalid deserialization of object of type '{0}'. No default constructor defined.", type.FullName));
			}

			return constructor;
		}

		protected static void CallDefaultConstructor(Type type, out IBinarySerializable value)
		{
			value = (IBinarySerializable) CheckDefaultConstructor(type).Invoke(emptyParameters);
		}
		#endregion
	}
}
