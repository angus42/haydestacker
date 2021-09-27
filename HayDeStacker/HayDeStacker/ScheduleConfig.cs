using System;
using System.Collections.Generic;

namespace HayDeStacker
{
    public class ScheduleConfig
    {
        public TimeSpan StartTime { get; set; }

        public TimeSpan Duration { get; set; }

        public List<DayOfWeek> Days { get; set; }
    }
}
