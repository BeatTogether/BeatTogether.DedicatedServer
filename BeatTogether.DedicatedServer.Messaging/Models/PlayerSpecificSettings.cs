using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;

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

        public void ReadFrom(ref SpanBuffer reader)
        {
            UserId = reader.ReadString();
            UserName = reader.ReadString();
            LeftHanded = reader.ReadBool();
            AutomaticPlayerHeight = reader.ReadBool();
            PlayerHeight = reader.ReadFloat32();
            HeadPosToPlayerHeightOffset = reader.ReadFloat32();
            ColorScheme.ReadFrom(ref reader);
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteString(UserId);
            writer.WriteString(UserName);
            writer.WriteBool(LeftHanded);
            writer.WriteBool(AutomaticPlayerHeight);
            writer.WriteFloat32(PlayerHeight);
            writer.WriteFloat32(HeadPosToPlayerHeightOffset);
            ColorScheme.WriteTo(ref writer);
        }
    }
}
