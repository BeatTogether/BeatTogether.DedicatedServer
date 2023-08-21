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
        private readonly IDedicatedInstance _instance;
        private readonly ILogger _logger = Log.ForContext<PlayerIdentityPacketHandler>();

        public PlayerIdentityPacketHandler(
            IPacketDispatcher packetDispatcher, IDedicatedInstance instance)
        {
            _packetDispatcher = packetDispatcher;
            _instance = instance;
        }

        public override async Task Handle(IPlayer sender, PlayerIdentityPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(PlayerIdentityPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            await sender.PlayerAccessSemaphore.WaitAsync();
            sender.Avatar = packet.PlayerAvatar;
            sender.State = packet.PlayerState;
            sender.Random = packet.Random.Data ?? Array.Empty<byte>();
            sender.PublicEncryptionKey = packet.PublicEncryptionKey.Data ?? Array.Empty<byte>();
            if (!sender.PlayerInitialised.Task.IsCompleted)
                sender.PlayerInitialised.SetResult();
            sender.PlayerAccessSemaphore.Release();
            _packetDispatcher.SendFromPlayer(sender, packet, DeliveryMethod.ReliableOrdered);
        }
    }
}
