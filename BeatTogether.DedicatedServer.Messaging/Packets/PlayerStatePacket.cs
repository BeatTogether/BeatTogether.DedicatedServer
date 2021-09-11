using BeatTogether.DedicatedServer.Messaging.Models;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerStatePacket : INetSerializable
    {
        public PlayerStateHash PlayerState { get; set; } = new();

        public void Deserialize(NetDataReader reader)
        {
            PlayerState.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            PlayerState.Serialize(writer);
        }
    }
}
