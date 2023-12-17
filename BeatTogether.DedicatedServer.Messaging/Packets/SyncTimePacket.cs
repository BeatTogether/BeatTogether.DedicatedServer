using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class SyncTimePacket : INetSerializable
    {
        public long SyncTime { get; set; }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteVarULong((ulong)SyncTime);
        }

        public void ReadFrom(ref SpanBuffer reader)
        {
            SyncTime = (long)reader.ReadVarULong();
        }
    }
}
