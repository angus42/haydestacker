using System;
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
            Console.WriteLine(config);
        }
    }
}
