using Microsoft.Extensions.DependencyInjection;
using Rinsen.Gelf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class Extensions
    {
        public static void AddRinsenGelf(this IServiceCollection services, Action<GelfOptions> options)
        {
            var gelfOptions = new GelfOptions();
            options.Invoke(gelfOptions);

            services.AddSingleton(gelfOptions);
            services.AddSingleton<IGelfPayloadQueue, GelfPayloadQueue>();
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

    }
}
