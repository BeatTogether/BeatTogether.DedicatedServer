using BeatTogether.DedicatedServer.Messaging.Converter;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.Legacy;
using BeatTogether.DedicatedServer.Messaging.Structs;
using BeatTogether.LiteNetLib.Util;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerIdentityPacket : IVersionedNetSerializable
    {
        public PlayerStateHash PlayerState { get; set; } = new();
        public MultiplayerAvatarsData PlayerAvatar { get; set; } = new();
        public ByteArray Random { get; set; } = new();
        public ByteArray PublicEncryptionKey { get; set; } = new();

        private readonly ILogger _logger = Log.ForContext<PlayerIdentityPacket>();

        public void ReadFrom(ref SpanBuffer reader, Version version)
        {
            try
            {
                if (version < ClientVersions.NewPacketVersion)
                {
                    _logger.Debug($"Reading packet using old version {version}.");
                    PlayerState.ReadFrom(ref reader);
                    AvatarData avatarData = new();
                    avatarData.ReadFrom(ref reader);
                    PlayerAvatar.AvatarsData = new List<MultiplayerAvatarData> { avatarData.CreateMultiplayerAvatarsData() };
                    Random.ReadFrom(ref reader);
                    PublicEncryptionKey.ReadFrom(ref reader);
                    return;
                }
                else
                {
                    _logger.Debug($"Reading packet using new version.");
                    PlayerState.ReadFrom(ref reader);
                    PlayerAvatar.ReadFrom(ref reader);
                    Random.ReadFrom(ref reader);
                    PublicEncryptionKey.ReadFrom(ref reader);
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"Failed to read packet using version {version}.");
                throw;
            }
        }

        public void WriteTo(ref SpanBuffer writer, Version version)
        {
            try
            {
                if (version < ClientVersions.NewPacketVersion)
                {
                    _logger.Debug($"Writing packet using old version {version}.");
                    _logger.Debug($"AvatarData count {PlayerAvatar.AvatarsData.Count}");
                    foreach(var avatarData in PlayerAvatar.AvatarsData)
                    {
                        _logger.Debug($"AvatarData: {JsonSerializer.Serialize(avatarData.CreateAvatarData())}");
                    }
                    PlayerState.WriteTo(ref writer);
                    PlayerAvatar.AvatarsData.LastOrDefault().CreateAvatarData().WriteTo(ref writer);
                    Random.WriteTo(ref writer);
                    PublicEncryptionKey.WriteTo(ref writer);
                    return;
                }
                else
                {
                    PlayerState.WriteTo(ref writer);
                    PlayerAvatar.WriteTo(ref writer);
                    Random.WriteTo(ref writer);
                    PublicEncryptionKey.WriteTo(ref writer);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to write packet using version {version}.");
                throw;
            }
        }
    }
}
