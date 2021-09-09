using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using LiteNetLib;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetMultiplayerGameStatePacketHandler : BasePacketHandler<GetMultiplayerGameStatePacket>
    {
        private readonly IMatchmakingServer _server;
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<GetMultiplayerGameStatePacketHandler>();

        public GetMultiplayerGameStatePacketHandler(
            IMatchmakingServer server,
            IPacketDispatcher packetDispatcher)
        {
            _server = server;
            _packetDispatcher = packetDispatcher;
        }

        public override Task Handle(IPlayer sender, GetMultiplayerGameStatePacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetMultiplayerGameStatePacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            var gameStatePacket = new SetMultiplayerGameStatePacket
            {
                State = _server.State
            };
            _packetDispatcher.SendToPlayer(sender, gameStatePacket, DeliveryMethod.ReliableOrdered);

            return Task.CompletedTask;
        }
    }
}
