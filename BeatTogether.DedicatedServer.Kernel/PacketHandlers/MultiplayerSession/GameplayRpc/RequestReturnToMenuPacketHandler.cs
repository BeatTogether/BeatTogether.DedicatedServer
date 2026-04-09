using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.GameplayRpc
{
    public sealed class RequestReturnToMenuPacketHandler : BasePacketHandler<RequestReturnToMenuPacket>
    {
        private readonly IGameplayManager _gameplayManager;
        private readonly Internal_Logger _logger;

        public RequestReturnToMenuPacketHandler(
            IGameplayManager gameplayManager,
            Kernel_Logger kernel_Logger)
        {
            _gameplayManager = gameplayManager;
            _logger = kernel_Logger.ForContext<RequestReturnToMenuPacketHandler>();
        }

        public override void Handle(IPlayer sender, RequestReturnToMenuPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(RequestReturnToMenuPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            if (sender.IsServerOwner)
                _gameplayManager.SignalRequestReturnToMenu();
        }
    }
}
