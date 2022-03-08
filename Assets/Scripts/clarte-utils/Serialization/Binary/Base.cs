using System;

namespace CLARTE.Serialization
{
	/// <summary>
	/// Binary serializer. It provide a fast and memory efficient way to serialize data into binary representation.
	/// </summary>
	/// <remarks>This class is pure C# and is compatible with all platforms, including hololens.</remarks>
	public partial class Binary
	{
		/// <summary>
		/// The types that are supported natively by the serializer. Other types can be added by implementing IBinarySerializable.
		/// </summary>
		public enum SupportedTypes : byte
		{
			NONE = 0,
			BOOL,
			BYTE,
			SBYTE,
			CHAR,
			SHORT,
			USHORT,
			INT,
			UINT,
			LONG,
			ULONG,
			FLOAT,
			DOUBLE,
			DECIMAL,
			STRING,
			TYPE,
			OBJECT,
			ENUM,
			ARRAY,
			LIST,
			DICTIONARY,
			BINARY_SERIALIZABLE,
			BINARY_TYPE_MAPPED,
			UNITY_VECTOR2,
			UNITY_VECTOR3,
			UNITY_VECTOR4,
			UNITY_QUATERNION,
			UNITY_MATRIX4X4,
			UNITY_COLOR,
			UNITY_GRADIENT,
			UNITY_GRADIENT_COLOR_KEY,
			UNITY_GRADIENT_ALPHA_KEY,
			UNITY_RECT,
			UNITY_RECT_OFFSET,
			UNITY_LAYER_MASK,
			UNITY_ANIMATION_CURVE,
			UNITY_KEY_FRAME,
			UNITY_GUI_STYLE,
			UNITY_GUI_STYLE_STATE,
			UNITY_FONT,
			UNITY_TEXTURE_2D,
		}

		/// <summary>
		/// Exception raised when an error happens during serialization.
		/// </summary>
		public class SerializationException : Exception
		{
			#region Constructors
			/// <summary>
			/// Constructor of serialization exception.
			/// </summary>
			/// <param name="message">Description of the error.</param>
			/// <param name="inner_exception">The exception that was raised during the serialization.</param>
			public SerializationException(string message, Exception inner_exception) : base(message, inner_exception)
			{

			}
			#endregion
		}

		/// <summary>
		/// Exception raised when an error happens during deserialization.
		/// </summary>
		public class DeserializationException : Exception
		{
			#region Constructors
			/// <summary>
			/// Constructor of deserialization exception.
			/// </summary>
			/// <param name="message">Description of the error.</param>
			/// /// <param name="inner_exception">The exception that was raised during the deserialization.</param>
			public DeserializationException(string message, Exception inner_exception) : base(message, inner_exception)
			{

			}
			#endregion
		}

		#region Members
		private static readonly object[] emptyParameters = new object[] { };
		#endregion

		#region Getter / Setter
		/// <summary>
		/// Get the size in bytes of a supported type.
		/// </summary>
		/// <param name="type">The type from which to get the size.</param>
		/// <returns>The number of bytes of the type once serialized, or 0 if unknown.</returns>
		public static uint Size(SupportedTypes type)
		{
			uint size;

			if(!sizes.TryGetValue(type, out size))
			{
				size = 0;
			}

			return size;
		}
		#endregion

		#region Convert from bytes
		/// <summary>
		/// Deserialize a 16 bits value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		protected uint FromBytes(Buffer buffer, uint start, out Converter16 value)
		{
			byte b1, b2;

			uint read = FromBytes(buffer, start, out b1);
			read += FromBytes(buffer, start + read, out b2);

			value = new Converter16(b1, b2);

			return read;
		}

		/// <summary>
		/// Deserialize a 32 bits value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		protected uint FromBytes(Buffer buffer, uint start, out Converter32 value)
		{
			byte b1, b2, b3, b4;

			uint read = FromBytes(buffer, start, out b1);
			read += FromBytes(buffer, start + read, out b2);
			read += FromBytes(buffer, start + read, out b3);
			read += FromBytes(buffer, start + read, out b4);

			value = new Converter32(b1, b2, b3, b4);

			return read;
		}

		/// <summary>
		/// Deserialize a 64 bits value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		protected uint FromBytes(Buffer buffer, uint start, out Converter64 value)
		{
			Converter32 i1, i2;

			uint read = FromBytes(buffer, start, out i1);
			read += FromBytes(buffer, start + read, out i2);

			value = new Converter64(i1, i2);

			return read;
		}

		/// <summary>
		/// Deserialize a 128 bits value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		protected uint FromBytes(Buffer buffer, uint start, out Converter128 value)
		{
			Converter32 i1, i2, i3, i4;

			uint read = FromBytes(buffer, start, out i1);
			read += FromBytes(buffer, start + read, out i2);
			read += FromBytes(buffer, start + read, out i3);
			read += FromBytes(buffer, start + read, out i4);

			value = new Converter128(i1, i2, i3, i4);

			return read;
		}
		#endregion

		#region Convert to bytes
		/// <summary>
		/// Serialize a 16 bits value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, Converter16 value)
		{
			CheckSerializationParameters(buffer, start);

			// Resize buffer if necessary
			ResizeBuffer(ref buffer, start + shortSize);

			byte[] data = buffer.Data;

			data[start] = value.Byte1;
			data[start + 1] = value.Byte2;

			buffer.Progress(start + shortSize);

			return shortSize;
		}

		/// <summary>
		/// Serialize a 32 bits value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, Converter32 value)
		{
			CheckSerializationParameters(buffer, start);

			// Resize buffer if necessary
			ResizeBuffer(ref buffer, start + intSize);

			byte[] data = buffer.Data;

			data[start] = value.Byte1;
			data[start + 1] = value.Byte2;
			data[start + 2] = value.Byte3;
			data[start + 3] = value.Byte4;

			buffer.Progress(start + intSize);

			return intSize;
		}

		/// <summary>
		/// Serialize a 64 bits value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, Converter64 value)
		{
			CheckSerializationParameters(buffer, start);

			// Resize buffer if necessary
			ResizeBuffer(ref buffer, start + longSize);

			uint written = ToBytes(ref buffer, start, value.Int1);
			written += ToBytes(ref buffer, start + written, value.Int2);

			return written;
		}

		/// <summary>
		/// Serialize a 128 bits value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, Converter128 value)
		{
			CheckSerializationParameters(buffer, start);

			// Resize buffer if necessary
			ResizeBuffer(ref buffer, start + decimalSize);

			uint written = ToBytes(ref buffer, start, value.Int1);
			written += ToBytes(ref buffer, start + written, value.Int2);
			written += ToBytes(ref buffer, start + written, value.Int3);
			written += ToBytes(ref buffer, start + written, value.Int4);

			return written;
		}
		#endregion

		#region Parameters checks
		protected static void CheckSerializationParameters(Buffer buffer, uint start)
		{
			if(buffer == null)
			{
				throw new ArgumentNullException("buffer", "Invalid null buffer.");
			}

			if(start > buffer.Data.Length)
			{
				throw new ArgumentException(string.Format("Invalid start position '{0}' after end of buffer of size '{1}'", start, buffer.Data.Length));
			}
		}

		protected static void CheckDeserializationParameters(Buffer buffer, uint start)
		{
			if(buffer == null)
			{
				throw new ArgumentNullException("Invalid null buffer.");
			}

			if(start >= buffer.Data.Length)
			{
				throw new ArgumentException(string.Format("Invalid start position '{0}' after end of buffer of size '{1}'", start, buffer.Data.Length));
			}
		}
		#endregion
	}
}
