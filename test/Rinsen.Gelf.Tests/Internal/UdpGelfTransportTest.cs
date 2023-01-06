using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Rinsen.Gelf.Tests.Internal.UdpClientMock;
using Xunit;

namespace Rinsen.Gelf.Tests.Internal
{
    public class UdpGelfTransportTest
    {
        private readonly string[] _names = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };

        // Standard description https://go2docs.graylog.org/5-0/getting_in_log_data/gelf.html
        [Fact]
        public async Task WhenSendingASmallPayload_WhitOnlyOneChunk_GetTheCorrectPayload()
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
            var data = udpTestClient.GetPayload();
            
            AssertProperties(data, new Dictionary<string, ExpectedProperty> {
                { "version", new ExpectedProperty("1.1", JsonValueKind.String) },
                { "level", new ExpectedProperty("0", JsonValueKind.Number) },
                { "host", new ExpectedProperty("FakeHost", JsonValueKind.String) },
                { "short_message", new ExpectedProperty("My short message", JsonValueKind.String) },
                { "timestamp", new ExpectedProperty("123", JsonValueKind.Number) },
            });
        }

        // Standard description https://go2docs.graylog.org/5-0/getting_in_log_data/gelf.html
        [Fact]
        public async Task WhenSendingASmallPayload_WithFullMessageIncluded_GetTheCorrectPayload()
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
                Timestamp = 123,
                FullMessage = "This is the complete message"
            };

            // Act
            await udpGelfTransport.Send(gelfPayload, cancellationTokenSource.Token);

            // Asser
            var data = udpTestClient.GetPayload();

            AssertProperties(data, new Dictionary<string, ExpectedProperty> {
                { "version", new ExpectedProperty("1.1", JsonValueKind.String) },
                { "level", new ExpectedProperty("0", JsonValueKind.Number) },
                { "host", new ExpectedProperty("FakeHost", JsonValueKind.String) },
                { "short_message", new ExpectedProperty("My short message", JsonValueKind.String) },
                { "full_message", new ExpectedProperty("This is the complete message", JsonValueKind.String) },
                { "timestamp", new ExpectedProperty("123", JsonValueKind.Number) },
            });
        }

        [Fact]
        public async Task WhenSendingAPayload_WhitTwoChunks_GetTheCorrectPayload()
        {
            // Arrange
            var udpTestClient = new UdpTestClient();
            var udpGelfTransport = new UdpGelfTransport(udpTestClient);
            using var cancellationTokenSource = new CancellationTokenSource();
            var additionalFields = GetAdditionalFields(80);
            var gelfPayload = new GelfPayload
            {
                Level = GelfLogLevel.Emergency,
                Host = "FakeHost",
                ShortMessage = "My short message",
                Timestamp = 123,
                FullMessage = "This is the complete message",
                AdditionalFields = additionalFields
            };

            // Act
            await udpGelfTransport.Send(gelfPayload, cancellationTokenSource.Token);

            // Asser
            var data = udpTestClient.GetPayload();

            var propertiesToAssert = new Dictionary<string, ExpectedProperty> {
                { "version", new ExpectedProperty("1.1", JsonValueKind.String) },
                { "level", new ExpectedProperty("0", JsonValueKind.Number) },
                { "host", new ExpectedProperty("FakeHost", JsonValueKind.String) },
                { "short_message", new ExpectedProperty("My short message", JsonValueKind.String) },
                { "full_message", new ExpectedProperty("This is the complete message", JsonValueKind.String) },
                { "timestamp", new ExpectedProperty("123", JsonValueKind.Number) } };

            foreach (var additionalField in additionalFields)
            {
                propertiesToAssert.Add(additionalField.Key, new ExpectedProperty(additionalField.Value.ToString(), JsonValueKind.String));
            }

            AssertProperties(data, propertiesToAssert);
        }

        [Fact]
        public async Task WhenSendingAPayload_WhitToManyChunks_GetASilentDropOfPackage()
        {
            // Arrange
            var udpTestClient = new UdpTestClient();
            var udpGelfTransport = new UdpGelfTransport(udpTestClient);
            using var cancellationTokenSource = new CancellationTokenSource();
            var additionalFields = GetAdditionalFields(5600);
            var gelfPayload = new GelfPayload
            {
                Level = GelfLogLevel.Emergency,
                Host = "FakeHost",
                ShortMessage = "My short message",
                Timestamp = 123,
                FullMessage = "This is the complete message",
                AdditionalFields = additionalFields
            };

            // Act
            await udpGelfTransport.Send(gelfPayload, cancellationTokenSource.Token);

            // Asser
            var data = udpTestClient.GetPayload();

            Assert.Null(data);
        }

        private Dictionary<string, object> GetAdditionalFields(int count)
        {
            var additionalFields = new Dictionary<string, object>();

            for (int i = 0; i < count; i++)
            {
                if (i < 10)
                {
                    additionalFields.Add($"_{_names[i]}", $"{_names[i]}_dfsfgdfhFGHfghj fdgh fgh fhd fgghfDHUJKFGyuikugszffd sda fDF gdfgjghk fdsagfgtyu jjkjk vghjasdf gsghsdfghtyuioulkfhdgf  fgs asdfggh dfhjd fhjhjk gf");
                }
                else if (i < 100)
                {
                    additionalFields.Add($"_{_names[i/10]}{_names[i % 10]}", $"{_names[i / 10]} {_names[i % 10]} dfsfgdfhFGHfghj fdgh fgh fhd fgghfDHUJKFGyuikugszffd sda fDF gdfgjghk fdsagfgtyu jjkjk vghjasdf gsghsdfghtyuioulkfhdgf  fgs asdfggh dfhjd fhjhjk gf");
                }
                else if (i < 1000)
                {
                    additionalFields.Add($"_{_names[i/100]}{_names[i%100/10]}{_names[i%10]}", $"{_names[i / 100]} {_names[i % 100 / 10]} {_names[i % 10]} dfsfgdfhFGHfghj fdgh fgh fhd fgghfDHUJKFGyuikugszffd sda fDF gdfgjghk fdsagfgtyu jjkjk vghjasdf gsghsdfghtyuioulkfhdgf  fgs asdfggh dfhjd fhjhjk gf");
                }
                else if (i < 10000)
                {
                    additionalFields.Add($"_{_names[i/1000]}{_names[i%1000/100]}{_names[i/10%10]}{_names[i%10]}", $"{_names[i / 1000]} {_names[i % 1000 / 100]} {_names[i / 10 % 10]} {_names[i % 10]} dfsfgdfhFGHfghj fdgh fgh fhd fgghfDHUJKFGyuikugszffd sda fDF gdfgjghk fdsagfgtyu jjkjk vghjasdf gsghsdfghtyuioulkfhdgf  fgs asdfggh dfhjd fhjhjk gf");
                }
            }

            return additionalFields;
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
