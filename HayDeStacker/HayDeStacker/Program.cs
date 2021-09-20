using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Quartz;
using Quartz.Impl;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HayDeStacker
{
    class Program
    {
        static void Main(string[] args)
        {
            var mqttClient = new MqttFactory().CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                .WithClientId("Haydestacker")
                .WithTcpServer("localhost")
                .Build();
            mqttClient.UseDisconnectedHandler(async e =>
            {
                Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await mqttClient.ConnectAsync(options, CancellationToken.None);
                }
                catch
                {
                    Console.WriteLine("### RECONNECTING FAILED ###");
                }
            });

            mqttClient.ConnectAsync(options, CancellationToken.None);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var input = File.ReadAllText("haydestracker.yml");
            var config = deserializer.Deserialize<Config>(input);

            var factory = new StdSchedulerFactory();

            IScheduler scheduler = factory.GetScheduler().Result;
            scheduler.JobFactory = new JobFactory(mqttClient);
            scheduler.Start();

            foreach (var kv in config.Racks)
            {
                var openJob = JobBuilder.Create<OpenJob>()
                    .WithIdentity(kv.Key, "Open")
                    .UsingJobData("MqttTopic", kv.Value.ShutterTopic)
                    .StoreDurably()
                    .Build();
                scheduler.AddJob(openJob, true);
                var closeJob = JobBuilder.Create<CloseJob>()
                    .WithIdentity(kv.Key, "Close")
                    .UsingJobData("MqttTopic", kv.Value.ShutterTopic)
                    .StoreDurably()
                    .Build();
                scheduler.AddJob(closeJob, true);

                foreach (var scheduleKey in kv.Value.ScheduleKeys)
                {
                    var schedule = config.Schedules[scheduleKey];
                    var trigger = TriggerBuilder.Create()
                        .WithIdentity(kv.Key + '@' + scheduleKey, "Open")
                        .StartNow()
                        .WithDailyTimeIntervalSchedule(x => x
                            .OnDaysOfTheWeek(schedule.Days)
                            .StartingDailyAt(new TimeOfDay(schedule.StartTime.Hours, schedule.StartTime.Minutes, schedule.StartTime.Seconds))
                            .EndingDailyAfterCount(1)
                            .WithMisfireHandlingInstructionDoNothing())
                        .ForJob(openJob)
                        .Build();
                    scheduler.ScheduleJob(trigger);

                    var endTime = schedule.StartTime + schedule.Duration;
                    trigger = TriggerBuilder.Create()
                        .WithIdentity(kv.Key + '@' + scheduleKey, "Close")
                        .StartNow()
                        .WithDailyTimeIntervalSchedule(x => x
                            .OnDaysOfTheWeek(schedule.Days)
                            .StartingDailyAt(new TimeOfDay(endTime.Hours, endTime.Minutes, endTime.Seconds))
                            .EndingDailyAfterCount(1)
                            .WithMisfireHandlingInstructionDoNothing())
                        .ForJob(closeJob)
                        .Build();
                    scheduler.ScheduleJob(trigger);
                }
            }

            Console.ReadLine();
        }
    }
}
