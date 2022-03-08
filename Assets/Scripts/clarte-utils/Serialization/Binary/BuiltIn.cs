using System;
using System.Collections.Generic;

namespace CLARTE.Serialization
{
	/// <summary>
	/// Binary serializer. It provide a fast and memory efficient way to serialize data into binary representation.
	/// </summary>
	/// <remarks>This class is pure C# and is compatible with all platforms, including hololens.</remarks>
	public partial class Binary
	{
		#region Members
		protected const uint boolSize = sizeof(bool);
		protected const uint byteSize = sizeof(byte);
		protected const uint sbyteSize = sizeof(sbyte);
		protected const uint charSize = sizeof(char);
		protected const uint shortSize = sizeof(short);
		protected const uint ushortSize = sizeof(ushort);
		protected const uint intSize = sizeof(int);
		protected const uint uintSize = sizeof(uint);
		protected const uint longSize = sizeof(long);
		protected const uint ulongSize = sizeof(ulong);
		protected const uint floatSize = sizeof(float);
		protected const uint doubleSize = sizeof(double);
		protected const uint decimalSize = sizeof(decimal);

		private static readonly Dictionary<SupportedTypes, uint> sizes = new Dictionary<SupportedTypes, uint>()
		{
			{SupportedTypes.BOOL, boolSize},
			{SupportedTypes.BYTE, byteSize},
			{SupportedTypes.SBYTE, sbyteSize},
			{SupportedTypes.CHAR, charSize},
			{SupportedTypes.SHORT, shortSize},
			{SupportedTypes.USHORT, ushortSize},
			{SupportedTypes.INT, intSize},
			{SupportedTypes.UINT, uintSize},
			{SupportedTypes.LONG, longSize},
			{SupportedTypes.ULONG, ulongSize},
			{SupportedTypes.FLOAT, floatSize},
			{SupportedTypes.DOUBLE, doubleSize},
			{SupportedTypes.DECIMAL, decimalSize},
			{SupportedTypes.TYPE, intSize},
			{SupportedTypes.UNITY_VECTOR2, 2 * floatSize},
			{SupportedTypes.UNITY_VECTOR3, 3 * floatSize},
			{SupportedTypes.UNITY_VECTOR4, 4 * floatSize},
			{SupportedTypes.UNITY_QUATERNION, 4 * floatSize},
			{SupportedTypes.UNITY_MATRIX4X4, 4 * 4 * floatSize},
			{SupportedTypes.UNITY_COLOR, 4 * byteSize},
			{SupportedTypes.UNITY_GRADIENT_COLOR_KEY, floatSize + 4 * byteSize},
			{SupportedTypes.UNITY_GRADIENT_ALPHA_KEY, 2 * floatSize},
			{SupportedTypes.UNITY_RECT, 2 * 2 * floatSize},
			{SupportedTypes.UNITY_RECT_OFFSET, 4 * intSize},
			{SupportedTypes.UNITY_LAYER_MASK, intSize},
			{SupportedTypes.UNITY_KEY_FRAME, 6 * floatSize},
		};
		#endregion

		#region Constructors
		static Binary()
		{
#pragma warning disable 0162
			if(boolSize != byteSize)
			{
				throw new NotSupportedException(string.Format("The size of types '{0}' and '{1}' does not match. ({2} != {3})", "byte", "bool", byteSize, boolSize));
			}

			if(sbyteSize != byteSize)
			{
				throw new NotSupportedException(string.Format("The size of types '{0}' and '{1}' does not match. ({2} != {3})", "byte", "sbyte", byteSize, sbyteSize));
			}

			if(charSize != 2 * byteSize)
			{
				throw new NotSupportedException(string.Format("The size of types '{0}' and '{1}' does not match. (2 * {2} != {3})", "byte", "char", byteSize, charSize));
			}

			if(shortSize != 2 * byteSize)
			{
				throw new NotSupportedException(string.Format("The size of types '{0}' and '{1}' does not match. (2 * {2} != {3})", "byte", "short", byteSize, shortSize));
			}

			if(ushortSize != shortSize)
			{
				throw new NotSupportedException(string.Format("The size of types '{0}' and '{1}' does not match. ({2} != {3})", "short", "ushort", shortSize, ushortSize));
			}

			if(intSize != 4 * byteSize)
			{
				throw new NotSupportedException(string.Format("The size of types '{0}' and '{1}' does not match. (4 * {2} != {3})", "byte", "int", byteSize, intSize));
			}

			if(uintSize != intSize)
			{
				throw new NotSupportedException(string.Format("The size of types '{0}' and '{1}' does not match. ({2} != {3})", "int", "uint", intSize, uintSize));
			}

			if(longSize != 2 * intSize)
			{
				throw new NotSupportedException(string.Format("The size of types '{0}' and '{1}' does not match. (2 * {2} != {3})", "int", "long", intSize, longSize));
			}

			if(ulongSize != longSize)
			{
				throw new NotSupportedException(string.Format("The size of types '{0}' and '{1}' does not match. ({2} != {3})", "long", "ulong", longSize, ulongSize));
			}

			if(floatSize != intSize)
			{
				throw new NotSupportedException(string.Format("The size of types '{0}' and '{1}' does not match. ({2} != {3})", "int", "float", intSize, floatSize));
			}

			if(doubleSize != 2 * floatSize)
			{
				throw new NotSupportedException(string.Format("The size of types '{0}' and '{1}' does not match. (2 * {2} != {3})", "float", "double", floatSize, doubleSize));
			}

			if(decimalSize != 4 * floatSize)
			{
				throw new NotSupportedException(string.Format("The size of types '{0}' and '{1}' does not match. (4 * {2} != {3})", "float", "decimal", floatSize, decimalSize));
			}
#pragma warning restore 0162

			InitTypeMapper(out typeToId, out idToType);
		}
		#endregion

		#region Convert from bytes
		/// <summary>
		/// Deserialize a bool value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out bool value)
		{
			byte ret;

			uint read = FromBytes(buffer, start, out ret);

			value = (ret != 0);

			return read;
		}

		/// <summary>
		/// Deserialize a byte value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out byte value)
		{
			CheckDeserializationParameters(buffer, start);

			byte[] data = buffer.Data;

			if(start + byteSize > data.Length)
			{
				throw new ArgumentException(string.Format("Buffer too small. {0} bytes required, only {1} bytes available.", byteSize, data.Length - start));
			}

			value = data[start];

			buffer.Progress(start + byteSize);

			return byteSize;
		}

		/// <summary>
		/// Deserialize a sbyte value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out sbyte value)
		{
			byte b;

			uint read = FromBytes(buffer, start, out b);

			value = (sbyte) b;

			return read;
		}

		/// <summary>
		/// Deserialize a char value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out char value)
		{
			Converter16 c;

			uint read = FromBytes(buffer, start, out c);

			value = c;

			return read;
		}

		/// <summary>
		/// Deserialize a short value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out short value)
		{
			Converter16 c;

			uint read = FromBytes(buffer, start, out c);

			value = c;

			return read;
		}

		/// <summary>
		/// Deserialize a ushort value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out ushort value)
		{
			Converter16 c;

			uint read = FromBytes(buffer, start, out c);

			value = c;

			return read;
		}

		/// <summary>
		/// Deserialize a int value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out int value)
		{
			Converter32 c;

			uint read = FromBytes(buffer, start, out c);

			value = c;

			return read;
		}

		/// <summary>
		/// Deserialize a uint value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out uint value)
		{
			Converter32 c;

			uint read = FromBytes(buffer, start, out c);

			value = c;

			return read;
		}

		/// <summary>
		/// Deserialize a long value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out long value)
		{
			Converter64 c;

			uint read = FromBytes(buffer, start, out c);

			value = c;

			return read;
		}

		/// <summary>
		/// Deserialize a ulong value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out ulong value)
		{
			Converter64 c;

			uint read = FromBytes(buffer, start, out c);

			value = c;

			return read;
		}

		/// <summary>
		/// Deserialize a float value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out float value)
		{
			Converter32 c;

			uint read = FromBytes(buffer, start, out c);

			value = c;

			return read;
		}

		/// <summary>
		/// Deserialize a double value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out double value)
		{
			Converter64 c;

			uint read = FromBytes(buffer, start, out c);

			value = c;

			return read;
		}

		/// <summary>
		/// Deserialize a decimal value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out decimal value)
		{
			Converter128 c;

			uint read = FromBytes(buffer, start, out c);

			value = c;

			return read;
		}

		/// <summary>
		/// Deserialize a string value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out string value)
		{
			uint size;

			value = null;

			CheckDeserializationParameters(buffer, start);

			uint read = FromBytes(buffer, start, out size);

			if(read != uintSize)
			{
				throw new FormatException(string.Format("The number of read bytes does not match the expected count. Read {0} bytes instead of {1}.", read, uintSize));
			}

			if(size > 0)
			{
				if(start + size > buffer.Data.Length)
				{
					throw new ArgumentException(string.Format("Buffer too small. {0} bytes required, only {1} bytes available.", size, buffer.Data.Length - start));
				}

				value = System.Text.Encoding.UTF8.GetString(buffer.Data, (int) (start + read), (int) size);

				read += size;
			}

			buffer.Progress(start + read);

			return read;
		}

		/// <summary>
		/// Deserialize a Type value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out Type value)
		{
			string raw_complete_type;

			// Get the precise type of this object
			uint read = FromBytes(buffer, start, out raw_complete_type);

			// If an explicit type is defined, use it.
			if (!string.IsNullOrEmpty(raw_complete_type))
			{
				try
				{
					value = Type.GetType(raw_complete_type);

					if (value == null)
					{
						throw new TypeLoadException();
					}
				}
				catch (Exception)
				{
					throw new SerializationException(string.Format("Missing type '{0}'. Use 'link.xml' files to include missing type in build.", raw_complete_type), new TypeLoadException(string.Format("Missing type '{0}'.", raw_complete_type)));
				}
			}
			else
			{
				value = null;
			}

			return read;
		}
		#endregion

		#region Convert to bytes
		/// <summary>
		/// Serialize a bool value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, bool value)
		{
			return ToBytes(ref buffer, start, value ? (byte) 0x1 : (byte) 0x0);
		}

		/// <summary>
		/// Serialize a byte value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, byte value)
		{
			CheckSerializationParameters(buffer, start);

			// Resize buffer if necessary
			ResizeBuffer(ref buffer, start + byteSize);

			buffer.Data[start] = value;

			buffer.Progress(start + byteSize);

			return byteSize;
		}

		/// <summary>
		/// Serialize a sbyte value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, sbyte value)
		{
			return ToBytes(ref buffer, start, (byte) value);
		}

		/// <summary>
		/// Serialize a char value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, char value)
		{
			return ToBytes(ref buffer, start, (Converter16) value);
		}

		/// <summary>
		/// Serialize a short value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, short value)
		{
			return ToBytes(ref buffer, start, (Converter16) value);
		}

		/// <summary>
		/// Serialize a ushort value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, ushort value)
		{
			return ToBytes(ref buffer, start, (Converter16) value);
		}

		/// <summary>
		/// Serialize a int value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, int value)
		{
			return ToBytes(ref buffer, start, (Converter32) value);
		}

		/// <summary>
		/// Serialize a uint value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, uint value)
		{
			return ToBytes(ref buffer, start, (Converter32) value);
		}

		/// <summary>
		/// Serialize a long value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, long value)
		{
			return ToBytes(ref buffer, start, (Converter64) value);
		}

		/// <summary>
		/// Serialize a ulong value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, ulong value)
		{
			return ToBytes(ref buffer, start, (Converter64) value);
		}

		/// <summary>
		/// Serialize a float value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, float value)
		{
			return ToBytes(ref buffer, start, (Converter32) value);
		}

		/// <summary>
		/// Serialize a double value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, double value)
		{
			return ToBytes(ref buffer, start, (Converter64) value);
		}

		/// <summary>
		/// Serialize a decimal value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, decimal value)
		{
			return ToBytes(ref buffer, start, (Converter128) value);
		}

		/// <summary>
		/// Serialize a string value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, string value)
		{
			uint written = 0;

			CheckSerializationParameters(buffer, start);

			if(!string.IsNullOrEmpty(value))
			{
				// Get number of required bytes
				uint size = (uint) System.Text.Encoding.UTF8.GetByteCount(value);

				// Make sure the buffer is large enough
				ResizeBuffer(ref buffer, start + uintSize + size);

				// Encode the string length first
				written = ToBytes(ref buffer, start, size);

				if(written != uintSize)
				{
					throw new FormatException(string.Format("The number of written bytes does not match the expected count. Wrote {0} bytes instead of {1}.", written, uintSize));
				}

				// Add the string bytes to the buffer (in-place)
				written += (uint) System.Text.Encoding.UTF8.GetBytes(value, 0, value.Length, buffer.Data, (int) (start + uintSize));
			}
			else
			{
				written = ToBytes(ref buffer, start, 0u);

				if(written != uintSize)
				{
					throw new FormatException(string.Format("The number of written bytes does not match the expected count. Wrote {0} bytes instead of {1}.", written, uintSize));
				}
			}

			buffer.Progress(start + written);

			return written;
		}

		/// <summary>
		/// Serialize a Type value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, Type value)
		{
			// Serialize the type info
			return ToBytes(ref buffer, start, value != null ? string.Format("{0}, {1}", value.ToString(), value.Assembly.GetName().Name) : "");
		}
		#endregion
	}
}
