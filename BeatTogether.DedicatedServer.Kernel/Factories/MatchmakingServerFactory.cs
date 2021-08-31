using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Models;

namespace BeatTogether.DedicatedServer.Kernel.Factories
{
    public sealed class MatchmakingServerFactory : IMatchmakingServerFactory
    {
        private readonly IMatchmakingServerRegistry _matchmakingServerRegistry;
        private readonly IPortAllocator _portAllocator;
        private readonly PacketEncryptionLayer _packetEncryptionLayer;
        private readonly IPacketSource _packetSource;
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IServerContextFactory _serverContextFactory;

        public MatchmakingServerFactory(
            IMatchmakingServerRegistry matchmakingServerRegistry,
            IPortAllocator portAllocator,
            PacketEncryptionLayer packetEncryptionLayer,
            IPacketSource packetSource,
            IPacketDispatcher packetDispatcher,
            IPlayerRegistry playerRegistry,
            IServerContextFactory serverContextFactory)
        {
            _matchmakingServerRegistry = matchmakingServerRegistry;
            _portAllocator = portAllocator;
            _packetEncryptionLayer = packetEncryptionLayer;
            _packetSource = packetSource;
            _packetDispatcher = packetDispatcher;
            _playerRegistry = playerRegistry;
            _serverContextFactory = serverContextFactory;
        }

        public IMatchmakingServer? CreateMatchmakingServer(
            string secret,
            string managerId,
            GameplayServerConfiguration configuration)
        {
            var matchmakingServer = new MatchmakingServer(
                _portAllocator, _packetEncryptionLayer, _packetSource, 
                _packetDispatcher, _playerRegistry, _serverContextFactory,
                secret, managerId, configuration);
            if (!_matchmakingServerRegistry.AddMatchmakingServer(matchmakingServer))
                return null;
            return matchmakingServer;
        }
    }
}
