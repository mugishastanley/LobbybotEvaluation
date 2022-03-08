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
		/// <summary>
		/// Helper class for compressed data saved as indexed values.
		/// </summary>
		/// <remarks>The actual values are automatically saved in the stream, interlaced with the other content.</remarks>
		/// <typeparam name="T">The type of values to save.</typeparam>
		public class IDMap<T>
		{
			#region Members
			protected Binary serializer;
			protected Serializer serializerCallback;
			protected Deserializer deserializerCallback;
			protected Dictionary<T, uint> ids;
			protected List<T> values;
			protected uint next;
			#endregion

			#region Delegates
			public delegate uint Serializer(Binary serializer, ref Buffer buffer, uint start, T value);
			public delegate uint Deserializer(Binary serializer, Buffer buffer, uint start, out T value);
			#endregion

			#region Constructors
			/// <summary>
			/// Create a new map.
			/// </summary>
			/// <param name="serializer">The serializer used by this mapping.</param>
			/// <param name="serializer_callback">Callback to serialize type T when required.</param>
			/// <param name="deserializer_callback">Callback to deserialize type T when required.</param>
			public IDMap(Binary serializer, Serializer serializer_callback, Deserializer deserializer_callback)
			{
				this.serializer = serializer;
				serializerCallback = serializer_callback;
				deserializerCallback = deserializer_callback;
				ids = new Dictionary<T, uint>();
				values = new List<T>();
				values.Add(default(T));
				next = 1;
			}
			#endregion

			#region Serialization methods
			/// <summary>
			/// Deserialize a compressed T value, stored in a mapping table.
			/// </summary>
			/// <param name="buffer">The buffer where to get the data from.</param>
			/// <param name="start">Start index of the data in the buffer.</param>
			/// <param name="value">The value to read in the buffer.</param>
			/// <returns></returns>
			public uint FromBytes(Buffer buffer, uint start, out T value)
			{
				uint read;
				uint id;

				if(next <= byte.MaxValue)
				{
					byte b;

					read = serializer.FromBytes(buffer, start, out b);

					id = b;
				}
				else if(next <= ushort.MaxValue)
				{
					ushort us;

					read = serializer.FromBytes(buffer, start, out us);

					id = us;
				}
				else
				{
					read = serializer.FromBytes(buffer, start, out id);
				}

				if(id < values.Count)
				{
					value = values[(int) id];
				}
				else if(id == values.Count)
				{
					values.Add(default(T));
					next = id + 1;

					read += deserializerCallback(serializer, buffer, start + read, out value);

					ids.Add(value, id);
					values[(int) id] = value;
				}
				else
				{
					throw new IndexOutOfRangeException(string.Format("Invalid index '{0}'. Some indexes are missing before this one.", id));
				}

				return read;
			}

			/// <summary>
			/// Serialize a T value in a compressed form, using a mapping table.
			/// </summary>
			/// <param name="buffer">The buffer where to store the data.</param>
			/// <param name="start">Start index where to store the data in the buffer.</param>
			/// <param name="value">The value to write in the buffer.</param>
			/// <returns></returns>
			public uint ToBytes(ref Buffer buffer, uint start, T value)
			{
				uint written;
				uint id;
				bool new_id = false;

				if(value == null)
				{
					id = 0;
				}
				else if(!ids.TryGetValue(value, out id))
				{
					id = next++;

					if(id > int.MaxValue)
					{
						throw new IndexOutOfRangeException(string.Format("Index '{0}' in ID mapping is superior to the maximal supported index.", id));
					}

					ids.Add(value, id);
					values.Add(value);

					new_id = true;
				}

				int offset = (new_id ? 1 : 0);

				if(next <= byte.MaxValue - offset)
				{
					written = serializer.ToBytes(ref buffer, start, (byte) id);
				}
				else if(next <= ushort.MaxValue - offset)
				{
					written = serializer.ToBytes(ref buffer, start, (ushort) id);
				}
				else
				{
					written = serializer.ToBytes(ref buffer, start, id);
				}

				if(new_id)
				{
					written += serializerCallback(serializer, ref buffer, start + written, value);
				}

				return written;
			}
			#endregion
		}
	}
}
