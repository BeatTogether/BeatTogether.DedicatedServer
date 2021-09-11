using BeatTogether.DedicatedServer.Messaging.Models;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerIdentityPacket : INetSerializable
    {
        public PlayerStateHash PlayerState { get; set; } = new();
        public AvatarData PlayerAvatar { get; set; } = new();

        public void Deserialize(NetDataReader reader)
        {
            PlayerState.Deserialize(reader);
            PlayerAvatar.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            PlayerState.Serialize(writer);
            PlayerAvatar.Serialize(writer);
        }
    }
}
