using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.GameplayRpc
{
    public sealed class RequestReturnToMenuPacketHandler : BasePacketHandler<RequestReturnToMenuPacket>
    {
        private readonly IGameplayManager _gameplayManager;
        private readonly ILogger _logger = Log.ForContext<RequestReturnToMenuPacketHandler>();

        public RequestReturnToMenuPacketHandler(
            IGameplayManager gameplayManager)
        {
            _gameplayManager = gameplayManager;
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
