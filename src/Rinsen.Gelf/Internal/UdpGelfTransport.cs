﻿using Rinsen.Gelf.Internal;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rinsen.Gelf
{
    internal class UdpGelfTransport : IGelfTransport
    {
        private readonly IUdpClient _udpClient;

        public GelfTransport TransportType => GelfTransport.Udp;

        public UdpGelfTransport(IUdpClient udpClient)
        {
            _udpClient = udpClient;
        }

        public async Task Send(GelfPayload gelfPayload, CancellationToken stoppingToken)
        {
            var serializedPayload = GelfPayloadSerializer.Serialize(gelfPayload);
            
            byte[] sendbuf = Encoding.UTF8.GetBytes(serializedPayload);

            // UDP datagrams are limited to a size of 65536 bytes. Some Graylog components are limited to processingup to 8192 bytes. 
            // https://docs.graylog.org/docs/gelf

            // UdpClient docs https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.udpclient?view=net-5.0

            if (sendbuf.Length > GelfChunk.TotalSize)
            {
                await SendChunkedMessages(sendbuf);
            }
            else
            {
                await _udpClient.SendAsync(sendbuf, sendbuf.Length);
            }
        }

        private async Task SendChunkedMessages(byte[] sendbuf)
        {
            var totalMessageChunksCount = GetMessageChunkCount(sendbuf);

            // If count is larger than 128 we silently drop this package for now, maybe we should trunkate away all additional fields and replace with error value?
            if (totalMessageChunksCount > GelfChunk.MaxCount)
            {
                return;
            }

            var chunkedSendBuffer = new byte[GelfChunk.TotalSize];
            var messageId = GetMessageId();

            chunkedSendBuffer[GelfChunkHeader.MagicByte1] = GelfChunkHeader.MagicByte1Value;
            chunkedSendBuffer[GelfChunkHeader.MagicByte2] = GelfChunkHeader.MagicByte2Value;
            Array.Copy(messageId, 0, chunkedSendBuffer, GelfChunkHeader.MessageIdBeginning, GelfChunkHeader.MessageIdLength);
            
            chunkedSendBuffer[GelfChunkHeader.SequenceCount] = Convert.ToByte(totalMessageChunksCount);

            for (int i = 0; i < totalMessageChunksCount - 1; i++)
            {
                chunkedSendBuffer[GelfChunkHeader.SequenseNumber] = Convert.ToByte(i);
                var offsetInSourceArray = i * GelfChunk.PayloadSize;
                Array.Copy(sendbuf, offsetInSourceArray, chunkedSendBuffer, GelfChunkHeader.MessageStart, GelfChunk.PayloadSize);
                
                await _udpClient.SendAsync(chunkedSendBuffer, chunkedSendBuffer.Length);
            }

            var remainingLength = sendbuf.Length - (totalMessageChunksCount - 1) * GelfChunk.PayloadSize;
            
            var lastChunkSendBuffer = new byte[GelfChunkHeader.HeaderLength + remainingLength];
            Array.Copy(chunkedSendBuffer, lastChunkSendBuffer, GelfChunkHeader.HeaderLength);

            var lastOffsetInSourceArray = (totalMessageChunksCount - 1) * GelfChunk.PayloadSize;
            lastChunkSendBuffer[GelfChunkHeader.SequenseNumber] = Convert.ToByte(totalMessageChunksCount - 1);
            Array.Copy(sendbuf, lastOffsetInSourceArray, lastChunkSendBuffer, GelfChunkHeader.MessageStart, remainingLength);

            await _udpClient.SendAsync(lastChunkSendBuffer, lastChunkSendBuffer.Length);
        }

        private static int GetMessageChunkCount(byte[] sendbuf)
        {
            var chunkCount = Math.DivRem(sendbuf.Length, GelfChunk.PayloadSize, out var reminder);

            if (reminder > 0)
            {
                chunkCount++;
            }

            return chunkCount;
        }

        private static byte[] GetMessageId()
        {
            var machineNameLength = Environment.MachineName.Length - 4;
            var unixTime = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            var unixTimeLength = unixTime.Length - 4;
            var messageId = Environment.MachineName[machineNameLength..] + unixTime[unixTimeLength..];

            return Encoding.UTF8.GetBytes(messageId);
        }

        internal static class GelfChunkHeader
        {
            public const int MagicByte1 = 0;
            public const byte MagicByte1Value = 0x1e;
            public const int MagicByte2 = 1;
            public const byte MagicByte2Value = 0x0f;
            public const int MessageIdBeginning = 2;
            public const int MessageIdLength = 8;
            public const int MessageIdEnd = 9;
            public const int SequenseNumber = 10;
            public const int SequenceCount = 11;
            public const int MessageStart = 12;
            public const int HeaderLength = 12;

            public const byte SequenseStart = 0x01;
        }

        internal static class GelfChunk
        {
            public const int PayloadSize = 8180;
            public const int TotalSize = 8192;
            public const int MaxCount = 128;
        }
    }
}
