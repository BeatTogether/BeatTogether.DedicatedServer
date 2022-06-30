using System;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;
using BeatTogether.LiteNetLib.Enums;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
    public sealed class PlayerIdentityPacketHandler : BasePacketHandler<PlayerIdentityPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<PlayerIdentityPacketHandler>();

        public PlayerIdentityPacketHandler(
            IPacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
        }

        public override Task Handle(IPlayer sender, PlayerIdentityPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(PlayerIdentityPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            lock (sender.PlayerIdentityLock)
            {
                sender.Avatar = packet.PlayerAvatar;
                sender.State = packet.PlayerState;
                sender.Random = packet.Random.Data ?? Array.Empty<byte>();
                sender.PublicEncryptionKey = packet.PublicEncryptionKey.Data ?? Array.Empty<byte>();
                _packetDispatcher.SendFromPlayer(sender, packet, DeliveryMethod.ReliableOrdered);
            }
            return Task.CompletedTask;
        }
    }
}
