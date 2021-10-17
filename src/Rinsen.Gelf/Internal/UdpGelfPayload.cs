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

            await _udpClient.SendAsync(sendbuf, sendbuf.Length);
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
