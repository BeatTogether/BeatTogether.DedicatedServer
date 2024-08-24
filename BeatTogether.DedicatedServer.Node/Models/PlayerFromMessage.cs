using BeatTogether.Core.Abstractions;
using BeatTogether.Core.Enums;
using System;

namespace BeatTogether.DedicatedServer.Node.Models
{
    public class PlayerFromMessage : IPlayer
    {
        public string HashedUserId { get; set; }
        public string PlatformUserId { get; set; }
        public string PlayerSessionId { get; set; }
        public Platform PlayerPlatform { get; set; }
        public Version PlayerClientVersion { get; set; }

        public PlayerFromMessage(Core.ServerMessaging.Models.Player player)
        {
            HashedUserId = player.HashedUserId;
            PlatformUserId = player.PlatformUserId;
            PlayerSessionId = player.PlayerSessionId;
            PlayerPlatform = player.PlayerPlatform;
            PlayerClientVersion = new Version(player.PlayerClientVersion);
        }
    }
}
