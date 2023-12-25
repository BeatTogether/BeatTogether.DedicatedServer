using BeatTogether.DedicatedServer.Messaging.Converter;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.Legacy;
using BeatTogether.DedicatedServer.Messaging.Structs;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerAvatarPacket : IVersionedNetSerializable
    {
        public MultiplayerAvatarsData PlayerAvatar { get; set; } = new();

        public void ReadFrom(ref SpanBuffer reader, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                AvatarData avatarData = new();
                avatarData.ReadFrom(ref reader);
                PlayerAvatar.AvatarsData = new List<MultiplayerAvatarData> { avatarData.CreateMultiplayerAvatarsData() };
                return;
            }
            else
            {
                PlayerAvatar.ReadFrom(ref reader);
            }
        }

        public void WriteTo(ref SpanBuffer writer, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                PlayerAvatar.AvatarsData.LastOrDefault().CreateAvatarData().WriteTo(ref writer);
                return;
            }
            else
            {
                PlayerAvatar.WriteTo(ref writer);
            }
        }
    }
}
