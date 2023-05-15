﻿using BeatTogether.DedicatedServer.Kernel.Abstractions;
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

        public override async Task Handle(IPlayer sender, PlayerSortOrderPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(PlayerSortOrderPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            await sender.PlayerAccessSemaphore.WaitAsync();
            if (sender.UserId == packet.UserId && sender.SortIndex != packet.SortIndex) //If they send themselves as being in the wrong place, correct them. Although this probably shouldnt have a handler
                {
                    packet.SortIndex = sender.SortIndex;
                    _packetDispatcher.SendToPlayer(sender, packet, DeliveryMethod.ReliableOrdered);
                }
            sender.PlayerAccessSemaphore.Release();
            return;
        }
    }
}
