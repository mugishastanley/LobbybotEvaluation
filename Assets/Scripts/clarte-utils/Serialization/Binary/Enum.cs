using System;

namespace CLARTE.Serialization
{
	/// <summary>
	/// Binary serializer. It provide a fast and memory efficient way to serialize data into binary representation.
	/// </summary>
	/// <remarks>This class is pure C# and is compatible with all platforms, including hololens.</remarks>
	public partial class Binary
	{
		#region Enums
		/// <summary>
		/// Deserialize an enum.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized object.</param>
		/// <param name="enumerate">The deserialized enum.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out Enum enumerate)
		{
			Type type;

			uint read = FromBytes(buffer, start, out type);

			TypeCode underlying_type = Type.GetTypeCode(Enum.GetUnderlyingType(type));

			switch(underlying_type)
			{
				case TypeCode.Byte:
					byte b;

					read += FromBytes(buffer, start + read, out b);

					enumerate = (Enum) Enum.ToObject(type, b);

					break;
				case TypeCode.SByte:
					sbyte sb;

					read += FromBytes(buffer, start + read, out sb);

					enumerate = (Enum) Enum.ToObject(type, sb);

					break;
				case TypeCode.Int16:
					short s;

					read += FromBytes(buffer, start + read, out s);

					enumerate = (Enum) Enum.ToObject(type, s);

					break;
				case TypeCode.UInt16:
					ushort us;

					read += FromBytes(buffer, start + read, out us);

					enumerate = (Enum) Enum.ToObject(type, us);

					break;
				case TypeCode.Int32:
					int i;

					read += FromBytes(buffer, start + read, out i);

					enumerate = (Enum) Enum.ToObject(type, i);

					break;
				case TypeCode.UInt32:
					uint ui;

					read += FromBytes(buffer, start + read, out ui);

					enumerate = (Enum) Enum.ToObject(type, ui);

					break;
				case TypeCode.Int64:
					long l;

					read += FromBytes(buffer, start + read, out l);

					enumerate = (Enum) Enum.ToObject(type, l);

					break;
				case TypeCode.UInt64:
					ulong ul;

					read += FromBytes(buffer, start + read, out ul);

					enumerate = (Enum) Enum.ToObject(type, ul);

					break;
				default:
					throw new DeserializationException(string.Format("Unsupported enum underlying type. '{0}' is not a valid integral type for enums.", underlying_type), new TypeInitializationException(type.ToString(), null));
			}

			return read;
		}

		/// <summary>
		/// Serialize an enum.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="enumerate">The enum to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, Enum enumerate)
		{
			TypeCode type = enumerate.GetTypeCode();

			uint written = ToBytes(ref buffer, start, enumerate.GetType());

			switch(type)
			{
				case TypeCode.Byte:
					written += ToBytes(ref buffer, start + written, (byte) ((object) enumerate));
					break;
				case TypeCode.SByte:
					written += ToBytes(ref buffer, start + written, (sbyte) ((object) enumerate));
					break;
				case TypeCode.Int16:
					written += ToBytes(ref buffer, start + written, (short) ((object) enumerate));
					break;
				case TypeCode.UInt16:
					written += ToBytes(ref buffer, start + written, (ushort) ((object) enumerate));
					break;
				case TypeCode.Int32:
					written += ToBytes(ref buffer, start + written, (int) ((object) enumerate));
					break;
				case TypeCode.UInt32:
					written += ToBytes(ref buffer, start + written, (uint) ((object) enumerate));
					break;
				case TypeCode.Int64:
					written += ToBytes(ref buffer, start + written, (long) ((object) enumerate));
					break;
				case TypeCode.UInt64:
					written += ToBytes(ref buffer, start + written, (ulong) ((object) enumerate));
					break;
				default:
					throw new SerializationException(string.Format("Unsupported enum underlying type. '{0}' is not a valid integral type for enums.", type), new TypeInitializationException(typeof(Enum).ToString(), null));
			}

			return written;
		}
		#endregion
	}
}
