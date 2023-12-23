using BeatTogether.DedicatedServer.Messaging.Converter;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.Legacy;
using BeatTogether.DedicatedServer.Messaging.Structs;
using BeatTogether.LiteNetLib.Util;
using System;
using System.Linq;
using System.Reflection.PortableExecutable;
namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerIdentityPacket : IVersionedNetSerializable
    {
        public PlayerStateHash PlayerState { get; set; } = new();
        public MultiplayerAvatarsData PlayerAvatar { get; set; } = new();
        public ByteArray Random { get; set; } = new();
        public ByteArray PublicEncryptionKey { get; set; } = new();

        public void ReadFrom(ref SpanBuffer reader)
        {
            PlayerState.ReadFrom(ref reader);
            PlayerAvatar.ReadFrom(ref reader);
            Random.ReadFrom(ref reader);
            PublicEncryptionKey.ReadFrom(ref reader);
        }

        public void ReadFrom(ref SpanBuffer reader, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                PlayerState.ReadFrom(ref reader);
                AvatarData avatarData = new();
                avatarData.ReadFrom(ref reader);
                PlayerAvatar.AvatarsData.Add(avatarData.CreateMultiplayerAvatarsData());
                Random.ReadFrom(ref reader);
                PublicEncryptionKey.ReadFrom(ref reader);
                return;
            }
            else
            {
                ReadFrom(ref reader);
            }
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            PlayerState.WriteTo(ref writer);
            PlayerAvatar.WriteTo(ref writer);
            Random.WriteTo(ref writer);
            PublicEncryptionKey.WriteTo(ref writer);
        }

        public void WriteTo(ref SpanBuffer writer, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                PlayerState.WriteTo(ref writer);
                PlayerAvatar.AvatarsData.FirstOrDefault().CreateAvatarData().WriteTo(ref writer);
                Random.WriteTo(ref writer);
                PublicEncryptionKey.WriteTo(ref writer);
                return;
            }
            else
            {
                WriteTo(ref writer);
            }
        }
    }
}
