using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class SyncTimePacket : INetSerializable
    {
        public float SyncTime { get; set; }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteFloat32(SyncTime);
        }

        public void ReadFrom(ref SpanBufferReader reader)
        {
            SyncTime = reader.ReadFloat32();
        }
    }
}
