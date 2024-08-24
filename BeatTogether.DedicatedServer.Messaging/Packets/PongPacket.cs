using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PongPacket : INetSerializable
    {
        public long PingTime { get; set; }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteVarULong((ulong)PingTime);
        }

        public void ReadFrom(ref SpanBuffer reader)
        {
            PingTime = (long)reader.ReadVarULong();
        }
    }
}
