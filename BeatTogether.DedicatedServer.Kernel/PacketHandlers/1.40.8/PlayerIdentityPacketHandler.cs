using System;
using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
    public sealed class PlayerIdentityPacketHandler_1_40_8 : BasePacketHandler<PlayerIdentityPacket_1_40_8>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly IDedicatedInstance _instance;
        private readonly ILogger _logger = Log.ForContext<PlayerIdentityPacketHandler>();

        public PlayerIdentityPacketHandler_1_40_8(
            IPacketDispatcher packetDispatcher, IDedicatedInstance instance)
        {
            _packetDispatcher = packetDispatcher;
            _instance = instance;
        }

        public override void Handle(IPlayer sender, PlayerIdentityPacket_1_40_8 packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(PlayerIdentityPacket_1_40_8)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            sender.Avatar = packet.PlayerAvatar;
            sender.State = packet.PlayerState;
            sender.Random = packet.Random.Data ?? Array.Empty<byte>();
            sender.PublicEncryptionKey = packet.PublicEncryptionKey.Data ?? Array.Empty<byte>();
            _packetDispatcher.SendFromPlayer(sender, packet, IgnoranceChannelTypes.Reliable);
        }
    }
}
