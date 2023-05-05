using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerLatencyPacket : INetSerializable
    {
        public float Latency { get; set; }

        public void ReadFrom(ref SpanBuffer reader)
        {
            Latency = reader.ReadFloat32();
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteFloat32(Latency);
        }
    }
}
