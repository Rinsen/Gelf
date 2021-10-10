using System;

namespace Rinsen.Gelf
{
    public class GelfOptions
    {
        public int MaxQueueSize { get; set; } = 2000;

        public int ProcessingBatchSize { get; set; } = 200;

        public TimeSpan TimeToSleepBetweenBatches { get; set; } = new TimeSpan(0, 0, 5);

        public string GelfServiceHostName { get; set; } = string.Empty;

        public int GelfServicePort { get; set; }

        public GelfTransport GelfTransport { get; set; }

    }

    public enum GelfTransport
    {
        Udp = 0,
        
    }
}
