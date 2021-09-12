using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using LiteNetLib;
using System;

namespace BeatTogether.DedicatedServer.Kernel.Managers
{
    public sealed class PermissionsManager : IPermissionsManager, IDisposable
    {
        public bool AllowBeatmapSelect { get; private set; } = true;
        public bool AllowVoteKick { get; private set; } = false;
        public bool AllowInvite { get; private set; } = true;

        public PlayersPermissionConfiguration Permissions { get; private set; } = new();

        private IMatchmakingServer _server;
        private IPlayerRegistry _playerRegistry;
        private IPacketDispatcher _packetDispatcher;

        public PermissionsManager(
            IMatchmakingServer server,
            IPlayerRegistry playerRegistry,
            IPacketDispatcher packetDispatcher)
        {
            _server = server;
            _playerRegistry = playerRegistry;
            _packetDispatcher = packetDispatcher;

            _playerRegistry.PlayerConnected += HandlePlayerConnected;
            _playerRegistry.PlayerDisconnected += HandlePlayerDisconnected;
        }

        public void Dispose()
        {
            _playerRegistry.PlayerConnected -= HandlePlayerConnected;
            _playerRegistry.PlayerDisconnected -= HandlePlayerDisconnected;
        }

        public void UpdatePermissions()
        {
            Permissions.PlayersPermission.Add(new PlayerPermissionConfiguration
            {
                UserId = _server.UserId,
                IsServerOwner = false,
                HasRecommendBeatmapsPermission = true,
                HasRecommendGameplayModifiersPermission = true,
                HasKickVotePermission = true,
                HasInvitePermission = true
            });

            foreach (IPlayer player in _playerRegistry.Players)
            {
                var playerPermission = new PlayerPermissionConfiguration
                {
                    UserId = player.UserId,
                    IsServerOwner = player.UserId == _server.ManagerId,
                    HasRecommendBeatmapsPermission = AllowBeatmapSelect,
                    HasRecommendGameplayModifiersPermission = _server.Configuration.GameplayServerControlSettings == Enums.GameplayServerControlSettings.AllowModifierSelection || _server.Configuration.GameplayServerControlSettings == Enums.GameplayServerControlSettings.All,
                    HasKickVotePermission = AllowVoteKick,
                    HasInvitePermission = AllowInvite
                };
                Permissions.PlayersPermission.Add(playerPermission);
            }
        }

        private void HandlePlayerConnected(IPlayer player)
        {
            UpdatePermissions();
            var permissionConfigurationPacket = new SetPlayersPermissionConfigurationPacket
            {
                PermissionConfiguration = Permissions
            };
            _packetDispatcher.SendToNearbyPlayers(permissionConfigurationPacket, DeliveryMethod.ReliableOrdered);
        }

        private void HandlePlayerDisconnected(IPlayer player)
        {
            UpdatePermissions();
            var permissionConfigurationPacket = new SetPlayersPermissionConfigurationPacket
            {
                PermissionConfiguration = Permissions
            };
            _packetDispatcher.SendToNearbyPlayers(permissionConfigurationPacket, DeliveryMethod.ReliableOrdered);
        }
    }
}
