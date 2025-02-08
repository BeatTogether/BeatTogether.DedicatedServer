using BeatTogether.Core.Abstractions;
using BeatTogether.Core.Enums;
using BeatTogether.Core.Models;
using System.Collections.Generic;
using System.Net;

namespace BeatTogether.DedicatedServer.Node.Models
{
    public class ServerFromMessage : IServerInstance
    {
        public string ServerName { get; set; }

        public string Secret { get; set; }

        public string Code { get; set; }

        public string InstanceId { get; set; }

        public MultiplayerGameState GameState { get; set; }

        public BeatmapDifficultyMask BeatmapDifficultyMask { get; set; }

        public GameplayModifiersMask GameplayModifiersMask { get; set; }

        public GameplayServerConfiguration GameplayServerConfiguration { get; set; }

        public string SongPackMasks { get; set; }

        public string ManagerId { get; set; }

        public bool PermanentManager { get; set; }

        public long ServerStartJoinTimeout { get; set; }

        public bool NeverCloseServer { get; set; }

        public long ResultScreenTime { get; set; }

        public long BeatmapStartTime { get; set; }

        public long PlayersReadyCountdownTime { get; set; }

        public bool AllowPerPlayerModifiers { get; set; }

        public bool AllowPerPlayerDifficulties { get; set; }

        public bool AllowChroma { get; set; }

        public bool AllowME { get; set; }

        public bool AllowNE { get; set; }

        public VersionRange SupportedVersionRange { get; set; }

        public IPEndPoint InstanceEndPoint { get; set; } = null!;
        public HashSet<string> PlayerHashes { get; set; } = null!;

        public ServerFromMessage(Core.ServerMessaging.Models.Server instance)
        {
            ServerName = instance.ServerName;
            Secret = instance.Secret;
            Code = instance.Code;
            InstanceId = instance.InstanceId;
            GameState = instance.GameState;
            BeatmapDifficultyMask = instance.BeatmapDifficultyMask;
            GameplayModifiersMask = instance.GameplayModifiersMask;
            GameplayServerConfiguration = instance.GameplayServerConfiguration;
            SongPackMasks = instance.SongPackMasks;
            ManagerId = instance.ManagerId;
            PermanentManager = instance.PermanentManager;
            ServerStartJoinTimeout = instance.ServerStartJoinTimeout;
            NeverCloseServer = instance.NeverCloseServer;
            ResultScreenTime = instance.ResultScreenTime;
            BeatmapStartTime = instance.BeatmapStartTime;
            PlayersReadyCountdownTime = instance.PlayersReadyCountdownTime;
            AllowPerPlayerDifficulties = instance.AllowPerPlayerDifficulties;
            AllowPerPlayerModifiers = instance.AllowPerPlayerModifiers;
            AllowChroma = instance.AllowChroma;
            AllowME = instance.AllowME;
            AllowNE = instance.AllowNE;
            SupportedVersionRange = instance.SupportedVersionRange;
        }
    }
}
