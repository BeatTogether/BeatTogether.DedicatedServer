using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerLatencyPacket : INetSerializable
    {
        public long Latency { get; set; }

        public void ReadFrom(ref SpanBuffer reader)
        {
            Latency = (long)reader.ReadVarULong();
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteVarULong((ulong)Latency);
        }
    }
}
