using System;
using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
    public sealed class PlayerIdentityPacketHandler : BasePacketHandler<PlayerIdentityPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly IDedicatedInstance _instance;
        private readonly Internal_Logger _logger;

        public PlayerIdentityPacketHandler(
            IPacketDispatcher packetDispatcher, IDedicatedInstance instance, Kernel_Logger kernel_Logger)
        {
            _packetDispatcher = packetDispatcher;
            _instance = instance;
            _logger = kernel_Logger.ForContext<PlayerIdentityPacketHandler>();
        }

        public override void Handle(IPlayer sender, PlayerIdentityPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(PlayerIdentityPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            sender.Avatar = packet.GameSpecificData.PlayerAvatar;
            sender.State = packet.PlayerState;
            sender.Random = packet.Random.Data ?? Array.Empty<byte>();
            sender.PublicEncryptionKey = packet.PublicEncryptionKey.Data ?? Array.Empty<byte>();
            _packetDispatcher.SendFromPlayer(sender, packet, IgnoranceChannelTypes.Reliable);
        }
    }
}
