using Quartz;
using Quartz.Simpl;
using Quartz.Spi;

namespace HayDeStacker
{
    class JobFactory : SimpleJobFactory
    {
        public override IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var result = base.NewJob(bundle, scheduler);
            if (result is Job job)
            {
                job.MQTT = "Test";
            }
            return result;
        }
    }
}
