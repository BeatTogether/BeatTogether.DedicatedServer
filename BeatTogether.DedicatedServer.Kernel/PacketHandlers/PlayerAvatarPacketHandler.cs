using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
    public sealed class PlayerAvatarPacketHandler : BasePacketHandler<PlayerAvatarPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly IDedicatedInstance _instance;
        private readonly Internal_Logger _logger;

        public PlayerAvatarPacketHandler(
            IPacketDispatcher packetDispatcher, IDedicatedInstance instance, Kernel_Logger kernel_Logger)
        {
            _packetDispatcher = packetDispatcher;
            _instance = instance;
            _logger = kernel_Logger.ForContext<PlayerAvatarPacketHandler>();
        }

        public override void Handle(IPlayer sender, PlayerAvatarPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(PlayerAvatarPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            sender.Avatar = packet.PlayerAvatar;
        }
    }
}
