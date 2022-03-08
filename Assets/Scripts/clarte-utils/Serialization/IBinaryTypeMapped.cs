namespace CLARTE.Serialization
{
	/// <summary>
	/// Interface for class to be handled by binary serializer using a custom type mapping scheme.
	/// Class implementing IBinaryTypeMapped MUST have a default constructor, otherwise
	/// deserialization will fail.
	/// </summary>
	public interface IBinaryTypeMapped : IBinarySerializable
	{

    }
}
