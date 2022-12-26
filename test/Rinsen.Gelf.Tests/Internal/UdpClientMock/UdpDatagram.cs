namespace Rinsen.Gelf.Tests.Internal.UdpClientMock
{
    internal class UdpDatagram
    {
        public UdpDatagram(byte[] datagram, int byteSize, int order)
        {
            Datagram = datagram;
            ByteSize = byteSize;
            Order = order;
        }

        public byte[] Datagram { get; }

        public int ByteSize { get; }
        public int Order { get; }
    }
}
