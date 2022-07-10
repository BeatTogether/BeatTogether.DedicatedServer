using BeatTogether.DedicatedServer.Kernel.Enums;

namespace BeatTogether.DedicatedServer.Kernel.Configuration
{
    public sealed class InstanceConfiguration
    {
        public int Port { get; set; }
        public string Secret { get; set; } = string.Empty;
        public string ManagerId { get; set; } = string.Empty;
        public string ServerId { get; set; } = "ziuMSceapEuNN7wRGQXrZg";
        public string ServerName { get; set; } = string.Empty;
        public float DestroyInstanceTimeout { get; set; } = 0f; //set to -1 for no timeout(must close using api), 0 would be for lobbies made the usaual way, or set a number for a timeout
        public string SetConstantManagerFromUserId { get; set; } = string.Empty; //If a user creates a server using the api and enteres there userId (eg uses discord bot with linked account))
        public BeatmapDiffering BeatmapDiffering { get; set; } = BeatmapDiffering.Same;
        public bool AllowPerPlayerModifiers { get; set; } = false;
        public bool AllowLocalBeatmaps { get; set; } = false;
        public CountdownConfig CountdownConfig { get; set; } = new();
        public int MaxPlayerCount { get; set; }
        public DiscoveryPolicy DiscoveryPolicy { get; set; }
        public InvitePolicy InvitePolicy { get; set; }
        public GameplayServerMode GameplayServerMode { get; set; }
        public SongSelectionMode SongSelectionMode { get; set; }
        public GameplayServerControlSettings GameplayServerControlSettings { get; set; }
        public bool AllowChroma { get; set; }
        public bool AllowMappingExtensions { get; set; }
        public bool AllowNoodleExtensions { get; set; }
    }

    public sealed class CountdownConfig
    {
        public float CountdownTimePlayersReady { get; set; } = 60.0f;
        public float BeatMapStartCountdownTime { get; set; } = 5.0f;
        public float ResultsScreenTime { get; set; } = 20.0f;
    }
}
