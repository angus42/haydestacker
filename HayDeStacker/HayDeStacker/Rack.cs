using Quartz;

namespace HayDeStacker
{
    class Rack
    {
        private readonly string Key;
        private readonly IScheduler Scheduler;
        private readonly IJobDetail OpenJob;
        private readonly IJobDetail CloseJob;

        public Rack(string key, RackConfig config, IScheduler scheduler)
        {
            Key = key;
            Scheduler = scheduler;
            OpenJob = JobBuilder.Create<OpenJob>()
                    .WithIdentity(Key, "Open")
                    .UsingJobData("MqttTopic", config.ShutterTopic)
                    .StoreDurably()
                    .Build();
            scheduler.AddJob(OpenJob, true);
            CloseJob = JobBuilder.Create<CloseJob>()
                .WithIdentity(Key, "Close")
                .UsingJobData("MqttTopic", config.ShutterTopic)
                .StoreDurably()
                .Build();
            scheduler.AddJob(CloseJob, true);
        }

        public void AddTrigger(string key, ScheduleConfig config)
        {
            var trigger = TriggerBuilder.Create()
                .WithIdentity(Key + '@' + key, "Open")
                .StartNow()
                .WithDailyTimeIntervalSchedule(x => x
                    .OnDaysOfTheWeek(config.Days)
                    .StartingDailyAt(new TimeOfDay(config.StartTime.Hours, config.StartTime.Minutes, config.StartTime.Seconds))
                    .EndingDailyAfterCount(1)
                    .WithMisfireHandlingInstructionDoNothing())
                .ForJob(OpenJob)
                .Build();
            Scheduler.ScheduleJob(trigger);

            var endTime = config.StartTime + config.Duration;
            trigger = TriggerBuilder.Create()
                .WithIdentity(Key + '@' + key, "Close")
                .StartNow()
                .WithDailyTimeIntervalSchedule(x => x
                    .OnDaysOfTheWeek(config.Days)
                    .StartingDailyAt(new TimeOfDay(endTime.Hours, endTime.Minutes, endTime.Seconds))
                    .EndingDailyAfterCount(1)
                    .WithMisfireHandlingInstructionDoNothing())
                .ForJob(CloseJob)
                .Build();
            Scheduler.ScheduleJob(trigger);
        }
    }
}
