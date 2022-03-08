using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CLARTE.Net.LMS
{
	public static class JsonArray 
    {
		[Serializable]
		private class Wrapper<T>
		{
			public List<T> Items;
		}

		public static List<T> FromJson<T>(string json)
		{
			if(json.TrimStart().StartsWith("["))
			{
				json = string.Format("{{\"Items\":{0}}}", json);
			}

			Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);

			return wrapper.Items;
		}

		public static string ToJson<T>(List<T> array, bool prettyPrint = false)
		{
			Wrapper<T> wrapper = new Wrapper<T>();

			wrapper.Items = array;

			string json = JsonUtility.ToJson(wrapper, prettyPrint).Trim();

			int start = json.IndexOf(':');

			json = json.Substring(start + 1, json.Length - start - 2).Trim();

			if(prettyPrint)
			{
				StringBuilder builder = new StringBuilder();

				int space_count = 0;

				foreach(char c in json)
				{
					bool is_space = c == ' ';

					if(is_space)
					{
						space_count++;
					}

					if(!is_space || space_count > 4)
					{
						builder.Append(c);
					}

					if(!is_space)
					{
						space_count = 0;
					}
				}

				json = builder.ToString();
			}

			return json;
		}
	}
}
