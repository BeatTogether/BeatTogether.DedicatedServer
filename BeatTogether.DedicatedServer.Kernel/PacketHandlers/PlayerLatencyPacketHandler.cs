﻿using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;
using BeatTogether.LiteNetLib.Enums;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
    class PlayerLatencyPacketHandler : BasePacketHandler<PlayerLatencyPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<PlayerLatencyPacketHandler>();

        public PlayerLatencyPacketHandler(
            IPacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
        }

        public override async Task Handle(IPlayer sender, PlayerLatencyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SyncTimePacket)}' " +
                $"(SenderId={sender.ConnectionId}, SyncTime={packet.Latency})."
            );

            await sender.PlayerAccessSemaphore.WaitAsync();
            sender.Latency.Update(packet.Latency);
            sender.PlayerAccessSemaphore.Release();
            _packetDispatcher.SendFromPlayer(sender, new PlayerLatencyPacket
            {
                Latency = sender.Latency.CurrentAverage
            }, DeliveryMethod.ReliableOrdered);
            return;
        }
    }
}
