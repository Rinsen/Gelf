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

        [Fact]
        public async Task WhenSendingAPayload_WhitTwoChunks_GetTheCorrectPayload()
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
                FullMessage = "This is the complete message",
                AdditionalFields = new Dictionary<string, object> {
                    { "_One", "1Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_Two", "2Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_Three", "3Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_Four", "4Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_Five", "5Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_Six", "6Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_Seven", "7Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },    
                    { "_Eight", "8Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_Nine", "9Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_OneOne", "11Afdsgkognmkdfngkdnflgjkndfjkngfj dskfnmlkjds nfgksdn lksgndf gndfglknsd flkgnerjtgoejrpmnjvlknhselkjfghba dfgszzdfgs" },
                    { "_OneTwo", "12Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_OneThree", "13Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_OneFour", "14Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl§" },
                    { "_OneFive", "15Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_OneSix", "16Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_OneSeven", "17Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_OneEight", "18Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_OneNine", "19Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_TwoOne", "21Afdsgkognmkdfngkdnflgjkndfjkngfj dskfnmlkjds nfgksdn lksgndf gndfglknsd flkgnerjtgoejrpmnjvlknhselkjfghba dfgszzdfgs" },
                    { "_TwoTwo", "22Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_TwoThree", "23Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_TwoFour", "24Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl§" },
                    { "_TwoFive", "25Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_TwoSix", "26Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_TwoSeven", "27Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_TwoEight", "28Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_TwoNine", "29Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_ThreeOne", "31Afdsgkognmkdfngkdnflgjkndfjkngfj dskfnmlkjds nfgksdn lksgndf gndfglknsd flkgnerjtgoejrpmnjvlknhselkjfghba dfgszzdfgs" },
                    { "_ThreeTwo", "32Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_ThreeThree", "33Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_ThreeFour", "34Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl§" },
                    { "_ThreeFive", "35Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_ThreeSix", "36Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_ThreeSeven", "37Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_ThreeEight", "38Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_ThreeNine", "39Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_FourOne", "41Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_FourTwo", "42Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_FourThree", "43Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_FourFour", "44Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl§" },
                    { "_FourFive", "45Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_FourSix", "46Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_FourSeven", "47Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_FourEight", "48Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_FourNine", "49Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_FiveOne", "51Afdsgkognmkdfngkdnflgjkndfjkngfj dskfnmlkjds nfgksdn lksgndf gndfglknsd flkgnerjtgoejrpmnjvlknhselkjfghba dfgszzdfgs" },
                    { "_FiveTwo", "52Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_FiveThree", "53Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_FiveFour", "54Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl§" },
                    { "_FiveFive", "55Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_FiveSix", "56Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_FiveSeven", "57Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_FiveEight", "58Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_FiveNine", "59Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_SixOne", "61Afdsgkognmkdfngkdnflgjkndfjkngfj dskfnmlkjds nfgksdn lksgndf gndfglknsd flkgnerjtgoejrpmnjvlknhselkjfghba dfgszzdfgs" },
                    { "_SixTwo", "62Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_SixThree", "63Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_SixFour", "64Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl§" },
                    { "_SixFive", "65Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_SixSix", "66Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_SixSeven", "67Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_SixEight", "68Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_SixNine", "69Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_SevenOne", "71Afdsgkognmkdfngkdnflgjkndfjkngfj dskfnmlkjds nfgksdn lksgndf gndfglknsd flkgnerjtgoejrpmnjvlknhselkjfghba dfgszzdfgs" },
                    { "_SevenTwo", "72Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_SevenThree", "73Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_SevenFour", "74Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl§" },
                    { "_SevenFive", "75Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_SevenSix", "76Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_SevenSeven", "77Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_SevenEight", "78Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                    { "_SevenNine", "79Adfsgdfknknm dfsfg sdfljkngdfgn lker gjdfgjkldfj glkwjeklmsfdgklgj kerjgflkrsj lkjmsrd lk gjsdklrjmk glksjergk jsmr kl" },
                }
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
