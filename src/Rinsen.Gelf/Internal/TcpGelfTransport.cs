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
        private bool _initialized = false;
        private readonly object _lock = new();

        public GelfTransport TransportType => GelfTransport.Tcp;

        public TcpGelfTransport(GelfOptions gelfOptions)
        {
            _gelfOptions = gelfOptions;
            _tcpClient = new TcpClient();      
        }

        public async Task Send(GelfPayload gelfPayload, CancellationToken stoppingToken)
        {
            Init(_gelfOptions);

            var serializedPayload = GelfPayloadSerializer.Serialize(gelfPayload);

            byte[] sendbuf = Encoding.UTF8.GetBytes(serializedPayload);

            var stream = _tcpClient.GetStream();

            await stream.WriteAsync(sendbuf, stoppingToken);
            await stream.WriteAsync(_endMessageByte, stoppingToken);

            await stream.FlushAsync(stoppingToken);
        }

        private void Init(GelfOptions gelfOptions)
        {
            if (_initialized)
            {
                return;
            }

            lock (_lock)
            {
                if (_initialized)
                {
                    return;
                }

                if (IPAddress.TryParse(gelfOptions.GelfServiceHostNameOrAddress, out var address))
                {
                    var ipEndpoint = new IPEndPoint(address, _gelfOptions.GelfServicePort);

                    _tcpClient.Connect(ipEndpoint);
                }
                else
                {
                    _tcpClient.Connect(gelfOptions.GelfServiceHostNameOrAddress, _gelfOptions.GelfServicePort);
                }
                _initialized = true;
            }
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
