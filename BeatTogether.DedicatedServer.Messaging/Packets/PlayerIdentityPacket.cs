﻿using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Util;
namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerIdentityPacket : INetSerializable
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

        public void WriteTo(ref SpanBuffer writer)
        {
            PlayerState.WriteTo(ref writer);
            PlayerAvatar.WriteTo(ref writer);
            Random.WriteTo(ref writer);
            PublicEncryptionKey.WriteTo(ref writer);
        }
    }
}
