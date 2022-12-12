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

        /// <inheritdoc/>>
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        /// <inheritdoc/>>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var gelfPayload = new GelfPayload
            {
                ShortMessage = formatter(state, exception),
                Level = GetGelfLogLevel(logLevel)
            };

            gelfPayload.AdditionalFields.Add("_stringLevel", logLevel.ToString());
            gelfPayload.AdditionalFields.Add("_category", _categoryName);

            AddEventInformation(eventId, gelfPayload);
            AddStateInformation(state, gelfPayload);
            AddScopeInformation(gelfPayload);
            AddExceptionInformation(exception, gelfPayload);

            _gelfPayloadQueue.AddLog(gelfPayload);
        }

        private static void AddScopeInformation(GelfPayload gelfPayload)
        {
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
        }

        private static void AddStateInformation<TState>(TState state, GelfPayload gelfPayload)
        {
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
        }

        private static void AddEventInformation(EventId eventId, GelfPayload gelfPayload)
        {
            if (eventId.Id != 0)
            {
                gelfPayload.AdditionalFields.Add("_event_id", eventId.Id);
                if (!string.IsNullOrEmpty(eventId.Name))
                {
                    gelfPayload.AdditionalFields.Add("_event_name", eventId.Name);
                }
            }
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

        /// <inheritdoc/>>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return GelfLogScope.Push(_categoryName, state);
        }
    }
}
