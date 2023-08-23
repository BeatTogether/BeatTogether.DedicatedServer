using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession
{
    public sealed class NodePoseSyncStateDeltaPacket : INetSerializable
    {
        public byte SyncStateId { get; set; }
        public int TimeOffsetMs { get; set; }
        public NodePoseSyncState Delta { get; set; } = new();

        public void ReadFrom(ref SpanBuffer reader)
        {
            SyncStateId = reader.ReadUInt8();
            TimeOffsetMs = reader.ReadVarInt();

            if (!((SyncStateId & 128) > 0))
                Delta.ReadFrom(ref reader);
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteUInt8(SyncStateId);
            writer.WriteVarInt(TimeOffsetMs);
            Delta.WriteTo(ref writer);
        }
    }
}
