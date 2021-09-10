using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
    public sealed class PlayerStatePacketHandler : BasePacketHandler<PlayerStatePacket>
    {
        private readonly ILogger _logger = Log.ForContext<PlayerStatePacketHandler>();

        public override Task Handle(IPlayer sender, PlayerStatePacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(PlayerStatePacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            sender.State = packet.PlayerState;
            return Task.CompletedTask;
        }
    }
}
