using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.GameplayRpc
{
    public sealed class SetGameplaySongReadyPacketHandler : BasePacketHandler<SetGameplaySongReadyPacket>
    {
        private IGameplayManager _gameplayManager;
        private ILogger _logger = Log.ForContext<SetGameplaySongReadyPacketHandler>();

        public SetGameplaySongReadyPacketHandler(
            IGameplayManager gameplayManager)
        {
            _gameplayManager = gameplayManager;
        }

        public override Task Handle(IPlayer sender, SetGameplaySongReadyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SetGameplaySongReadyPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            _gameplayManager.HandleGameSongLoaded(sender);
            return Task.CompletedTask;
        }
    }
}
