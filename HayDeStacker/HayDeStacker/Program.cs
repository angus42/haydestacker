using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace HayDeStacker
{
    class Program
    {
        static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5000/")
                .Build()
                .Run();
        }
    }
}
