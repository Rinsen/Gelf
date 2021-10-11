using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Rinsen.Gelf
{
    internal class GelfPayloadQueue : IGelfPayloadQueue
    {
        readonly ConcurrentQueue<GelfPayload> _payloads = new ConcurrentQueue<GelfPayload>();
        private readonly GelfOptions _gelfOptions;
        private readonly ILogger<GelfPayload> _logger;

        public GelfPayloadQueue(GelfOptions gelfOptions,
            ILogger<GelfPayload> logger)
        {
            _gelfOptions = gelfOptions;
            _logger = logger;
        }

        public void AddLog(GelfPayload gelfPayload)
        {
            if (_payloads.Count > _gelfOptions.MaxQueueSize)
            {
                _logger.LogDebug("Failed to enqueue gelf payload because max queue capacity is reached");

                return;
            }

            _payloads.Enqueue(gelfPayload);
        }

        public void GetReportedGelfPayloads(List<GelfPayload> payloads)
        {
            payloads.Clear();

            if (_payloads.IsEmpty)
            {
                if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Trace))
                {
                    _logger.LogTrace("No payloads to forward");
                }

                return;
            }

            int logCount = _payloads.Count;
            var resultSize = logCount < payloads.Capacity ? logCount : payloads.Capacity;

            for (int i = 0; i < resultSize; i++)
            {
                if (_payloads.TryDequeue(out var payload))
                {
                    payloads.Add(payload);
                }
                else
                {
                    break;
                }
            }

            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Trace))
            {
                _logger.LogTrace($"Dequeued {payloads.Count} payloads");
            }
        }
    }
}
