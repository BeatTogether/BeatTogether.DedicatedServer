using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Models;

namespace BeatTogether.DedicatedServer.Kernel.Factories
{
    public sealed class MatchmakingServerFactory : IMatchmakingServerFactory
    {
        private readonly IMatchmakingServerRegistry _matchmakingServerRegistry;
        private readonly IPortAllocator _portAllocator;
        private readonly PacketEncryptionLayer _packetEncryptionLayer;
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly IServiceAccessor<IMatchmakingServer> _matchmakingServerAccessor;
        private readonly IServiceAccessor<IPlayerRegistry> _playerRegistryAccessor;
        private readonly IServiceAccessor<IPacketSource> _packetSourceAccessor;
        private readonly IServiceAccessor<IPermissionsManager> _permissionsManagerAccessor;

        public MatchmakingServerFactory(
            IMatchmakingServerRegistry matchmakingServerRegistry,
            IPortAllocator portAllocator,
            PacketEncryptionLayer packetEncryptionLayer,
            IPacketDispatcher packetDispatcher,
            IServiceAccessor<IMatchmakingServer> matchmakingServerAccessor,
            IServiceAccessor<IPlayerRegistry> playerRegistryAccessor,
            IServiceAccessor<IPacketSource> packetSourceAccessor,
            IServiceAccessor<IPermissionsManager> permissionsManagerAccessor)
        {
            _matchmakingServerRegistry = matchmakingServerRegistry;
            _portAllocator = portAllocator;
            _packetEncryptionLayer = packetEncryptionLayer;
            _packetDispatcher = packetDispatcher;
            _matchmakingServerAccessor = matchmakingServerAccessor;
            _playerRegistryAccessor = playerRegistryAccessor;
            _packetSourceAccessor = packetSourceAccessor;
            _permissionsManagerAccessor = permissionsManagerAccessor;
        }

        public IMatchmakingServer? CreateMatchmakingServer(
            string secret,
            string managerId,
            GameplayServerConfiguration configuration)
        {
            var matchmakingServer = new MatchmakingServer(
                _portAllocator, _packetEncryptionLayer, _packetDispatcher, 
                _matchmakingServerAccessor, _playerRegistryAccessor, _packetSourceAccessor,
                _permissionsManagerAccessor,
                secret, managerId, configuration);
            if (!_matchmakingServerRegistry.AddMatchmakingServer(matchmakingServer))
                return null;
            return matchmakingServer;
        }
    }
}
