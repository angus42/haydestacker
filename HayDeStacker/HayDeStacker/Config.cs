using System.Collections.Generic;

namespace HayDeStacker
{
    class Config
    {
        public Dictionary<string, Rack> Racks { get; set; }

        public Dictionary<string, Schedule> Schedules { get; set; }
    }
}
