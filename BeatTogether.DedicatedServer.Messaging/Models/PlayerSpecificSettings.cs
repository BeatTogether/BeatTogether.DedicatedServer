using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;

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

        public void ReadFrom(ref SpanBufferReader reader)
        {
            UserId = reader.ReadUTF8String();
            UserName = reader.ReadUTF8String();
            LeftHanded = reader.ReadBool();
            AutomaticPlayerHeight = reader.ReadBool();
            PlayerHeight = reader.ReadFloat32();
            HeadPosToPlayerHeightOffset = reader.ReadFloat32();
            ColorScheme.ReadFrom(ref reader);
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteUTF8String(UserId);
            writer.WriteUTF8String(UserName);
            writer.WriteBool(LeftHanded);
            writer.WriteBool(AutomaticPlayerHeight);
            writer.WriteFloat32(PlayerHeight);
            writer.WriteFloat32(HeadPosToPlayerHeightOffset);
            ColorScheme.WriteTo(ref writer);
        }
    }
}
