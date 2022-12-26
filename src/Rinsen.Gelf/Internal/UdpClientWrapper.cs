using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Rinsen.Gelf.Internal
{
    internal class UdpClientWrapper : IUdpClient
    {
        private readonly UdpClient _udpClient = new UdpClient();
        private bool _disposedValue;
        private bool _initialized = false;
        private readonly object _lock = new();
        private readonly GelfOptions _gelfOptions;

        public UdpClientWrapper(GelfOptions gelfOptions)
        {
            _gelfOptions = gelfOptions;
        }

        public Task<int> SendAsync(byte[] datagram, int bytes)
        {
            Init();

            return _udpClient.SendAsync(datagram, bytes);
        }

        private void Init()
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

                if (IPAddress.TryParse(_gelfOptions.GelfServiceHostNameOrAddress, out var address))
                {
                    var ipEndpoint = new IPEndPoint(address, _gelfOptions.GelfServicePort);

                    _udpClient.Connect(ipEndpoint);
                }
                else
                {
                    _udpClient.Connect(_gelfOptions.GelfServiceHostNameOrAddress, _gelfOptions.GelfServicePort);
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
