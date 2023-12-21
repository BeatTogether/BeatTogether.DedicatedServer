using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession
{
    public sealed class ScoreSyncStatePacket : INetSerializable
    {
        public byte SyncStateId { get; set; }
        public long Time { get; set; }
        public StandardScoreSyncState State { get; set; } = new();

        public void ReadFrom(ref SpanBuffer reader)
        {
            SyncStateId = reader.ReadUInt8();
            Time = (long)reader.ReadVarULong();
            State.ReadFrom(ref reader);
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteUInt8(SyncStateId);
            writer.WriteVarULong((ulong)Time);
            State.WriteTo(ref writer);
        }
    }
}
