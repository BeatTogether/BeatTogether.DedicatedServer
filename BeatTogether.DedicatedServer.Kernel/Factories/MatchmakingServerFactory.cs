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

        public MatchmakingServerFactory(
            IMatchmakingServerRegistry matchmakingServerRegistry,
            IPortAllocator portAllocator,
            PacketEncryptionLayer packetEncryptionLayer,
            IPacketSource packetSource,
            IPacketDispatcher packetDispatcher,
            IPlayerRegistry playerRegistry)
        {
            _matchmakingServerRegistry = matchmakingServerRegistry;
            _portAllocator = portAllocator;
            _packetEncryptionLayer = packetEncryptionLayer;
            _packetSource = packetSource;
            _packetDispatcher = packetDispatcher;
            _playerRegistry = playerRegistry;
        }

        public IMatchmakingServer? CreateMatchmakingServer(
            string secret,
            string managerId,
            GameplayServerConfiguration configuration)
        {
            var matchmakingServer = new MatchmakingServer(
                _portAllocator, _packetEncryptionLayer,
                _packetSource, _packetDispatcher, _playerRegistry,
                secret, managerId, configuration);
            if (!_matchmakingServerRegistry.AddMatchmakingServer(matchmakingServer))
                return null;
            return matchmakingServer;
        }
    }
}
