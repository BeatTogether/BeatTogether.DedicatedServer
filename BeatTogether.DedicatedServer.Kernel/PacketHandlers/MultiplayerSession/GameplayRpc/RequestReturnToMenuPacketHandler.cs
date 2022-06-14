using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.GameplayRpc
{
    public sealed class RequestReturnToMenuPacketHandler : BasePacketHandler<RequestReturnToMenuPacket>
    {
        private IGameplayManager _gameplayManager;
        private ILogger _logger = Log.ForContext<RequestReturnToMenuPacketHandler>();

        public RequestReturnToMenuPacketHandler(
            IGameplayManager gameplayManager)
        {
            _gameplayManager = gameplayManager;
        }

        public override Task Handle(IPlayer sender, RequestReturnToMenuPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(RequestReturnToMenuPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            if (sender.IsManager)
                _gameplayManager.SignalRequestReturnToMenu();

            return Task.CompletedTask;
        }
    }
}
