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
                        options.GelfServiceHostNameOrAddress = "127.0.0.1";
                        options.GelfServicePort = 30761;
                        options.GelfTransport = Rinsen.Gelf.GelfTransport.Udp;
                    });

                    services.AddHostedService<Worker>();
                });
    }
}
