using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.GameplayRpc
{
    public sealed class LevelFinishedPacketHandler : BasePacketHandler<LevelFinishedPacket>
    {
        private readonly IGameplayManager _gameplayManager;
        private readonly ILogger _logger = Log.ForContext<LevelFinishedPacketHandler>();

        public LevelFinishedPacketHandler(
            IGameplayManager gameplayManager)
        {
            _gameplayManager = gameplayManager;
        }

        public override void Handle(IPlayer sender, LevelFinishedPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(LevelFinishedPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            _gameplayManager.HandleLevelFinished(sender, packet);
        }
    }
}
