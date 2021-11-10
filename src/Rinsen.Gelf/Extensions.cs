using Microsoft.Extensions.Logging;
using Rinsen.Gelf;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class Extensions
    {
        public static void AddRinsenGelf(this IServiceCollection services, Action<GelfOptions> options)
        {
            var gelfOptions = new GelfOptions();
            options.Invoke(gelfOptions);

            ValidateGelfOptions(gelfOptions);

            services.AddSingleton(gelfOptions);
            services.AddTransient<IGelfPublisher, GelfPublisher>();
            services.AddHostedService<GelfBackgroundService>();
            services.AddTransient<GelfPayloadSerializer>();

            switch (gelfOptions.GelfTransport)
            {
                case GelfTransport.Udp:
                    services.AddTransient<IGelfTransport, UdpGelfPayloadTransport>();
                    break;
                default:
                    throw new NotSupportedException($"Transport {gelfOptions.GelfTransport} is not supported");
            }
        }

        private static void ValidateGelfOptions(GelfOptions gelfOptions)
        {
            if (string.IsNullOrEmpty(gelfOptions.GelfServiceHostName))
            {
                throw new Exception("GelfServiceHostName is null or empty");
            }

            if (string.IsNullOrEmpty(gelfOptions.ApplicationName))
            {
                throw new Exception("ApplicationName is null or empty");
            }
        }

        public static void AddRinsenGelfConsole(this IServiceCollection services, Action<GelfOptions> options)
        {
            services.AddSingleton<IGelfPayloadQueue, GelfPayloadQueue>();
            services.AddRinsenGelf(options);        
        }

        public static void AddRinsenGelfLogger(this ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.Services.AddSingleton<IGelfPayloadQueue, GelfPayloadQueue>();
            loggingBuilder.Services.AddSingleton<ILoggerProvider, GelfLoggerProvider>();
        }

    }
}
