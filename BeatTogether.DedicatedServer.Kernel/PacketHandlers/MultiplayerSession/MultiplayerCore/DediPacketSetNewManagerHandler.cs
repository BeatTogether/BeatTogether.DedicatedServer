using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;
using BeatTogether.LiteNetLib.Enums;
using Serilog;
using System.Linq;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    class DediPacketSetNewManagerPacketHandler : BasePacketHandler<DediPacketSetNewManagerPacket>
    {
        public InstanceConfiguration _configuration;
        public readonly IPacketDispatcher _packetDispatcher;
        public readonly IPlayerRegistry _playerRegistry;
        private readonly ILogger _logger = Log.ForContext<DediPacketSetNewManagerPacketHandler>();

        public DediPacketSetNewManagerPacketHandler(
            IPlayerRegistry playerRegistry,
            IPacketDispatcher packetDispatcher,
            InstanceConfiguration configuration)
        {
            _playerRegistry = playerRegistry;
            _packetDispatcher = packetDispatcher;
            _configuration = configuration;
        }

        public override void Handle(IPlayer sender, DediPacketSetNewManagerPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(DediPacketSetNewManagerPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            if (sender.IsServerOwner && _configuration.GameplayServerMode == Enums.GameplayServerMode.Managed)
            {
                _configuration.ServerOwnerId = packet.NewManagerID;

                _packetDispatcher.SendToNearbyPlayers(new SetPlayersPermissionConfigurationPacket
                {
                    PermissionConfiguration = new PlayersPermissionConfiguration
                    {
                        PlayersPermission = _playerRegistry.Players.Select(x => new PlayerPermissionConfiguration
                        {
                            UserId = x.UserId,
                            IsServerOwner = x.IsServerOwner,
                            HasRecommendBeatmapsPermission = x.CanRecommendBeatmaps,
                            HasRecommendGameplayModifiersPermission = x.CanRecommendModifiers,
                            HasKickVotePermission = x.CanKickVote,
                            HasInvitePermission = x.CanInvite
                        }).ToArray()
                    }
                }, DeliveryMethod.ReliableOrdered);
            }
        }
    }
}
