using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;
using LiteNetLib;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
    class PlayerLatencyPacketHandler : BasePacketHandler<PlayerLatencyPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<PlayerLatencyPacketHandler>();

        public PlayerLatencyPacketHandler(IPacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
        }

        public override Task Handle(IPlayer sender, PlayerLatencyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SyncTimePacket)}' " +
                $"(SenderId={sender.ConnectionId}, SyncTime={packet.Latency})."
            );

            sender.Latency.Update(packet.Latency);
            var playerLatencyPacket = new PlayerLatencyPacket
            {
                Latency = sender.Latency.CurrentAverage
            };
            _packetDispatcher.SendFromPlayer(sender, playerLatencyPacket, DeliveryMethod.ReliableOrdered);

            return Task.CompletedTask;
        }
    }
}
