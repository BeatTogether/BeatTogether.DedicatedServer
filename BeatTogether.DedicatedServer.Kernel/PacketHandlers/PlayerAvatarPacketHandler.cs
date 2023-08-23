using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
    public sealed class PlayerAvatarPacketHandler : BasePacketHandler<PlayerAvatarPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly IDedicatedInstance _instance;
        private readonly ILogger _logger = Log.ForContext<PlayerAvatarPacketHandler>();

        public PlayerAvatarPacketHandler(
            IPacketDispatcher packetDispatcher, IDedicatedInstance instance)
        {
            _packetDispatcher = packetDispatcher;
            _instance = instance;
        }

        public override async Task Handle(IPlayer sender, PlayerAvatarPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(PlayerAvatarPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            await sender.PlayerAccessSemaphore.WaitAsync();
            sender.Avatar = packet.PlayerAvatar;
            sender.PlayerAccessSemaphore.Release();
        }
    }
}
