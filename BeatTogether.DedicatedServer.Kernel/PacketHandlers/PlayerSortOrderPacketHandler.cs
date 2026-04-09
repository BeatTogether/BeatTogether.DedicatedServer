using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
	public sealed class PlayerSortOrderPacketHandler : BasePacketHandler<PlayerSortOrderPacket>
	{
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly Internal_Logger _logger;//

        public PlayerSortOrderPacketHandler(IPacketDispatcher packetDispatcher, Kernel_Logger kernel_Logger)
        {
            _packetDispatcher = packetDispatcher;
            _logger = kernel_Logger.ForContext<PlayerSortOrderPacketHandler>(); ;
        }

        public override void Handle(IPlayer sender, PlayerSortOrderPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(PlayerSortOrderPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            if (sender.HashedUserId == packet.UserId && sender.SortIndex != packet.SortIndex) //If they send themselves as being in the wrong place, correct them. Although this probably shouldnt have a handler
            {
                packet.SortIndex = sender.SortIndex;
                _packetDispatcher.SendToPlayer(sender, packet, IgnoranceChannelTypes.Reliable);
            }
                
        }
    }
}
