using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.GameplayRpc
{
    public sealed class SetGameplaySceneReadyPacketHandler : BasePacketHandler<SetGameplaySceneReadyPacket>
    {
        private IGameplayManager _gameplayManager;
        private ILogger _logger = Log.ForContext<SetGameplaySceneReadyPacketHandler>();

        public SetGameplaySceneReadyPacketHandler(
            IGameplayManager gameplayManager)
        {
            _gameplayManager = gameplayManager;
        }

        public override Task Handle(IPlayer sender, SetGameplaySceneReadyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SetGameplaySceneReadyPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            _gameplayManager.HandleGameSceneLoaded(sender, packet);
            return Task.CompletedTask;
        }
    }
}
