﻿using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerIdentityPacket : INetSerializable
    {
        public PlayerStateHash PlayerState { get; set; } = new();
        public AvatarData PlayerAvatar { get; set; } = new();
        public ByteArray Random { get; set; } = new();
        public ByteArray PublicEncryptionKey { get; set; } = new();

        public void ReadFrom(ref SpanBufferReader reader)
        {
            PlayerState.ReadFrom(ref reader);
            PlayerAvatar.ReadFrom(ref reader);
            Random.ReadFrom(ref reader);
            PublicEncryptionKey.ReadFrom(ref reader);
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            PlayerState.WriteTo(ref writer);
            PlayerAvatar.WriteTo(ref writer);
            Random.WriteTo(ref writer);
            PublicEncryptionKey.WriteTo(ref writer);
        }
    }
}
