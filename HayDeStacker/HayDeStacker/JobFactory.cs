using MQTTnet.Client;
using Quartz;
using Quartz.Simpl;
using Quartz.Spi;

namespace HayDeStacker
{
    class JobFactory : SimpleJobFactory
    {
        private readonly IMqttClient MqttClient;

        public JobFactory(IMqttClient mqttClient)
        {
            MqttClient = mqttClient;
        }

        public override IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var result = base.NewJob(bundle, scheduler);
            if (result is Job job)
            {
                job.MqttClient = MqttClient;
            }
            return result;
        }
    }
}
