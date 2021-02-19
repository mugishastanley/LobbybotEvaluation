using UnityEngine;
using System.Collections;
using System.IO;


class BinaryReaderBigEndian : BinaryReader
{ 
	private byte[] a16 = new byte[2];
    private byte[] a32 = new byte[4];
    private byte[] a64 = new byte[8];
	public BinaryReaderBigEndian(System.IO.Stream stream)  : base(stream) { }




	public override int ReadInt32()
	{
        a32 = base.ReadBytes(4);
        System.Array.Reverse(a32);
        return System.BitConverter.ToInt32(a32,0);
    }
    public override short ReadInt16()
    {
        a16 = base.ReadBytes(2);
        System.Array.Reverse(a16);
        return System.BitConverter.ToInt16(a16, 0);
    }
    public override long ReadInt64()
    {
        a64 = base.ReadBytes(8);
        System.Array.Reverse(a64);
        return System.BitConverter.ToInt64(a64, 0);
    }
    public override uint ReadUInt32()
    {
        a32 = base.ReadBytes(4);
        System.Array.Reverse(a32);
        return System.BitConverter.ToUInt32(a32, 0);
    }
    public override ulong ReadUInt64()
    {
        a64 = base.ReadBytes(8);
        System.Array.Reverse(a64);
        return System.BitConverter.ToUInt64(a64, 0);
    }
    public override double ReadDouble()
    {
		//return base.ReadDouble();
        a64 = base.ReadBytes(8);
        System.Array.Reverse(a64);
        return System.BitConverter.ToDouble(a64, 0);
    }	
    public float ReadFloat()
    {
        a32 = base.ReadBytes(4);
        System.Array.Reverse(a32);
        return System.BitConverter.ToSingle(a32, 0);
	}
}
