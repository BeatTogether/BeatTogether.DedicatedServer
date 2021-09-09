using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.GameplayRpc
{
    public sealed class SetGameplaySongReadyPacketHandler : BasePacketHandler<SetGameplaySongReadyPacket>
    {
        private ILogger _logger = Log.ForContext<SetGameplaySongReadyPacketHandler>();

        public override Task Handle(IPlayer sender, SetGameplaySongReadyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SetGameplaySongReadyPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            return Task.CompletedTask;
        }
    }
}
