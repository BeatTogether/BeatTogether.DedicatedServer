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
            bool flag = (SyncStateId & 128) > 0;
            SyncStateId &= 127;
            TimeOffsetMs = reader.ReadVarInt();

            if (!flag)
                Delta.ReadFrom(ref reader);
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            bool flag = default(NodePoseSyncState).Equals(Delta);
            writer.WriteUInt8((byte)(SyncStateId | (flag ? 128 : 0)));
            writer.WriteVarInt(TimeOffsetMs);
            if (!flag)
                Delta.WriteTo(ref writer);
        }
    }
}
