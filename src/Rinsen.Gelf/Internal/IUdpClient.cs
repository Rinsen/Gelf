using System;
using System.Net;
using System.Threading.Tasks;

namespace Rinsen.Gelf.Internal
{
    internal interface IUdpClient : IDisposable
    {
        Task<int> SendAsync(byte[] datagram, int bytes);

    }
}
