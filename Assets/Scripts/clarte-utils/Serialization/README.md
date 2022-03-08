Serialization
===============

The 'Serialization' namespace contains all the classes and helper tools
relative to serialization:
- 'Binary' is a high performance serialization tool that can be used on
  all platforms, including hololens. It provides high level methods
  'Serialize' and 'Deserialize', available both in synchronous or
  asynchronous modes. It also provides low level serialization methods
  that can offer more control over the serialization process. This
  class is split into multiple partial class in order to make the code
  base more manageable. It also contains sub-classes:
  - 'Buffer' is an helper type that encapsulate a byte array for the
	serialization / deserialization process. The underlying arrays are
	managed by the serializer in a pool, reducing the need for memory
	allocations. Resize of the buffer array are therefore done in a very
	efficient way. However, this means that the returned byte array after
	serialization is generally bigger than the actual size of serialized
	data. Synchronous versions of the 'Serialize' method automatically
	copy the serialized data into a new byte array of the appropriate size.
	However, the asynchronous versions does not do this automatically.
	Moreover, the internal array of the buffer will be reclamed once the
	buffer is disposed. Therefore, the user must handle the copy of the
	serialized data himself. This choice was made based on performance
	concern, to avoid unnecessary copies and allocation wherever possible.
  - 'IDMap' is an helper class that allows some sort of compression inside
	of the serialized stream. When values are often repeated, this map
	allows to serialize them as indexes, with a binary representation of
	1, 2 or 4 bytes depending on the number of index values. The map is
	streamed during serialization, with the values serializaded only once
	the first time they appear in the serialization process.
- 'Converter' contains a set of helper structs that allows cast between
  different types of identic binary size, where cast would not be
  considered as valid otherwise. It can for example convert between 'int'
  and 'float' values, without changing the byte representation. Moreover,
  those helper structs also take into account the endianness of the
  platform to provide serialized values that could be decoded on any
  platform. For efficiency issues, the values are always stored in the
  little endian form.
- 'IBinarySerializable' is an interface that objects can implement in
  order to be serializable using the binary serializer.
- 'link.xml' contains compilation related configurations to avoid that
  methods and types of the serialization namespace would be striped
  during compilation. Otherwise, compiler could optimize the generated
  assembly by removing some methods and types that could be requested
  for successful deserialization of data. On mono backend, the missing
  methods would be compiled on the fly and everything work as expected.
  However, on ill2cpp backend, missing methods results in crashes during
  deserialization. In case users serialize types or methods only
  referenced via reflection on ill2cpp backend, they will need to add
  their own 'link.xml' file in their folder to force inclusion of the
  required symbols in build.
- 'xml' is a helper class for easy XML serialization. Save an object with
  XML.Serialize(object_to_be_saved, filename) and load with
  loaded_object=XML.Deserialize<Class_of_object_to_be_loaded>(filename).
