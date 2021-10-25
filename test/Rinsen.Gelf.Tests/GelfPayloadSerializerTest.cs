using System;
using System.Collections.Generic;
using Xunit;

namespace Rinsen.Gelf.Tests
{
    public class GelfPayloadSerializerTest
    {
        [Fact]
        public void TestSerialize()
        {
            var payload = new GelfPayload
            {
                Host = "host1",
                ShortMessage = "Short message",
                FullMessage = "Full message",
                Timestamp = 1633819329526,
                AdditionalFields = new Dictionary<string, object>
                {
                    { "_item_name", "Name" },
                    { "_item_age", "18" },
                }
            };

            var serializedObject = GelfPayloadSerializer.Serialize(payload);

            Assert.Equal("{\"version\":\"1.1\",\"host\":\"host1\",\"short_message\":\"Short message\",\"full_message\":\"Full message\",\"timestamp\":1633819329526,\"level\":1,\"_item_name\":\"Name\",\"_item_age\":\"18\"}", serializedObject);
        }

        [Fact]
        public void TestSerialize_WithOptionalFieldsEmpty()
        {
            var payload = new GelfPayload
            {
                Host = "host1",
                ShortMessage = "Short message",
                Timestamp = 1633819329526
            };

            var serializedObject = GelfPayloadSerializer.Serialize(payload);

            Assert.Equal("{\"version\":\"1.1\",\"host\":\"host1\",\"short_message\":\"Short message\",\"timestamp\":1633819329526,\"level\":1}", serializedObject);
        }
    }
}
