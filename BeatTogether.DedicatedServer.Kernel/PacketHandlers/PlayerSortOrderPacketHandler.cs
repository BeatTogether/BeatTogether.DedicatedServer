using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;
using BeatTogether.LiteNetLib.Enums;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
	public sealed class PlayerSortOrderPacketHandler : BasePacketHandler<PlayerSortOrderPacket>
	{
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<PlayerSortOrderPacketHandler>();

        public PlayerSortOrderPacketHandler(IPacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
        }

        public override Task Handle(IPlayer sender, PlayerSortOrderPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(PlayerSortOrderPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            if (sender.UserId == packet.UserId && sender.SortIndex != packet.SortIndex)
			{
                sender.SortIndex = packet.SortIndex;
                _packetDispatcher.SendExcludingPlayer(sender, packet, DeliveryMethod.ReliableOrdered);
            }

            return Task.CompletedTask;
        }
    }
}
