using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HayDeStacker
{
    class Program
    {
        static void Main(string[] args)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var input = File.ReadAllText("haydestracker.yml");
            var config = deserializer.Deserialize<Config>(input);

            StdSchedulerFactory factory = new StdSchedulerFactory();

            IScheduler scheduler = factory.GetScheduler().Result;
            scheduler.JobFactory = new JobFactory();
            scheduler.Start();

            foreach (var kv in config.Racks)
            {
                var openJob = JobBuilder.Create<OpenJob>()
                    .WithIdentity(kv.Key, "Open")
                    .Build();
                var closeJob = JobBuilder.Create<CloseJob>()
                    .WithIdentity(kv.Key, "Close")
                    .Build();

                foreach (var scheduleKey in kv.Value.ScheduleKeys)
                {
                    var schedule = config.Schedules[scheduleKey];
                    var trigger = TriggerBuilder.Create()
                        .WithIdentity(kv.Key, "Open")
                        .StartNow()
                        .WithDailyTimeIntervalSchedule(x => x
                            .OnDaysOfTheWeek(schedule.Days)
                            .StartingDailyAt(new TimeOfDay(schedule.StartTime.Hours, schedule.StartTime.Minutes, schedule.StartTime.Seconds)))
                        .Build();
                    scheduler.ScheduleJob(openJob, trigger);

                    var endTime = schedule.StartTime + schedule.Duration;
                    trigger = TriggerBuilder.Create()
                        .WithIdentity(kv.Key, "Close")
                        .StartNow()
                        .WithDailyTimeIntervalSchedule(x => x
                            .OnDaysOfTheWeek(schedule.Days)
                            .StartingDailyAt(new TimeOfDay(endTime.Hours, endTime.Minutes, endTime.Seconds)))
                        .Build();
                    scheduler.ScheduleJob(closeJob, trigger);
                }
            }

            Console.ReadLine();
        }
    }
}
