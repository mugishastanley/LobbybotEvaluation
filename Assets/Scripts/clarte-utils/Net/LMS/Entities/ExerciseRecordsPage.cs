using System;
using System.Collections.Generic;

namespace CLARTE.Net.LMS.Entities
{
    [Serializable]
    public class ExerciseRecordsPage
    {
        public ulong total;
        public ulong offset;
        public ulong count;
        public List<ExerciseRecord> records;
    }
}
