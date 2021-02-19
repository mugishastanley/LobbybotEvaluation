using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Settings
{
	public Matrix4x4 transitionMatrix;
	
	public void Save(string filename)
	{
		XML.Serialize(this, filename);
	}

	public static void Save(Settings settings, string filename)
	{
		XML.Serialize(settings, filename);
	}

	public static Settings Load(string filename)
	{
		return XML.Deserialize<Settings>(filename);
	}
}