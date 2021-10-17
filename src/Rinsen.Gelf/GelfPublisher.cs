using System.Collections.Generic;

namespace Rinsen.Gelf
{
    internal class GelfPublisher : IGelfPublisher
    {
        private readonly IGelfPayloadQueue _gelfPayloadQueue;

        public GelfPublisher(IGelfPayloadQueue gelfPayloadQueue)
        {
            _gelfPayloadQueue = gelfPayloadQueue;
        }

        public void Send(string shortMessage, string? fullMessage, GelfLogLevel logLevel, Dictionary<string, object> additionalFields)
        {
            var gelfPayload = new GelfPayload
            {
                ShortMessage = shortMessage,
                FullMessage = fullMessage,
                AdditionalFields = additionalFields,
                Host = string.Empty
            };

            _gelfPayloadQueue.AddLog(gelfPayload);
        }
    }
}
