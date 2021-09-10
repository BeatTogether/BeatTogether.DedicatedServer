using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.Extensions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession
{
    public sealed class NodePoseSyncStateDeltaPacket : INetSerializable
    {
        public byte SyncStateId { get; set; }
        public int TimeOffsetMs { get; set; }
        public NodePoseSyncState Delta { get; set; } = new();

        public void Deserialize(NetDataReader reader)
        {
            SyncStateId = reader.GetByte();
            TimeOffsetMs = reader.GetVarInt();

            if (!((SyncStateId & 128) > 0))
                Delta.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(SyncStateId);
            writer.PutVarInt(TimeOffsetMs);
            Delta.Serialize(writer);
        }
    }
}
