using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.GameplayRpc
{
    public sealed class SetGameplaySongReadyPacketHandler : BasePacketHandler<SetGameplaySongReadyPacket>
    {
        private readonly IGameplayManager _gameplayManager;
        private readonly ILogger _logger = Log.ForContext<SetGameplaySongReadyPacketHandler>();

        public SetGameplaySongReadyPacketHandler(
            IGameplayManager gameplayManager)
        {
            _gameplayManager = gameplayManager;
        }

        public override void Handle(IPlayer sender, SetGameplaySongReadyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SetGameplaySongReadyPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            _gameplayManager.HandleGameSongLoaded(sender);
        }
    }
}
