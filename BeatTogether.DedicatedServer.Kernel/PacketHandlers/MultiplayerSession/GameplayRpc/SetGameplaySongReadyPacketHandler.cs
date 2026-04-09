using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.GameplayRpc
{
    public sealed class SetGameplaySongReadyPacketHandler : BasePacketHandler<SetGameplaySongReadyPacket>
    {
        private readonly IGameplayManager _gameplayManager;
        private readonly Internal_Logger _logger;

        public SetGameplaySongReadyPacketHandler(
            IGameplayManager gameplayManager,
            Kernel_Logger kernel_Logger)
        {
            _gameplayManager = gameplayManager;
            _logger = kernel_Logger.ForContext<SetGameplaySongReadyPacketHandler>();
        }

        public override void Handle(IPlayer sender, SetGameplaySongReadyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SetGameplaySongReadyPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            _gameplayManager.HandleGameSongLoaded(sender);
        }
    }
}
