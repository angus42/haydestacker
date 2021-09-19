using MQTTnet;
using MQTTnet.Client;
using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HayDeStacker
{
    abstract class Job : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            string mqttTopic = context.JobDetail.JobDataMap.GetString("MqttTopic");
            await Console.Out.WriteLineAsync($"{GetType().Name} {context.JobDetail.Key} @ {mqttTopic}.");
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(mqttTopic)
                .WithPayload(MqttPayload)
                .Build();
            await MqttClient.PublishAsync(message, CancellationToken.None);
        }

        internal IMqttClient MqttClient { get; set; } 

        protected abstract string MqttPayload { get; }
    }

    class OpenJob : Job
    {
        protected override string MqttPayload => "ON";
    }

    class CloseJob : Job
    {
        protected override string MqttPayload => "OFF";
    }
}
