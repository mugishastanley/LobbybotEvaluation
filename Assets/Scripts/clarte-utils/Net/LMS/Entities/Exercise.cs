using System;

namespace CLARTE.Net.LMS.Entities
{
	[Serializable]
	public class Exercise
	{
		public long id;
		public string guid; // Guid
		public string name;
		public byte level;
		public Module module;
	}
}
