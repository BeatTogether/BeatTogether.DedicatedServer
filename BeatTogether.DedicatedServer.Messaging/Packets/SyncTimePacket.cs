using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class SyncTimePacket : INetSerializable
    {
        public float SyncTime { get; set; }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteFloat32(SyncTime);
        }

        public void ReadFrom(ref SpanBuffer reader)
        {
            SyncTime = reader.ReadFloat32();
        }
        public void WriteTo(ref MemoryBuffer writer)
        {
            writer.WriteFloat32(SyncTime);
        }

        public void ReadFrom(ref MemoryBuffer reader)
        {
            SyncTime = reader.ReadFloat32();
        }
    }
}
