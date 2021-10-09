using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Rinsen.Gelf
{
    /// <summary>
    /// Implementation of spec in https://docs.graylog.org/docs/gelf
    /// </summary>
    internal class GelfPayload
    {
        /// <summary>
        /// GELF spec version – “1.1”; MUST be set by client library.
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.1";

        /// <summary>
        /// The name of the host, source or application that sent this message; MUST be set by client library.
        /// </summary>
        [JsonPropertyName("host")]
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// Short descriptive message; MUST be set by client library.
        /// </summary>
        [JsonPropertyName("short_message")]
        public string ShortMessage { get; set; } = string.Empty;

        /// <summary>
        /// Long message that can i.e. contain a backtrace; optional.
        /// </summary>
        [JsonPropertyName("full_message")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? FullMessage { get; set; }

        /// <summary>
        /// Seconds since UNIX epoch with optional decimal places for milliseconds; SHOULD be set by client library. Will be set to the current timestamp (now) by the server if absent.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        /// <summary>
        /// Level equal to the standard syslog levels; optional, default is 1 (ALERT).
        /// </summary>
        [JsonPropertyName("level")]
        public LogLevel Level { get; set; } = LogLevel.Alert;

        /// <summary>
        /// Every field you send and prefix with an underscore ( _) will be treated as an additional field. Allowed characters in field names are any word character (letter, number, underscore), dashes and dots. The verifying regular expression is: ^[\w\.\-]*$. Libraries SHOULD not allow to send id as additional field ( _id). Graylog server nodes omit this field automatically.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> AdditionalFields { get; set; } = new Dictionary<string, object>();



    }

    /// <summary>
    /// Based on https://en.wikipedia.org/wiki/Syslog#Severity_level
    /// </summary>
    internal enum LogLevel
    {
        /// <summary>
        /// System is unusable	
        /// A panic condition.
        /// </summary>
        Emergency = 0,
        /// <summary>
        /// Action must be taken immediately	
        /// A condition that should be corrected immediately, such as a corrupted system database.
        /// </summary>
        Alert = 1,
        /// <summary>
        /// Critical conditions
        /// Hard device errors.
        /// </summary>
        Critical = 2,
        /// <summary>
        /// Error conditions	
        /// </summary>
        Error = 3,
        /// <summary>
        /// Warning conditions
        /// </summary>
        Warning = 4,
        /// <summary>
        /// Normal but significant conditions
        /// Conditions that are not error conditions, but that may require special handling.
        /// </summary>
        Notice = 5,
        /// <summary>
        /// Informational messages
        /// </summary>
        Information = 6,
        /// <summary>
        /// Debug-level messages
        /// Messages that contain information normally of use only when debugging a program.
        /// </summary>
        Debug = 7
    }
}
