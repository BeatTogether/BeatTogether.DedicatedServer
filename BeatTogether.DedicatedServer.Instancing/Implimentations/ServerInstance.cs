using BeatTogether.Core.Abstractions;
using BeatTogether.Core.Enums;
using BeatTogether.Core.Models;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace BeatTogether.DedicatedServer.Instancing.Implimentations
{
    public class ServerInstance : IServerInstance
    {
        private readonly IDedicatedInstance _ServerInstance;

        public ServerInstance(IDedicatedInstance serverInstance, IPEndPoint instanceEndPoint)
        {
            _ServerInstance = serverInstance;
            InstanceEndPoint = instanceEndPoint;
        }

        public string ServerName { get => _ServerInstance._configuration.ServerName; set => throw new NotImplementedException(); }
        public IPEndPoint InstanceEndPoint { get; set; }
        public string Secret { get => _ServerInstance._configuration.Secret; set => throw new NotImplementedException(); }
        public string Code { get => _ServerInstance._configuration.Code; set => throw new NotImplementedException(); }
        public MultiplayerGameState GameState { get => (MultiplayerGameState)_ServerInstance.State; set => throw new NotImplementedException(); }
        public BeatmapDifficultyMask BeatmapDifficultyMask { get => _ServerInstance._configuration.BeatmapDifficultyMask; set => throw new NotImplementedException(); }
        public GameplayModifiersMask GameplayModifiersMask { get => _ServerInstance._configuration.GameplayModifiersMask; set => throw new NotImplementedException(); }
        public string SongPackMasks { get => _ServerInstance._configuration.SongPacksMask; set => throw new NotImplementedException(); }
        public GameplayServerConfiguration GameplayServerConfiguration { get => _ServerInstance._configuration.GameplayServerConfiguration; set => throw new NotImplementedException(); }
        public HashSet<string> PlayerHashes { get => _ServerInstance.GetPlayerRegistry().Players.Select(p => p.HashedUserId).ToHashSet(); set => throw new NotImplementedException(); }
        public string InstanceId { get => _ServerInstance._configuration.ServerId; set => throw new NotImplementedException(); }
        public string ManagerId { get => _ServerInstance._configuration.ServerOwnerId; set => throw new NotImplementedException(); }
        public bool PermanentManager { get => !string.IsNullOrEmpty(_ServerInstance._configuration.SetConstantManagerFromUserId); set => throw new NotImplementedException(); }
        public long ServerStartJoinTimeout { get => _ServerInstance._configuration.DestroyInstanceTimeout; set => throw new NotImplementedException(); }
        public bool NeverCloseServer { get => _ServerInstance._configuration.DestroyInstanceTimeout == -1; set => throw new NotImplementedException(); }
        public long ResultScreenTime { get => _ServerInstance._configuration.CountdownConfig.ResultsScreenTime; set => throw new NotImplementedException(); }
        public long BeatmapStartTime { get => _ServerInstance._configuration.CountdownConfig.BeatMapStartCountdownTime; set => throw new NotImplementedException(); }
        public long PlayersReadyCountdownTime { get => _ServerInstance._configuration.CountdownConfig.CountdownTimePlayersReady; set => throw new NotImplementedException(); }
        public bool AllowPerPlayerModifiers { get => _ServerInstance._configuration.AllowPerPlayerModifiers; set => throw new NotImplementedException(); }
        public bool AllowPerPlayerDifficulties { get => _ServerInstance._configuration.AllowPerPlayerDifficulties; set => throw new NotImplementedException(); }
        public bool AllowChroma { get => _ServerInstance._configuration.AllowChroma; set => throw new NotImplementedException(); }
        public bool AllowME { get => _ServerInstance._configuration.AllowMappingExtensions; set => throw new NotImplementedException(); }
        public bool AllowNE { get => _ServerInstance._configuration.AllowNoodleExtensions; set => throw new NotImplementedException(); }
    }
}
