using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetIsInLobbyPacketHandler : BasePacketHandler<GetIsInLobbyPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<GetIsReadyPacketHandler>();

        public GetIsInLobbyPacketHandler(IPacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
        }

        public override Task Handle(IPlayer sender, GetIsInLobbyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetIsInLobbyPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            // TODO
            return Task.CompletedTask;
        }
    }
}
