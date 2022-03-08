using System;

namespace CLARTE.Net.LMS.Entities
{
	[Serializable]
	public class Organization
	{
		public long id;
		public string key;
		public string name;
		public long licenseExpiration; // In ticks, i.e. the number of 100-nanosecond intervals that have elapsed since January 1, 0001 at 00:00:00.000 in the Gregorian calendar UTC.
	}
}
