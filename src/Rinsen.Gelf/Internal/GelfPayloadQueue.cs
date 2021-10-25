using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Rinsen.Gelf
{
    internal class GelfPayloadQueue : IGelfPayloadQueue
    {
        readonly ConcurrentQueue<GelfPayload> _payloads = new ConcurrentQueue<GelfPayload>();
        private readonly GelfOptions _gelfOptions;

        public GelfPayloadQueue(GelfOptions gelfOptions)
        {
            _gelfOptions = gelfOptions;
        }

        public void AddLog(GelfPayload gelfPayload)
        {
            if (_payloads.Count > _gelfOptions.MaxQueueSize)
            {
                return;
            }

            _payloads.Enqueue(gelfPayload);
        }

        public void GetReportedGelfPayloads(List<GelfPayload> payloads)
        {
            payloads.Clear();

            if (_payloads.IsEmpty)
            {
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
        }
    }
}
