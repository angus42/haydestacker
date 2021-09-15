using System.Collections.Generic;

namespace HayDeStacker
{
    class Config
    {
        public Dictionary<string, RackConfig> Racks { get; set; }

        public Dictionary<string, ScheduleConfig> Schedules { get; set; }
    }
}
