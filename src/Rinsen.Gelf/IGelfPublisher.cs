using System.Collections.Generic;

namespace Rinsen.Gelf
{
    public interface IGelfPublisher
    {
        void Send(string shortMessage, string? fullMessage, LogLevel logLevel, Dictionary<string, object> additionalFields);
    }
}
