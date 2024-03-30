using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using BeatTogether.LiteNetLib.Enums;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
	public sealed class RequestKickPlayerPacketHandler : BasePacketHandler<RequestKickPlayerPacket>
	{
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<RequestKickPlayerPacketHandler>();

        public RequestKickPlayerPacketHandler(
            IPlayerRegistry playerRegistry,
            IPacketDispatcher packetDispatcher)
        {
            _playerRegistry = playerRegistry;
            _packetDispatcher = packetDispatcher;
        }

        public override void Handle(IPlayer sender, RequestKickPlayerPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(RequestKickPlayerPacket)}' " +
                $"(SenderId={sender.ConnectionId}, KickedPlayerId={packet.KickedPlayerId})."
            );
            if (sender.CanKickVote)
                if (_playerRegistry.TryGetPlayer(packet.KickedPlayerId, out var kickedPlayer))
                    _packetDispatcher.SendToPlayer(kickedPlayer, new KickPlayerPacket
                    {
                        DisconnectedReason = DisconnectedReason.Kicked
                    }, DeliveryMethod.ReliableOrdered);

        }
    }
}
