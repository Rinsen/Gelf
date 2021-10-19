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
        private readonly IGelfTransport _gelfTransport;
        private readonly GelfOptions _gelfOptions;
        private readonly ILogger<GelfBackgroundService> _logger;

        public GelfBackgroundService(IGelfPayloadQueue gelfPayloadQueue,
            IGelfTransport gelfTransport,
            GelfOptions gelfOptions,
            ILogger<GelfBackgroundService> logger)
        {
            _gelfPayloadQueue = gelfPayloadQueue;
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
                            if (!string.IsNullOrEmpty(_gelfOptions.ApplicationName))
                            {
                                gelfPayload.AdditionalFields.TryAdd("_application_name", _gelfOptions.ApplicationName);
                            }

                            await _gelfTransport.Send(gelfPayload, stoppingToken);
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
