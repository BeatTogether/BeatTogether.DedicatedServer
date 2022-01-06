using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using BeatTogether.LiteNetLib.Enums;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetMultiplayerGameStatePacketHandler : BasePacketHandler<GetMultiplayerGameStatePacket>
    {
        private readonly IDedicatedServer _server;
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<GetMultiplayerGameStatePacketHandler>();

        public GetMultiplayerGameStatePacketHandler(
            IDedicatedServer server,
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

            _packetDispatcher.SendToPlayer(sender, new SetMultiplayerGameStatePacket
            {
                State = _server.State
            }, DeliveryMethod.ReliableOrdered);

            return Task.CompletedTask;
        }
    }
}
