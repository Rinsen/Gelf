using System.Collections.Generic;

namespace Rinsen.Gelf
{
    public interface IGelfPublisher
    {
        void Send(string shortMessage, string? fullMessage, GelfLogLevel logLevel, Dictionary<string, object> additionalFields);
    }
}
