using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Rinsen.Gelf
{
    internal class GelfLogger : ILogger
    {
        private string _categoryName;
        private readonly IGelfPayloadQueue _gelfPayloadQueue;

        public GelfLogger(string categoryName, IGelfPayloadQueue gelfPayloadQueue)
        {
            _categoryName = categoryName;
            _gelfPayloadQueue = gelfPayloadQueue;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var gelfPayload = new GelfPayload
            {
                ShortMessage = formatter(state, exception),
                Level = GetGelfLogLevel(logLevel)
            };

            if (state is IEnumerable<KeyValuePair<string, object>> keyValuePairs)
            {
                foreach (var keyValue in keyValuePairs)
                {
                    if (keyValue.Value is string)
                    {
                        gelfPayload.AdditionalFields.TryAdd("_state_" + keyValue.Key, keyValue.Value);
                    }
                }
            }

            var current = GelfLogScope.Current;
            while (current != null)
            {
                foreach (var keyValue in current.GetScopeKeyValuePairs())
                {
                    if (keyValue.Value is string)
                    {
                        gelfPayload.AdditionalFields.TryAdd("_scope_" + keyValue.Key, keyValue.Value);
                    }
                }

                current = current.Parent;
            }

            AddExceptionInformation(exception, gelfPayload);

            _gelfPayloadQueue.AddLog(gelfPayload);
        }

        private void AddExceptionInformation(Exception? exception, GelfPayload gelfPayload, int count = 0)
        {
            if (exception != null)
            {
                gelfPayload.AdditionalFields.TryAdd($"_Exception_Message_{count}", exception.Message);

                if (exception.StackTrace is string)
                {
                    gelfPayload.AdditionalFields.TryAdd($"_Exception_StackTrace_{count}", exception.StackTrace);
                }

                count++;
                AddExceptionInformation(exception.InnerException, gelfPayload, count);
            }
        }

        private static GelfLogLevel GetGelfLogLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    return GelfLogLevel.Debug;
                case LogLevel.Information:
                    return GelfLogLevel.Information;
                case LogLevel.Warning:
                    return GelfLogLevel.Warning;
                case LogLevel.Error:
                    return GelfLogLevel.Error;
                case LogLevel.Critical:
                    return GelfLogLevel.Critical;
                default:
                    throw new Exception();
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return GelfLogScope.Push(_categoryName, state);
        }
    }
}
