namespace CLARTE.Serialization
{
	/// <summary>
	/// Interface for class to be handled by binary serializer. Class implementing
	/// IBinarySerializable MUST have a default constructor, otherwise deserialization
	/// will fail.
	/// </summary>
	public interface IBinarySerializable
	{
		/// <summary>
		/// Method to deserialize from a byte array.
		/// </summary>
		/// <param name="serializer">The used serializer.</param>
		/// <param name="buffer">The buffer containing the data to deserialize.</param>
		/// <param name="start">The start index in the buffer of the data to deserialize.</param>
		/// <returns>The number of deserialized bytes.</returns>
		uint FromBytes(Binary serializer, Binary.Buffer buffer, uint start);

		/// <summary>
		/// Method to serialize to a byte array.
		/// </summary>
		/// <param name="serializer">The used serializer.</param>
		/// <param name="buffer">The buffer containing the data to serialize.</param>
		/// <param name="start">The start index in the buffer of the data to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		uint ToBytes(Binary serializer, ref Binary.Buffer buffer, uint start);
	}
}
