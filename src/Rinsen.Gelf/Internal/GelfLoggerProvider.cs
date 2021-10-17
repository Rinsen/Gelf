using Microsoft.Extensions.Logging;

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
