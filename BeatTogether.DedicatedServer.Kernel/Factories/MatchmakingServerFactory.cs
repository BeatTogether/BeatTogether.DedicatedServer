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
        private readonly IServiceAccessor<IServerContext> _serverContextAccessor;
        private readonly IServiceAccessor<IPlayerRegistry> _playerRegistryAccessor;
        private readonly IServiceAccessor<IPacketSource> _packetSourceAccessor;

        public MatchmakingServerFactory(
            IMatchmakingServerRegistry matchmakingServerRegistry,
            IPortAllocator portAllocator,
            PacketEncryptionLayer packetEncryptionLayer,
            IPacketDispatcher packetDispatcher,
            IServiceAccessor<IServerContext> serverContextAccessor,
            IServiceAccessor<IPlayerRegistry> playerRegistryAccessor,
            IServiceAccessor<IPacketSource> packetSourceAccessor)
        {
            _matchmakingServerRegistry = matchmakingServerRegistry;
            _portAllocator = portAllocator;
            _packetEncryptionLayer = packetEncryptionLayer;
            _packetDispatcher = packetDispatcher;
            _serverContextAccessor = serverContextAccessor;
            _playerRegistryAccessor = playerRegistryAccessor;
            _packetSourceAccessor = packetSourceAccessor;
        }

        public IMatchmakingServer? CreateMatchmakingServer(
            string secret,
            string managerId,
            GameplayServerConfiguration configuration)
        {
            var matchmakingServer = new MatchmakingServer(
                _portAllocator, _packetEncryptionLayer, _packetDispatcher, 
                _serverContextAccessor, _playerRegistryAccessor, _packetSourceAccessor,
                secret, managerId, configuration);
            if (!_matchmakingServerRegistry.AddMatchmakingServer(matchmakingServer))
                return null;
            return matchmakingServer;
        }
    }
}
