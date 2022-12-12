using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                    services.AddRinsenGelf(options =>
                    {
                        options.GelfServiceHostNameOrAddress = "graylog.rinsen.se";
                        options.GelfServicePort = 12202;
                        options.GelfTransport = Rinsen.Gelf.GelfTransport.Udp;
                    });

                    services.AddHostedService<Worker>();

                    services.AddLogging(options =>
                    {
                        options.AddRinsenGelfLogger();
                    });
                });
    }
}
