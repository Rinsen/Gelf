using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ConsoleSample
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddRinsenGelfConsole(options =>
                    {
                        options.GelfServiceHostName = "";
                        options.GelfServicePort = 12201;
                    });

                    services.AddHostedService<Worker>();
                });
    }
}
