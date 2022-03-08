using System;
using System.Runtime.InteropServices;

namespace CLARTE.Serialization
{
	// Store different types values at the same offset. Therefore, all fields share the same bytes
	// Values are stored in little endian form.

	public static class Converter
	{
		public static readonly bool isLittleEndian;

		static Converter()
		{
			isLittleEndian = BitConverter.IsLittleEndian;
		}

		public static void Swap<T>(ref T b1, ref T b2)
		{
			T b = b1;

			b1 = b2;

			b2 = b;
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct Converter16
	{
		[FieldOffset(0)]
		public byte Byte1;

		[FieldOffset(sizeof(byte))]
		public byte Byte2;

		[FieldOffset(0)]
		public char _char;

		[FieldOffset(0)]
		public short _short;

		[FieldOffset(0)]
		public ushort _ushort;

		public char Char
		{
			get
			{
				if(Converter.isLittleEndian)
				{
					return _char;
				}
				else
				{
					Converter.Swap(ref Byte1, ref Byte2);

					char result = _char;

					Converter.Swap(ref Byte1, ref Byte2);

					return result;
				}
			}
		}

		public short Short
		{
			get
			{
				if(Converter.isLittleEndian)
				{
					return _short;
				}
				else
				{
					Converter.Swap(ref Byte1, ref Byte2);

					short result = _short;

					Converter.Swap(ref Byte1, ref Byte2);

					return result;
				}
			}
		}

		public ushort UShort
		{
			get
			{
				if(Converter.isLittleEndian)
				{
					return _ushort;
				}
				else
				{
					Converter.Swap(ref Byte1, ref Byte2);

					ushort result = _ushort;

					Converter.Swap(ref Byte1, ref Byte2);

					return result;
				}
			}
		}

		public Converter16(byte value1, byte value2)
		{
			_char = (char) 0;
			_short = 0;
			_ushort = 0;
			Byte1 = value1;
			Byte2 = value2;
		}

		public Converter16(char value)
		{
			Byte1 = 0;
			Byte2 = 0;
			_short = 0;
			_ushort = 0;
			_char = value;

			if(!Converter.isLittleEndian)
			{
				Converter.Swap(ref Byte1, ref Byte2);
			}
		}

		public Converter16(short value)
		{
			Byte1 = 0;
			Byte2 = 0;
			_char = (char) 0;
			_ushort = 0;
			_short = value;

			if(!Converter.isLittleEndian)
			{
				Converter.Swap(ref Byte1, ref Byte2);
			}
		}

		public Converter16(ushort value)
		{
			Byte1 = 0;
			Byte2 = 0;
			_char = (char) 0;
			_short = 0;
			_ushort = value;

			if(!Converter.isLittleEndian)
			{
				Converter.Swap(ref Byte1, ref Byte2);
			}
		}

		public static implicit operator char(Converter16 c)
		{
			return c.Char;
		}

		public static implicit operator Converter16(char v)
		{
			return new Converter16(v);
		}

		public static implicit operator short(Converter16 c)
		{
			return c.Short;
		}

		public static implicit operator Converter16(short v)
		{
			return new Converter16(v);
		}

		public static implicit operator ushort(Converter16 c)
		{
			return c.UShort;
		}

		public static implicit operator Converter16(ushort v)
		{
			return new Converter16(v);
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct Converter32
	{
		[FieldOffset(0)]
		public byte Byte1;

		[FieldOffset(sizeof(byte))]
		public byte Byte2;

		[FieldOffset(2 * sizeof(byte))]
		public byte Byte3;

		[FieldOffset(3 * sizeof(byte))]
		public byte Byte4;

		[FieldOffset(0)]
		public int _int;

		[FieldOffset(0)]
		public uint _uint;

		[FieldOffset(0)]
		public float _float;

		public int Int
		{
			get
			{
				if(Converter.isLittleEndian)
				{
					return _int;
				}
				else
				{
					Converter.Swap(ref Byte1, ref Byte4);
					Converter.Swap(ref Byte2, ref Byte3);

					int result = _int;

					Converter.Swap(ref Byte1, ref Byte4);
					Converter.Swap(ref Byte2, ref Byte3);

					return result;
				}
			}
		}

		public uint UInt
		{
			get
			{
				if(Converter.isLittleEndian)
				{
					return _uint;
				}
				else
				{
					Converter.Swap(ref Byte1, ref Byte4);
					Converter.Swap(ref Byte2, ref Byte3);

					uint result = _uint;

					Converter.Swap(ref Byte1, ref Byte4);
					Converter.Swap(ref Byte2, ref Byte3);

					return result;
				}
			}
		}

		public float Float
		{
			get
			{
				if(Converter.isLittleEndian)
				{
					return _float;
				}
				else
				{
					Converter.Swap(ref Byte1, ref Byte4);
					Converter.Swap(ref Byte2, ref Byte3);

					float result = _float;

					Converter.Swap(ref Byte1, ref Byte4);
					Converter.Swap(ref Byte2, ref Byte3);

					return result;
				}
			}
		}

		public Converter32(byte value1, byte value2, byte value3, byte value4)
		{
			_int = 0;
			_uint = 0;
			_float = 0;
			Byte1 = value1;
			Byte2 = value2;
			Byte3 = value3;
			Byte4 = value4;
		}

		public Converter32(int value)
		{
			Byte1 = 0;
			Byte2 = 0;
			Byte3 = 0;
			Byte4 = 0;
			_uint = 0;
			_float = 0;
			_int = value;

			if(!Converter.isLittleEndian)
			{
				Converter.Swap(ref Byte1, ref Byte4);
				Converter.Swap(ref Byte2, ref Byte3);
			}
		}

		public Converter32(uint value)
		{
			Byte1 = 0;
			Byte2 = 0;
			Byte3 = 0;
			Byte4 = 0;
			_int = 0;
			_float = 0;
			_uint = value;

			if(!Converter.isLittleEndian)
			{
				Converter.Swap(ref Byte1, ref Byte4);
				Converter.Swap(ref Byte2, ref Byte3);
			}
		}

		public Converter32(float value)
		{
			Byte1 = 0;
			Byte2 = 0;
			Byte3 = 0;
			Byte4 = 0;
			_int = 0;
			_uint = 0;
			_float = value;

			if(!Converter.isLittleEndian)
			{
				Converter.Swap(ref Byte1, ref Byte4);
				Converter.Swap(ref Byte2, ref Byte3);
			}
		}

		public static implicit operator int(Converter32 c)
		{
			return c.Int;
		}

		public static implicit operator Converter32(int v)
		{
			return new Converter32(v);
		}

		public static implicit operator uint(Converter32 c)
		{
			return c.UInt;
		}

		public static implicit operator Converter32(uint v)
		{
			return new Converter32(v);
		}

		public static implicit operator float(Converter32 c)
		{
			return c.Float;
		}

		public static implicit operator Converter32(float v)
		{
			return new Converter32(v);
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct Converter64
	{
		[FieldOffset(0)]
		public int Int1;

		[FieldOffset(sizeof(int))]
		public int Int2;

		[FieldOffset(0)]
		public long _long;

		[FieldOffset(0)]
		public ulong _ulong;

		[FieldOffset(0)]
		public double _double;

		public long Long
		{
			get
			{
				if(Converter.isLittleEndian)
				{
					return _long;
				}
				else
				{
					Int1 = new Converter32(Int1)._int;
					Int2 = new Converter32(Int2)._int;

					Converter.Swap(ref Int1, ref Int2);

					long result = _long;

					Int1 = new Converter32(Int1)._int;
					Int2 = new Converter32(Int2)._int;

					Converter.Swap(ref Int1, ref Int2);

					return result;
				}
			}
		}

		public ulong ULong
		{
			get
			{
				if(Converter.isLittleEndian)
				{
					return _ulong;
				}
				else
				{
					Int1 = new Converter32(Int1)._int;
					Int2 = new Converter32(Int2)._int;

					Converter.Swap(ref Int1, ref Int2);

					ulong result = _ulong;

					Int1 = new Converter32(Int1)._int;
					Int2 = new Converter32(Int2)._int;

					Converter.Swap(ref Int1, ref Int2);

					return result;
				}
			}
		}

		public double Double
		{
			get
			{
				if(Converter.isLittleEndian)
				{
					return _double;
				}
				else
				{
					Int1 = new Converter32(Int1)._int;
					Int2 = new Converter32(Int2)._int;

					Converter.Swap(ref Int1, ref Int2);

					double result = _double;

					Int1 = new Converter32(Int1)._int;
					Int2 = new Converter32(Int2)._int;

					Converter.Swap(ref Int1, ref Int2);

					return result;
				}
			}
		}

		public Converter64(int value1, int value2)
		{
			_long = 0;
			_ulong = 0;
			_double = 0;
			Int1 = value1;
			Int2 = value2;
		}

		public Converter64(long value)
		{
			Int1 = 0;
			Int2 = 0;
			_ulong = 0;
			_double = 0;
			_long = value;

			if(!Converter.isLittleEndian)
			{
				Int1 = new Converter32(Int1)._int;
				Int2 = new Converter32(Int2)._int;

				Converter.Swap(ref Int1, ref Int2);
			}
		}

		public Converter64(ulong value)
		{
			Int1 = 0;
			Int2 = 0;
			_long = 0;
			_double = 0;
			_ulong = value;

			if(!Converter.isLittleEndian)
			{
				Int1 = new Converter32(Int1)._int;
				Int2 = new Converter32(Int2)._int;

				Converter.Swap(ref Int1, ref Int2);
			}
		}

		public Converter64(double value)
		{
			Int1 = 0;
			Int2 = 0;
			_long = 0;
			_ulong = 0;
			_double = value;

			if(!Converter.isLittleEndian)
			{
				Int1 = new Converter32(Int1)._int;
				Int2 = new Converter32(Int2)._int;

				Converter.Swap(ref Int1, ref Int2);
			}
		}

		public static implicit operator long(Converter64 c)
		{
			return c.Long;
		}

		public static implicit operator Converter64(long v)
		{
			return new Converter64(v);
		}

		public static implicit operator ulong(Converter64 c)
		{
			return c.ULong;
		}

		public static implicit operator Converter64(ulong v)
		{
			return new Converter64(v);
		}

		public static implicit operator double(Converter64 c)
		{
			return c.Double;
		}

		public static implicit operator Converter64(double v)
		{
			return new Converter64(v);
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct Converter128
	{
		[FieldOffset(0)]
		public int Int1;

		[FieldOffset(sizeof(int))]
		public int Int2;

		[FieldOffset(2 * sizeof(int))]
		public int Int3;

		[FieldOffset(3 * sizeof(int))]
		public int Int4;

		[FieldOffset(0)]
		public decimal _decimal;

		public decimal Decimal
		{
			get
			{
				if(Converter.isLittleEndian)
				{
					return _decimal;
				}
				else
				{
					Int1 = new Converter32(Int1)._int;
					Int2 = new Converter32(Int2)._int;
					Int3 = new Converter32(Int3)._int;
					Int4 = new Converter32(Int4)._int;

					Converter.Swap(ref Int1, ref Int4);
					Converter.Swap(ref Int2, ref Int3);

					decimal result = _decimal;

					Int1 = new Converter32(Int1)._int;
					Int2 = new Converter32(Int2)._int;
					Int3 = new Converter32(Int3)._int;
					Int4 = new Converter32(Int4)._int;

					Converter.Swap(ref Int1, ref Int4);
					Converter.Swap(ref Int2, ref Int3);

					return result;
				}
			}
		}

		public Converter128(int value1, int value2, int value3, int value4)
		{
			_decimal = 0;
			Int1 = value1;
			Int2 = value2;
			Int3 = value3;
			Int4 = value4;
		}

		public Converter128(decimal value)
		{
			Int1 = 0;
			Int2 = 0;
			Int3 = 0;
			Int4 = 0;
			_decimal = value;

			if(!Converter.isLittleEndian)
			{
				Int1 = new Converter32(Int1)._int;
				Int2 = new Converter32(Int2)._int;
				Int3 = new Converter32(Int3)._int;
				Int4 = new Converter32(Int4)._int;

				Converter.Swap(ref Int1, ref Int4);
				Converter.Swap(ref Int2, ref Int3);
			}
		}

		public static implicit operator decimal(Converter128 c)
		{
			return c.Decimal;
		}

		public static implicit operator Converter128(decimal v)
		{
			return new Converter128(v);
		}
	}
}
