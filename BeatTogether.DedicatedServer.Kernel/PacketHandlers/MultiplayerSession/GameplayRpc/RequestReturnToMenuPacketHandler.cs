using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.GameplayRpc
{
    public sealed class RequestReturnToMenuPacketHandler : BasePacketHandler<RequestReturnToMenuPacket>
    {
        private IMatchmakingServer _server;
        private IGameplayManager _gameplayManager;
        private ILogger _logger = Log.ForContext<SetGameplaySceneReadyPacketHandler>();

        public RequestReturnToMenuPacketHandler(
            IMatchmakingServer server,
            IGameplayManager gameplayManager)
        {
            _server = server;
            _gameplayManager = gameplayManager;
        }

        public override Task Handle(IPlayer sender, RequestReturnToMenuPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(RequestReturnToMenuPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            if (sender.UserId == _server.ManagerId)
                _gameplayManager.SignalRequestReturnToMenu();

            return Task.CompletedTask;
        }
    }
}
