using System.IO;
using System.Xml.Serialization;

public class XML
{
	public static void Serialize(object item, string path)
	{
		XmlSerializer serializer = new XmlSerializer(item.GetType());

		StreamWriter writer = new StreamWriter(path);

		serializer.Serialize(writer.BaseStream, item);

		writer.Close();
	}

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