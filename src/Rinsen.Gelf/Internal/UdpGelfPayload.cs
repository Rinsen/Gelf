using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rinsen.Gelf
{
    internal class UdpGelfPayload : IGelfTransport, IDisposable
    {
        private readonly GelfOptions _gelfOptions;
        private readonly UdpClient _udpClient;
        private bool _disposedValue;

        public UdpGelfPayload(GelfOptions gelfOptions)
        {
            _gelfOptions = gelfOptions;
            _udpClient = new UdpClient(_gelfOptions.GelfServiceHostName, _gelfOptions.GelfServicePort);
        }

        public async Task Send(string gelfPayload, CancellationToken stoppingToken)
        {
            byte[] sendbuf = Encoding.UTF8.GetBytes(gelfPayload);

            // UDP datagrams are limited to a size of 65536 bytes. Some Graylog components are limited to processingup to 8192 bytes. 
            // https://docs.graylog.org/docs/gelf

            // UdpClient docs https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.udpclient?view=net-5.0

            if (sendbuf.Length > 8192)
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

            // If count is larger than 128 we silently drop this package for now
            if (totalMessageChunksCount > 128)
            {
                return;
            }

            var chunkedSendBuffer = new byte[8192 + 12];
            var messageId = GetMessageId();

            chunkedSendBuffer[0] = 0x1e;
            chunkedSendBuffer[1] = 0x0f;
            Array.Copy(messageId, 0, chunkedSendBuffer, 2, 8);
            chunkedSendBuffer[10] = 0x01;
            chunkedSendBuffer[11] = (byte)totalMessageChunksCount;

            for (int i = 0; i < totalMessageChunksCount; i++)
            {
                var startIndex = i * 8192;

                if (i == totalMessageChunksCount - 1)
                {
                    var remainingLength = sendbuf.Length - (totalMessageChunksCount - 1) * 8192;

                    var lastChunkSendBuffer = new byte[12 + remainingLength];
                    Array.Copy(chunkedSendBuffer, lastChunkSendBuffer, 12);

                }
                else
                {
                    // Copy in log message content
                    await _udpClient.SendAsync(chunkedSendBuffer, sendbuf.Length);
                }
            }
        }

        private static int GetMessageChunkCount(byte[] sendbuf)
        {
            double chunkCount = sendbuf.Length / 8192;

            var count = (int)chunkCount;

            if (chunkCount - Math.Truncate(chunkCount) > 0)
            {
                count++;
            }

            return (byte)count;
        }

        private byte[] GetMessageId()
        {
            var messageId = Environment.MachineName + DateTimeOffset.Now.ToUnixTimeMilliseconds();

            return Encoding.UTF8.GetBytes(messageId);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _udpClient.Dispose();
                }
                _disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
