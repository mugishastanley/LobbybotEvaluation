using System;

namespace CLARTE.Net.LMS.Entities
{
    [Serializable]
    public class ExerciseRecord
    {
        public long id;
        public long user;
        public long exercise;
        public long date; // In ticks, i.e. the number of 100-nanosecond intervals that have elapsed since January 1, 0001 at 00:00:00.000 in the Gregorian calendar UTC.
        public uint duration; // In seconds
        public float grade;
        public bool success;
        public uint nbChallengesValidated;
        public string debriefData; // Base64 encoded byte[]
    }
}
