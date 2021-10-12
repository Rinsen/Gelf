using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rinsen.Gelf
{
    internal class GelfBackgroundService : BackgroundService
    {
        private readonly IGelfPayloadQueue _gelfPayloadQueue;
        private readonly GelfPayloadSerializer _gelfPayloadSerializer;
        private readonly IGelfTransport _gelfTransport;
        private readonly GelfOptions _gelfOptions;
        private readonly ILogger<GelfBackgroundService> _logger;

        public GelfBackgroundService(IGelfPayloadQueue gelfPayloadQueue,
            GelfPayloadSerializer gelfPayloadSerializer,
            IGelfTransport gelfTransport,
            GelfOptions gelfOptions,
            ILogger<GelfBackgroundService> logger)
        {
            _gelfPayloadQueue = gelfPayloadQueue;
            _gelfPayloadSerializer = gelfPayloadSerializer;
            _gelfTransport = gelfTransport;
            _gelfOptions = gelfOptions;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var gelfPayloads = new List<GelfPayload>(200);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _gelfPayloadQueue.GetReportedGelfPayloads(gelfPayloads);

                    if (gelfPayloads.Any())
                    {
                        foreach (var gelfPayload in gelfPayloads)
                        {
                            gelfPayload.Host = Environment.MachineName;

                            var serializedPayload = _gelfPayloadSerializer.Serialize(gelfPayload);

                            await _gelfTransport.Send(serializedPayload, stoppingToken);
                        }
                    }
                    else
                    {
                        await Task.Delay(_gelfOptions.TimeToSleepBetweenBatches, stoppingToken);
                    }
                }
                catch (Exception e)
                {
                    if (Debugger.IsAttached)
                    {
                        Debug.WriteLine($"{e.Message}, {e.StackTrace}");
                    }

                    await Task.Delay(20000, stoppingToken);
                }
            }
        }
    }
}
