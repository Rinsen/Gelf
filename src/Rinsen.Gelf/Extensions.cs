using Microsoft.Extensions.Logging;
using Rinsen.Gelf;
using Rinsen.Gelf.Internal;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class Extensions
    {
        /// <summary>
        /// Add background service with UDP and TCP transports for web applications.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="options">The <see cref="GelfOptions"/> of the service.</param>
        public static void AddRinsenGelf(this IServiceCollection services, Action<GelfOptions> options)
        {
            var gelfOptions = new GelfOptions();
            options.Invoke(gelfOptions);

            services.AddSingleton(gelfOptions);
            services.AddTransient<IGelfPublisher, GelfPublisher>();
            services.AddHostedService<GelfBackgroundService>();
            services.AddTransient<GelfPayloadSerializer>();
            services.AddTransient<IGelfTransport, UdpGelfTransport>();
            services.AddTransient<IGelfTransport, TcpGelfTransport>();
            services.AddTransient<IUdpClient, UdpClientWrapper>();
        }

        /// <summary>
        /// Adds a GELF logger to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        public static void AddRinsenGelfLogger(this ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.Services.AddSingleton<IGelfPayloadQueue, GelfPayloadQueue>();
            loggingBuilder.Services.AddSingleton<ILoggerProvider, GelfLoggerProvider>();
        }

    }
}
