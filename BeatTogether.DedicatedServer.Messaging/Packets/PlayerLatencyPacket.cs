using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerLatencyPacket : INetSerializable
    {
        public float Latency { get; set; }

        public void ReadFrom(ref SpanBufferReader reader)
        {
            Latency = reader.ReadFloat32();
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteFloat32(Latency);
        }
    }
}
