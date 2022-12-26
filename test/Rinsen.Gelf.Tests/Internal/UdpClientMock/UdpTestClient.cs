using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Rinsen.Gelf.Internal;

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

        public JsonDocument GetData()
        {
            var data = Encoding.UTF8.GetString(_udpDatagrams.First().Datagram);

            var result = JsonSerializer.Deserialize<JsonDocument>(data);

            return result;
        }
    }
}
