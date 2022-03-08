using System;

namespace CLARTE.Net.LMS.Content
{
	public abstract class Exercise<T> : Base where T : Enum
	{
		#region Members
		public Module module;
		public string title;
		public T level;
		#endregion
	}
}
