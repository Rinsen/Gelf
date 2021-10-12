using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rinsen.Gelf
{
    internal class GelfLoggerProvider : ILoggerProvider
    {
        private readonly IGelfPayloadQueue _gelfPayloadQueue;

        public GelfLoggerProvider(IGelfPayloadQueue gelfPayloadQueue)
        {
            _gelfPayloadQueue = gelfPayloadQueue;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new GelfLogger(categoryName, _gelfPayloadQueue);
        }

        public void Dispose()
        {
            
        }
    }
}
