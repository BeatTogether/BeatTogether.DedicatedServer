using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.GameplayRpc
{
    public sealed class LevelFinishedPacketHandler : BasePacketHandler<LevelFinishedPacket>
    {
        private IGameplayManager _gameplayManager;
        private ILogger _logger = Log.ForContext<LevelFinishedPacketHandler>();

        public LevelFinishedPacketHandler(
            IGameplayManager gameplayManager)
        {
            _gameplayManager = gameplayManager;
        }

        public override Task Handle(IPlayer sender, LevelFinishedPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(LevelFinishedPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            _gameplayManager.HandleLevelFinished(sender, packet);
            return Task.CompletedTask;
        }
    }
}
