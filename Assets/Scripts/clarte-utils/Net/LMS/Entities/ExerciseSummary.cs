using System;

namespace CLARTE.Net.LMS.Entities
{
	[Serializable]
	public class ExerciseSummary
	{
		public long user;
		public long exercise;
		public uint nbCompletions;
		public float gradeMax;
	}
}
