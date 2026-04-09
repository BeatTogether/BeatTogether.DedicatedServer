using BeatTogether.Core.Enums;
using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetSelectedBeatmapHandler : BasePacketHandler<GetSelectedBeatmap>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILobbyManager _lobbyManager;
        private readonly IGameplayManager _gameplayManager;
        private readonly IDedicatedInstance _instance;
        private readonly Internal_Logger _logger;

        public GetSelectedBeatmapHandler(
            IPacketDispatcher packetDispatcher,
            ILobbyManager lobbyManager,
            IGameplayManager gameplayManager,
            IDedicatedInstance instance,
            Kernel_Logger kernel_Logger)
        {
            _packetDispatcher = packetDispatcher;
            _lobbyManager = lobbyManager;
            _gameplayManager = gameplayManager;
            _instance = instance;
            _logger = kernel_Logger.ForContext<GetSelectedBeatmapHandler>();
        }

        public override void Handle(IPlayer sender, GetSelectedBeatmap packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetSelectedBeatmap)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            //TODO send custom packet details
            if (_instance.State != MultiplayerGameState.Game && _lobbyManager.SelectedBeatmap != null)
            {
                _packetDispatcher.SendToPlayer(sender, new SetSelectedBeatmap
                {
                    Beatmap = _lobbyManager.SelectedBeatmap
                }, IgnoranceChannelTypes.Reliable);
                return;
            }
            if (_instance.State == MultiplayerGameState.Game && _gameplayManager.CurrentBeatmap != null)
            {
                _packetDispatcher.SendToPlayer(sender, new SetSelectedBeatmap
                {
                    Beatmap = _gameplayManager.CurrentBeatmap
                }, IgnoranceChannelTypes.Reliable);
                return;
            }
            _packetDispatcher.SendToPlayer(sender, new ClearSelectedBeatmap(), IgnoranceChannelTypes.Reliable);
        }
    }
}
