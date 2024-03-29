﻿using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using BeatTogether.LiteNetLib.Enums;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetRecommendedModifiersPacketHandler : BasePacketHandler<GetRecommendedModifiersPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<GetRecommendedModifiersPacketHandler>();

        public GetRecommendedModifiersPacketHandler(
            IPacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
        }

        public override Task Handle(IPlayer sender, GetRecommendedModifiersPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetRecommendedModifiersPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            _packetDispatcher.SendToPlayer(sender, new SetRecommendedModifiersPacket
            {
                Modifiers = sender.Modifiers
            }, DeliveryMethod.ReliableOrdered);

            return Task.CompletedTask;
        }
    }
}
