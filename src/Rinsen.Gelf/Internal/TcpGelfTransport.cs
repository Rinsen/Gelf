using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rinsen.Gelf.Internal
{
    internal class TcpGelfTransport : IGelfTransport, IDisposable
    {
        private readonly GelfOptions _gelfOptions;
        private readonly TcpClient _tcpClient;
        private readonly byte[] _endMessageByte = new byte[] { (byte)'\0' };
        private bool _disposedValue;

        public GelfTransport TransportType => GelfTransport.Tcp;

        public TcpGelfTransport(GelfOptions gelfOptions)
        {
            _gelfOptions = gelfOptions;
            _tcpClient = new TcpClient();
            if (IPAddress.TryParse(gelfOptions.GelfServiceHostName, out var address))
            {
                var ipEndpoint = new IPEndPoint(address, _gelfOptions.GelfServicePort);
             
                _tcpClient.Connect(ipEndpoint);
            }
            else
            {
                _tcpClient.Connect(gelfOptions.GelfServiceHostName, _gelfOptions.GelfServicePort);
            }
        }

        public async Task Send(GelfPayload gelfPayload, CancellationToken stoppingToken)
        {
            var serializedPayload = GelfPayloadSerializer.Serialize(gelfPayload);

            byte[] sendbuf = Encoding.UTF8.GetBytes(serializedPayload);

            var stream = _tcpClient.GetStream();

            await stream.WriteAsync(sendbuf, stoppingToken);
            await stream.WriteAsync(_endMessageByte, stoppingToken);

            await stream.FlushAsync(stoppingToken);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _tcpClient.Dispose();
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
