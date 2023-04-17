using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PingPacket : INetSerializable
    {
        public float PingTime { get; set; }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteFloat32(PingTime);
        }

        public void ReadFrom(ref SpanBuffer reader)
        {
            PingTime = reader.ReadFloat32();
        }
        public void WriteTo(ref MemoryBuffer writer)
        {
            writer.WriteFloat32(PingTime);
        }

        public void ReadFrom(ref MemoryBuffer reader)
        {
            PingTime = reader.ReadFloat32();
        }
    }
}
