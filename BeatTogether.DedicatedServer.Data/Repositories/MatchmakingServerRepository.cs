using System.Collections.Concurrent;
using BeatTogether.DedicatedServer.Data.Abstractions;
using BeatTogether.DedicatedServer.Domain.Models;

namespace BeatTogether.DedicatedServer.Data.Repositories
{
    public sealed class MatchmakingServerRepository : IMatchmakingServerRepository
    {
        private readonly ConcurrentDictionary<string, MatchmakingServer> _matchmakingServers = new();

        public bool CreateMatchmakingServer(MatchmakingServer matchmakingServer) =>
            _matchmakingServers.TryAdd(matchmakingServer.Secret, matchmakingServer);

        public bool DeleteMatchmakingServer(string secret) =>
            _matchmakingServers.TryRemove(secret, out _);

        public MatchmakingServer? GetMatchmakingServer(string secret)
        {
            if (!_matchmakingServers.TryGetValue(secret, out var matchmakingServer))
                return null;
            return matchmakingServer;
        }
    }
}
