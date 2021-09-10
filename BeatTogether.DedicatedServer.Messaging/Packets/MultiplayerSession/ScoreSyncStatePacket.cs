using BeatTogether.DedicatedServer.Messaging.Models;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession
{
    public sealed class ScoreSyncStatePacket : INetSerializable
    {
        public byte SyncStateId { get; set; }
        public float Time { get; set; }
        public StandardScoreSyncState State { get; set; } = new();

        public void Deserialize(NetDataReader reader)
        {
            SyncStateId = reader.GetByte();
            Time = reader.GetFloat();
            State.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(SyncStateId);
            writer.Put(Time);
            State.Serialize(writer);
        }
    }
}
