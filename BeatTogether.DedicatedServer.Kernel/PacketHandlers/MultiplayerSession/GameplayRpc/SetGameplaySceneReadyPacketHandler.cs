using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.GameplayRpc
{
    public sealed class SetGameplaySceneReadyPacketHandler : BasePacketHandler<SetGameplaySceneReadyPacket>
    {
        private readonly IGameplayManager _gameplayManager;
        private readonly Internal_Logger _logger;

        public SetGameplaySceneReadyPacketHandler(
            IGameplayManager gameplayManager,
            Kernel_Logger kernel_Logger)
        {
            _gameplayManager = gameplayManager;
            _logger = kernel_Logger.ForContext<SetGameplaySceneReadyPacketHandler>();
        }

        public override void Handle(IPlayer sender, SetGameplaySceneReadyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SetGameplaySceneReadyPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            _gameplayManager.HandleGameSceneLoaded(sender, packet);
        }
    }
}
