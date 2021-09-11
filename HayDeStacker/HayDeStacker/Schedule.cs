using System;
using System.Collections.Generic;

namespace HayDeStacker
{
    class Schedule
    {
        public TimeSpan StartTime { get; set; }

        public TimeSpan Duration { get; set; }

        public List<Day> Days { get; set; }


        public enum Day
        {
            Mo, Tu, We, Th, Fr, Sa, Su
        }
    }
}
