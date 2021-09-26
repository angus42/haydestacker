using System;

namespace HayDeStacker
{
    class Program
    {
        static void Main(string[] args)
        {
            new Startup();

            // var config = new HttpSelfHostConfiguration("http://localhost:4242");
            // 
            // config.Routes.MapHttpRoute(
            //     "API Default", "api/{controller}/{id}",
            //     new { id = RouteParameter.Optional });
            // 
            // using (HttpSelfHostServer server = new HttpSelfHostServer(config))
            // {
            // 
            //     server.OpenAsync().Wait();
                Console.ReadLine();
            // }
        }
    }
}
