using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Rinsen.Gelf.Internal;
using static Rinsen.Gelf.UdpGelfTransport;

namespace Rinsen.Gelf.Tests.Internal.UdpClientMock
{
    internal class UdpTestClient : IUdpClient
    {
        private readonly List<UdpDatagram> _udpDatagrams= new();
        public void Dispose()
        {
            // nothing to dispose
        }

        public Task<int> SendAsync(byte[] datagram, int bytes)
        {
            _udpDatagrams.Add(new UdpDatagram(datagram, bytes, _udpDatagrams.Count));

            return Task.FromResult(0);
        }

        public JsonDocument GetPayload()
        {
            if (_udpDatagrams.Count == 1)
            {
                var data = Encoding.UTF8.GetString(_udpDatagrams.First().Datagram);

                var result = JsonSerializer.Deserialize<JsonDocument>(data);

                return result;
            }

            var firstMessageDatagram = _udpDatagrams.Single(m => m.Order == 0).Datagram.AsSpan();
            var firstMessageId = firstMessageDatagram.Slice(2, 8);
            var firstSequenceCount = firstMessageDatagram[11];

            if (firstSequenceCount != _udpDatagrams.Count)
            {
                throw new Exception($"Wrong number of chunks, expected {firstSequenceCount} but recieved {_udpDatagrams.Count}");
            }

            var messageLength = 0;
            foreach (var udpDatagram in _udpDatagrams.OrderBy(m => m.Order))
            {
                if (udpDatagram.Datagram[GelfChunkHeader.MagicByte1] != GelfChunkHeader.MagicByte1Value || 
                    udpDatagram.Datagram[GelfChunkHeader.MagicByte2] != GelfChunkHeader.MagicByte2Value)
                {
                    throw new Exception("Error in magic bytes");
                }

                var datagramSpan = udpDatagram.Datagram.AsSpan();

                var messageId = datagramSpan.Slice(2, 8);

                if (!firstMessageId.SequenceEqual(messageId))
                {
                    throw new Exception("Message id is not the same in chunk");
                }

                var sequenseNumber = datagramSpan[10]; // The sequence number of this chunk starts at 0 and is always less than the sequence count.
                if (sequenseNumber != udpDatagram.Order)
                {
                    throw new Exception("Message order is not correct");
                }

                var sequenseCount = datagramSpan[11]; // Total number of chunks in this message.
                if (sequenseCount != firstSequenceCount)
                {
                    throw new Exception("Message chunk length is not correct");
                }

                messageLength += udpDatagram.ByteSize;
            }

            var buffer = new byte[messageLength];

            var count = 0;
            foreach (var udpDatagram in _udpDatagrams.OrderBy(m => m.Order))
            {
                var length = udpDatagram.Datagram.Length - 12;

                Array.Copy(udpDatagram.Datagram, 12, buffer, 8192 * count, length);
            }

            var chunkedData = Encoding.UTF8.GetString(buffer);

            var chunkedResult = JsonSerializer.Deserialize<JsonDocument>(chunkedData);

            return chunkedResult;
        }
    }
}
