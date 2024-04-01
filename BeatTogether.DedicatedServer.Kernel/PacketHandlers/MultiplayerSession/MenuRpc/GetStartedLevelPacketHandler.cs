using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;
using System.Linq;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetStartedLevelPacketHandler : BasePacketHandler<GetStartedLevelPacket>
    {
        private readonly IDedicatedInstance _instance;
        private readonly InstanceConfiguration _configuration;
        private readonly ILobbyManager _lobbyManager;
        private readonly IGameplayManager _gameplayManager;
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<GetStartedLevelPacketHandler>();

        public GetStartedLevelPacketHandler(
            InstanceConfiguration instanceConfiguration,
            IDedicatedInstance instance, 
            ILobbyManager lobbyManager,
            IGameplayManager gameplayManager, 
            IPacketDispatcher packetDispatcher)
        {
            _configuration = instanceConfiguration;
            _instance = instance;
            _lobbyManager = lobbyManager;
            _gameplayManager = gameplayManager;
            _packetDispatcher = packetDispatcher;
        }

        public override void Handle(IPlayer sender, GetStartedLevelPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetStartedLevelPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            if (_instance.State == MultiplayerGameState.Game && _gameplayManager.CurrentBeatmap != null && _gameplayManager.State != GameplayManagerState.None)
            {
                _packetDispatcher.SendToPlayer(sender, new StartLevelPacket
                {
                    Beatmap = _gameplayManager.CurrentBeatmap,
                    Modifiers = _gameplayManager.CurrentModifiers,
                    StartTime = _instance.RunTime
                }, IgnoranceChannelTypes.Reliable);
            }
            else
            {
                if (_lobbyManager.SelectedBeatmap != null)
                {
                    if(sender.GetEntitlement(_lobbyManager.SelectedBeatmap.LevelId) != EntitlementStatus.Ok)
                        _packetDispatcher.SendToPlayer(sender, new GetIsEntitledToLevelPacket
                        {
                            LevelId = _lobbyManager.SelectedBeatmap.LevelId
                        }, IgnoranceChannelTypes.Reliable);

                    if(_lobbyManager.CountDownState == CountdownState.WaitingForEntitlement || _lobbyManager.CountDownState == CountdownState.StartBeatmapCountdown)
                    {
                        BeatmapIdentifier Beatmap = _lobbyManager.SelectedBeatmap!;
                        GameplayModifiers Modifiers = _lobbyManager.SelectedModifiers;
                        if (_configuration.AllowPerPlayerModifiers)
                            Modifiers = sender.Modifiers;
                        if (_configuration.AllowPerPlayerDifficulties)
                        {
                            BeatmapDifficulty[] diff = _lobbyManager.GetSelectedBeatmapDifficulties();
                            if (sender.BeatmapIdentifier != null && diff.Contains(sender.BeatmapIdentifier.Difficulty))
                                Beatmap.Difficulty = sender.BeatmapIdentifier.Difficulty;
                        }
                        _packetDispatcher.SendToPlayer(sender, new StartLevelPacket
                        {
                            Beatmap = Beatmap,
                            Modifiers = Modifiers,
                            StartTime = _lobbyManager.CountdownEndTime
                        }, IgnoranceChannelTypes.Reliable);
                    }
                }
                else
                    _packetDispatcher.SendToPlayer(sender, new CancelLevelStartPacket(), IgnoranceChannelTypes.Reliable);
            }
        }
    }
}
