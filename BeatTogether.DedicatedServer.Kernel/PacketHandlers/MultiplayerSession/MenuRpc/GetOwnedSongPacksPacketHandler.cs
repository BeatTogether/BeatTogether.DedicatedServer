using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetOwnedSongPacksPacketHandler : BasePacketHandler<GetOwnedSongPacksPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<GetOwnedSongPacksPacketHandler>();

        public GetOwnedSongPacksPacketHandler(IPacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
        }

        public override Task Handle(IPlayer sender, GetOwnedSongPacksPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetOwnedSongPacksPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            // TODO
            return Task.CompletedTask;
        }
    }
}
