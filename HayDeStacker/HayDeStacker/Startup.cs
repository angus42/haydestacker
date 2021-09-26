using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Disconnecting;
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
    class Startup
    {             
        private readonly IMqttClient MqttClient;
        private readonly IMqttClientOptions MqttClientOptions;
        private readonly IScheduler Scheduler;

        public Startup()
        {
            MqttClient = new MqttFactory().CreateMqttClient()
                .UseDisconnectedHandler(MqttClientDisconnected);
            MqttClientOptions = new MqttClientOptionsBuilder()
                .WithClientId("Haydestacker")
                .WithTcpServer("localhost")
                .Build();
            MqttClient.ConnectAsync(MqttClientOptions, CancellationToken.None);

            var factory = new StdSchedulerFactory();
            Scheduler = factory.GetScheduler().Result;
            Scheduler.JobFactory = new JobFactory(MqttClient);
            Scheduler.Start();

            ReadConfig();
        }

        private void ReadConfig()
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var input = File.ReadAllText("haydestracker.yml");
            var config = deserializer.Deserialize<Config>(input); foreach (var kv in config.Racks)
            {
                var rack = new Rack(kv.Key, kv.Value, Scheduler);

                foreach (var scheduleKey in kv.Value.ScheduleKeys)
                {
                    var scheduleConfig = config.Schedules[scheduleKey];
                    rack.AddTrigger(scheduleKey, scheduleConfig);
                }
            }
        }

        private async void MqttClientDisconnected(MqttClientDisconnectedEventArgs e)
        {
            Console.WriteLine("### DISCONNECTED FROM SERVER ###");
            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await MqttClient.ConnectAsync(MqttClientOptions, CancellationToken.None);
            }
            catch
            {
                Console.WriteLine("### RECONNECTING FAILED ###");
            }
        }
    }
}
