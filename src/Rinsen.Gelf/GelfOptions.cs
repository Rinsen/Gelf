using System;

namespace Rinsen.Gelf
{
    public class GelfOptions
    {
        /// <summary>
        /// Max queue size that will be allowed in memory. If this is reached no more messages will be appended.
        /// <p>Default is 2000.</p>
        /// </summary>
        public int MaxQueueSize { get; set; } = 2000;

        /// <summary>
        /// Processing batch size in each background service loop.
        /// <p>Default is 200.</p>
        /// </summary>
        public int ProcessingBatchSize { get; set; } = 200;

        /// <summary>
        /// Time the background service loop will sleap if there are no more batches to process. If ther is another batch that will be processed immediately.
        /// <p>Default 5 seconds</p>
        /// </summary>
        public TimeSpan TimeToSleepBetweenBatches { get; set; } = new TimeSpan(0, 0, 5);

        /// <summary>
        /// Service host name or ip address to use when connecting to gelf log service.
        /// </summary>
        public string GelfServiceHostNameOrAddress { get; set; } = string.Empty;

        /// <summary>
        /// Port to use when connecting to gelf log service.
        /// </summary>
        public int GelfServicePort { get; set; }

        /// <summary>
        /// Gelf transport type to use. Default is UDP.
        /// </summary>
        public GelfTransport GelfTransport { get; set; }

        /// <summary>
        /// Application name that will be added to all log instances.
        /// </summary>
        public string ApplicationName { get; set; } = string.Empty;
    }

    public enum GelfTransport
    {
        Udp = 0,
        Tcp = 1,

    }
}
