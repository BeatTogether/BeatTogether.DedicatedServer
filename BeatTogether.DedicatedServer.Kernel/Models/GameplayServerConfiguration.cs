using BeatTogether.DedicatedServer.Kernel.Enums;

namespace BeatTogether.DedicatedServer.Kernel.Models
{
    public sealed class GameplayServerConfiguration
    {
        public int MaxPlayerCount { get; }
        public DiscoveryPolicy DiscoveryPolicy { get; }
        public InvitePolicy InvitePolicy { get; }
        public GameplayServerMode GameplayServerMode { get; }
        public SongSelectionMode SongSelectionMode { get; }
        public GameplayServerControlSettings GameplayServerControlSettings { get; }

        public GameplayServerConfiguration(
            int maxPlayerCount,
            DiscoveryPolicy discoveryPolicy,
            InvitePolicy invitePolicy,
            GameplayServerMode gameplayServerMode,
            SongSelectionMode songSelectionMode,
            GameplayServerControlSettings gameplayServerControlSettings)
        {
            MaxPlayerCount = maxPlayerCount;
            DiscoveryPolicy = discoveryPolicy;
            InvitePolicy = invitePolicy;
            GameplayServerMode = gameplayServerMode;
            SongSelectionMode = songSelectionMode;
            GameplayServerControlSettings = gameplayServerControlSettings;
        }
    }
}
