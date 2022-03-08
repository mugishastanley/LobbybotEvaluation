using System;

namespace CLARTE.Net.LMS.Entities
{
	[Serializable]
	public class User
	{
		public long id;
		public string username;
		public string firstName;
		public string lastName;
		public bool isTrainer;
		public string token;
		public Group group;
		public Organization organization;
	}
}
