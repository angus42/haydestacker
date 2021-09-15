using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace HayDeStacker
{
    class RackConfig
    {
        public string Name { get; set; }

        public string ShutterTopic { get; set; }

        [YamlMember(Alias = "schedules", ApplyNamingConventions = false)]
        public List<string> ScheduleKeys { get; set; }
    }
}
