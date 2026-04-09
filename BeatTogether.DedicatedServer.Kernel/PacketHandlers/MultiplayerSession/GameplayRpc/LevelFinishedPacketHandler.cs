using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.GameplayRpc
{
    public sealed class LevelFinishedPacketHandler : BasePacketHandler<LevelFinishedPacket>
    {
        private readonly IGameplayManager _gameplayManager;
        private readonly Internal_Logger _logger;

        public LevelFinishedPacketHandler(
            IGameplayManager gameplayManager,
            Kernel_Logger kernel_Logger)
        {
            _gameplayManager = gameplayManager;
            _logger = kernel_Logger.ForContext<LevelFinishedPacketHandler>();
        }

        public override void Handle(IPlayer sender, LevelFinishedPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(LevelFinishedPacket)}' " +
                $"(SenderId={sender.ConnectionId}, HasValue0={packet.HasValue0}, HasAnyResult={packet.Results.HasAnyResult()}, " +
                $"ModifiedScore={(packet.HasValue0 && packet.Results.HasAnyResult() ? packet.Results.LevelCompletionResults.ModifiedScore : "NoValue/NoResults" )}, " +
                $"MultipliedScore={(packet.HasValue0 && packet.Results.HasAnyResult() ? packet.Results.LevelCompletionResults.MultipliedScore : "NoValue/NoResults")})."
            );

            _gameplayManager.HandleLevelFinished(sender, packet);
        }
    }
}
