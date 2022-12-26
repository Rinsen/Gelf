using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Rinsen.Gelf.Tests.Internal.UdpClientMock;
using Xunit;

namespace Rinsen.Gelf.Tests.Internal
{
    public class UdpGelfTransportTest
    {
        [Fact]
        public async Task WhenSendingASmallPayload_GetTheCorrectEncoding()
        {
            // Arrange
            var udpTestClient = new UdpTestClient();
            var udpGelfTransport = new UdpGelfTransport(udpTestClient);
            using var cancellationTokenSource = new CancellationTokenSource();
            var gelfPayload = new GelfPayload
            {
                Level = GelfLogLevel.Emergency,
                Host = "FakeHost",
                ShortMessage = "My short message",
                Timestamp = 123
            };

            // Act
            await udpGelfTransport.Send(gelfPayload, cancellationTokenSource.Token);

            // Asser
            var data = udpTestClient.GetData();
            
            AssertProperties(data, new Dictionary<string, ExpectedProperty> {
                { "version", new ExpectedProperty("1.1", JsonValueKind.String) },
                { "level", new ExpectedProperty("0", JsonValueKind.Number) },
                { "host", new ExpectedProperty("FakeHost", JsonValueKind.String) },
                { "short_message", new ExpectedProperty("My short message", JsonValueKind.String) },
                { "timestamp", new ExpectedProperty("123", JsonValueKind.Number) },
            });
        }

        private static void AssertProperties(JsonDocument data, Dictionary<string, ExpectedProperty> expectedProperties)
        {
            Assert.NotNull(data);

            var count = 0;

            foreach (var property in data.RootElement.EnumerateObject())
            {
                if (expectedProperties.TryGetValue(property.Name, out var experctedProperty))
                {
                    Assert.Equal(experctedProperty.ValueKind, property.Value.ValueKind);

                    switch (property.Value.ValueKind)
                    {
                        case JsonValueKind.Undefined:
                            throw new NotSupportedException();
                        case JsonValueKind.Object:
                            throw new NotSupportedException();
                        case JsonValueKind.Array:
                            throw new NotSupportedException();
                        case JsonValueKind.String:
                            Assert.Equal(experctedProperty.Value, property.Value.GetString());
                            break;
                        case JsonValueKind.Number:
                            Assert.Equal(experctedProperty.Value, property.Value.GetInt32().ToString());
                            break;
                        case JsonValueKind.True:
                            throw new NotSupportedException();
                        case JsonValueKind.False:
                            throw new NotSupportedException();
                        case JsonValueKind.Null:
                            throw new NotSupportedException();
                        default:
                            throw new NotSupportedException();
                    }

                    experctedProperty.SetExists(property.Name);
                }
                else
                {
                    Assert.Fail($"Could not find property '{property.Name}'");
                }
                
                count++;
            }

            Assert.Equal(count, expectedProperties.Count);
        }

        internal class ExpectedProperty
        {
            public ExpectedProperty(string value, JsonValueKind valueKind)
            {
                Value = value;
                ValueKind = valueKind;
            }

            public string Value { get; }

            public JsonValueKind ValueKind { get; set; }

            public bool Exists { get; private set; }

            internal void SetExists(string name)
            {
                if (Exists)
                {
                    Assert.Fail($"Property '{name}' exists more than once");
                }

                Exists = true;
            }
        }
    }
}
