using BeatTogether.Core.Enums;
using BeatTogether.Core.Models;

namespace BeatTogether.DedicatedServer.Kernel.Configuration
{
    public sealed class InstanceConfiguration
    {
        public int Port { get; set; }
        public string Secret { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string ServerOwnerId { get; set; } = string.Empty;
        public string ServerId { get; set; } = "ziuMSceapEuNN7wRGQXrZg";
        public string WelcomeMessage { get; set; } = string.Empty;
        public string ServerName { get; set; } = string.Empty;
        public long DestroyInstanceTimeout { get; set; } = 10000L; //set to -1 for no timeout(must close using api), 0 would be for instant close, 10 seconds is default. Less than 6 seconds can cause cfr-3 issues
        public string SetConstantManagerFromUserId { get; set; } = string.Empty; //If a user creates a server using the api and enteres there userId (eg uses discord bot with linked account))
        public bool AllowPerPlayerDifficulties { get; set; } = false;
        public bool AllowPerPlayerModifiers { get; set; } = false;

        public VersionRange SupportedVersionRange { get; set; } = new();
        public CountdownConfig CountdownConfig { get; set; } = new();

        public GameplayServerConfiguration GameplayServerConfiguration { get; set; } = new();

        public BeatmapDifficultyMask BeatmapDifficultyMask { get; set; }
        public GameplayModifiersMask GameplayModifiersMask { get; set; }
        public string SongPacksMask { get; set; } = string.Empty;

        public bool AllowChroma { get; set; }
        public bool AllowMappingExtensions { get; set; }
        public bool AllowNoodleExtensions { get; set; }
        public bool DisableNotes { get; set; }
        public bool ForceEnableNotes { get; set; } = false;
        public bool ForceStartMode { get; set; } = false;
        public long SendPlayersWithoutEntitlementToSpectateTimeout { get; set; } = 30000L;
        public int MaxLengthCommand { get; set; } = 200;
        public bool ApplyNoFailModifier { get; set; } = true;
        public int DisableNotesPlayerCount { get; set; } = 16;
    }

    public sealed class CountdownConfig
    {
        public long CountdownTimePlayersReady { get; set; } = 30000L;
        public long BeatMapStartCountdownTime { get; set; } = 5000L;
        public long ResultsScreenTime { get; set; } = 20000L;
    }
}
