using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        private Config Config;

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Config);

            services.AddSwaggerDocument(settings =>
            {
                settings.Title = "HayDeStacker API";
                settings.Description = "Control settings and operations of your automated hay racks.";
            });

            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseMvc();
        }

        private void ReadConfig()
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var input = File.ReadAllText("haydestracker.yml");
            Config = deserializer.Deserialize<Config>(input);
            foreach (var kv in Config.Racks)
            {
                var rack = new Rack(kv.Key, kv.Value, Scheduler);

                foreach (var scheduleKey in kv.Value.ScheduleKeys)
                {
                    var scheduleConfig = Config.Schedules[scheduleKey];
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
