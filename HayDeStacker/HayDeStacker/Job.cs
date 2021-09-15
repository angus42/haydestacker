using Quartz;
using System;
using System.Threading.Tasks;

namespace HayDeStacker
{
    class Job : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await Console.Out.WriteLineAsync($"{GetType().Name} {context.JobDetail.Key} @ {MQTT}.");
        }

        internal string MQTT { get; set; }
    }

    class OpenJob : Job { }

    class CloseJob : Job { }
}
