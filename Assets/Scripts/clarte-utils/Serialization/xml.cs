using System.IO;
using System.Xml.Serialization;

namespace CLARTE.Serialization
{
	/// <summary>
	/// Methods dor serializing and deserializing objects into an XML file.
	/// </summary>
	public class XML
	{
		/// <summary>
		/// Serialize an object into an XML file
		/// </summary>
		/// <param name="item">Object to be serialized</param>
		/// <param name="path">Path of the XML file to be created</param>
		public static void Serialize(object item, string path)
		{
			XmlSerializer serializer = new XmlSerializer(item.GetType());

			StreamWriter writer = new StreamWriter(path);

			serializer.Serialize(writer.BaseStream, item);

			writer.Close();
		}

		/// <summary>
		/// Deserialize an object from an XML file
		/// </summary>
		/// <typeparam name="T">Type of the objet to be deserialized</typeparam>
		/// <param name="path">Path to the XML file to be parsed</param>
		/// <returns>Created object</returns>
		public static T Deserialize<T>(string path)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(T));

			if(File.Exists(path))
			{
				StreamReader reader = new StreamReader(path);

				T deserialized = (T)serializer.Deserialize(reader.BaseStream);

				reader.Close();

				return deserialized;
			}
			else
			{
				return default(T);
			}
		}
	}
}