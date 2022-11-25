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
        private readonly IEnumerable<IGelfTransport> _gelfTransports;
        private readonly GelfOptions _gelfOptions;
        private readonly ILogger<GelfBackgroundService> _logger;

        public GelfBackgroundService(IGelfPayloadQueue gelfPayloadQueue,
            IEnumerable<IGelfTransport> gelfTransports,
            GelfOptions gelfOptions,
            ILogger<GelfBackgroundService> logger)
        {
            _gelfPayloadQueue = gelfPayloadQueue;
            _gelfTransports = gelfTransports;
            _gelfOptions = gelfOptions;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var gelfPayloads = new List<GelfPayload>(200);

            var gelfTransport = _gelfTransports.Single(m => m.TransportType == _gelfOptions.GelfTransport);

            while (!cancellationToken.IsCancellationRequested)
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

                            await gelfTransport.Send(gelfPayload, cancellationToken);
                        }
                    }
                    else
                    {
                        await Task.Delay(_gelfOptions.TimeToSleepBetweenBatches, cancellationToken);
                    }
                }
                catch (Exception e)
                {
                    if (Debugger.IsAttached)
                    {
                        Debug.WriteLine($"{e.Message}, {e.StackTrace}");
                    }

                    _logger.LogError(e, "Failed to send GELF message");

                    await Task.Delay(20000, cancellationToken);
                }
            }
        }
    }
}
