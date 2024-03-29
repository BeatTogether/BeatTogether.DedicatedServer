﻿using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using BeatTogether.LiteNetLib.Enums;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetRecommendedBeatmapPacketHandler : BasePacketHandler<GetRecommendedBeatmapPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<GetRecommendedBeatmapPacketHandler>();

        public GetRecommendedBeatmapPacketHandler(
            IPacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
        }

        public override Task Handle(IPlayer sender, GetRecommendedBeatmapPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetRecommendedBeatmapPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            if(sender.BeatmapIdentifier != null)
                _packetDispatcher.SendToPlayer(sender, new SetRecommendedBeatmapPacket
                {
                    BeatmapIdentifier = sender.BeatmapIdentifier
                }, DeliveryMethod.ReliableOrdered);

            return Task.CompletedTask;
        }
    }
}
