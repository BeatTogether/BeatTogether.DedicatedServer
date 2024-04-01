using System;
using System.Net;

namespace BeatTogether.DedicatedServer.Messaging.Abstractions
{
    public interface IPacketLayer
    {
        public void ProcessInboundPacket(EndPoint endPoint, ref Span<byte> data);
        public void ProcessOutBoundPacket(EndPoint endPoint, ref Span<byte> data);
    }
}
