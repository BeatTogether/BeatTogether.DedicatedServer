using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class SyncTimePacket : INetSerializable
    {
        public long SyncTime { get; set; }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteUInt64((ulong)SyncTime);
        }

        public void ReadFrom(ref SpanBuffer reader)
        {
            SyncTime = (long)reader.ReadUInt64();
        }
    }
}
