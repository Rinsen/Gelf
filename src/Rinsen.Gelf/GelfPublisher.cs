using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rinsen.Gelf
{
    internal class GelfPublisher : IGelfPublisher
    {
        private readonly IGelfPayloadQueue _gelfPayloadQueue;
        //private readonly IConfiguration _configuration;

        public GelfPublisher(IGelfPayloadQueue gelfPayloadQueue)
        //    IConfiguration configuration)
        {
            _gelfPayloadQueue = gelfPayloadQueue;
            //_configuration = configuration;
        }

        public void Send(string shortMessage, string? fullMessage, GelfLogLevel logLevel, Dictionary<string, object> additionalFields)
        {
            var gelfPayload = new GelfPayload
            {
                ShortMessage = shortMessage,
                FullMessage = fullMessage,
                AdditionalFields = additionalFields,
                Host = "TestHost"
            };

            _gelfPayloadQueue.AddLog(gelfPayload);
        }
    }
}
