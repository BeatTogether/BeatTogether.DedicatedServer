using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PongPacket : INetSerializable
    {
        public float PingTime { get; set; }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteFloat32(PingTime);
        }

        public void ReadFrom(ref SpanBufferReader reader)
        {
            PingTime = reader.ReadFloat32();
        }
    }
}
