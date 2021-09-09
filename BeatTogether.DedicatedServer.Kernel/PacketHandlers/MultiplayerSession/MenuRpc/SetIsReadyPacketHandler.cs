﻿using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class SetIsReadyPacketHandler : BasePacketHandler<SetIsReadyPacket>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<SetIsReadyPacketHandler>();

        public SetIsReadyPacketHandler(
            IPlayerRegistry playerRegistry,
            IPacketDispatcher packetDispatcher)
        {
            _playerRegistry = playerRegistry;
            _packetDispatcher = packetDispatcher;
        }

        public override Task Handle(IPlayer sender, SetIsReadyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SetIsReadyPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            sender.IsReady = packet.IsReady;

            if (_playerRegistry.Players.All(player => player.IsReady))
			{
                var setStartGameTimePacket = new SetStartGameTimePacket
                {
                    StartTime = sender.MatchmakingServer.RunTime + 5
                };
                //_packetDispatcher.SendToNearbyPlayers( )

                return Task.CompletedTask;
			}

            return Task.CompletedTask;
        }
    }
}