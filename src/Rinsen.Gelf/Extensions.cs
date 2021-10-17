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

            services.AddSingleton(gelfOptions);
            services.AddTransient<IGelfPublisher, GelfPublisher>();
            services.AddHostedService<GelfBackgroundService>();
            services.AddTransient<GelfPayloadSerializer>();

            switch (gelfOptions.GelfTransport)
            {
                case GelfTransport.Udp:
                    services.AddTransient<IGelfTransport, UdpGelfPayload>();
                    break;
                default:
                    throw new NotSupportedException($"Transport {gelfOptions.GelfTransport} is not supported");
            }
        }

        public static void AddRinsenGelfLogger(this ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.Services.AddSingleton<IGelfPayloadQueue, GelfPayloadQueue>();
            loggingBuilder.Services.AddSingleton<ILoggerProvider, GelfLoggerProvider>();
        }

    }
}
