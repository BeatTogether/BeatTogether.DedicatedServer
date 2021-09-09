using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class PlayerSpecificSettings : INetSerializable
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public bool LeftHanded { get; set; }
        public bool AutomaticPlayerHeight { get; set; }
        public float PlayerHeight { get; set; }
        public float HeadPosToPlayerHeightOffset { get; set; }
        public ColorScheme ColorScheme { get; set; } = new();

        public void Deserialize(NetDataReader reader)
        {
            UserId = reader.GetString();
            UserName = reader.GetString();
            LeftHanded = reader.GetBool();
            AutomaticPlayerHeight = reader.GetBool();
            PlayerHeight = reader.GetFloat();
            HeadPosToPlayerHeightOffset = reader.GetFloat();
            ColorScheme.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserId);
            writer.Put(UserName);
            writer.Put(LeftHanded);
            writer.Put(AutomaticPlayerHeight);
            writer.Put(PlayerHeight);
            writer.Put(HeadPosToPlayerHeightOffset);
            ColorScheme.Serialize(writer);
        }
    }
}
