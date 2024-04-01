using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class SetIsReadyPacketHandler : BasePacketHandler<SetIsReadyPacket>
    {
        private readonly ILobbyManager _lobbyManager;
        private readonly ILogger _logger = Log.ForContext<SetIsReadyPacketHandler>();
        private readonly IDedicatedInstance _instance;
        private readonly IGameplayManager _gameplayManager;
        private readonly IPacketDispatcher _packetDispatcher;

        public SetIsReadyPacketHandler(
            ILobbyManager lobbyManager,
            IDedicatedInstance dedicatedInstance,
            IGameplayManager gameplayManager,
            IPacketDispatcher packetDispatcher)
        {
            _lobbyManager = lobbyManager;
            _instance = dedicatedInstance;
            _gameplayManager = gameplayManager;
            _packetDispatcher = packetDispatcher;
        }

        public override void Handle(IPlayer sender, SetIsReadyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SetIsReadyPacket)}' " +
                $"(SenderId={sender.ConnectionId}, IsReady={packet.IsReady})."
            );
            sender.IsReady = packet.IsReady;
            //If the player somehow is in the lobby during gameplay then readying should send them to spectate
            if (sender.IsReady && _instance.State == MultiplayerGameState.Game && _gameplayManager.CurrentBeatmap != null && _gameplayManager.State == GameplayManagerState.Gameplay)
                _packetDispatcher.SendToPlayer(sender, new StartLevelPacket
                {
                    Beatmap = _gameplayManager.CurrentBeatmap!,
                    Modifiers = _gameplayManager.CurrentModifiers,
                    StartTime = _instance.RunTime
                }, IgnoranceChannelTypes.Reliable);
        }
    }
}
