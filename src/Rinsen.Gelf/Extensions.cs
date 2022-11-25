using Microsoft.Extensions.Logging;
using Rinsen.Gelf;
using Rinsen.Gelf.Internal;
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
            services.AddTransient<UdpGelfTransport>();
            services.AddTransient<TcpGelfTransport>();
            
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
