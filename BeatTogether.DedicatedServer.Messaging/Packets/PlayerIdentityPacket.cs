using BeatTogether.DedicatedServer.Messaging.Models;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerIdentityPacket : INetSerializable
    {
        public PlayerStateBloomFilter PlayerStateBloomFilter { get; set; } = new();
        public AvatarData AvatarData { get; set; } = new();

        public void Deserialize(NetDataReader reader)
        {
            PlayerStateBloomFilter.Deserialize(reader);
            AvatarData.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            PlayerStateBloomFilter.Serialize(writer);
            AvatarData.Serialize(writer);
        }
    }
}
