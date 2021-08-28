using BeatTogether.DedicatedServer.Messaging.Models;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession
{
    public sealed class NodePoseSyncStatePacket : INetSerializable
    {
        public NodePoseSyncState? State { get; set; } = new();

        public void Deserialize(NetDataReader reader)
        {
            State.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            State.Serialize(writer);
        }
    }
}
