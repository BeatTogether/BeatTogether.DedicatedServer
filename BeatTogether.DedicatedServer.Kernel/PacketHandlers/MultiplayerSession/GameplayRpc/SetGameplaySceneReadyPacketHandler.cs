using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.GameplayRpc
{
    public sealed class SetGameplaySceneReadyPacketHandler : BasePacketHandler<SetGameplaySceneReadyPacket>
    {
        private readonly IGameplayManager _gameplayManager;
        private readonly ILogger _logger = Log.ForContext<SetGameplaySceneReadyPacketHandler>();

        public SetGameplaySceneReadyPacketHandler(
            IGameplayManager gameplayManager)
        {
            _gameplayManager = gameplayManager;
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
