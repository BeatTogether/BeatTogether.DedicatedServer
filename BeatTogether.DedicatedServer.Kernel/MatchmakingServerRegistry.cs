using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class MatchmakingServerRegistry : IMatchmakingServerRegistry
    {
        private readonly ConcurrentDictionary<string, IMatchmakingServer> _matchmakingServers = new();

        public bool AddMatchmakingServer(IMatchmakingServer matchmakingServer) =>
            _matchmakingServers.TryAdd(matchmakingServer.Secret, matchmakingServer);

        public bool RemoveMatchmakingServer(IMatchmakingServer matchmakingServer) =>
            _matchmakingServers.TryRemove(matchmakingServer.Secret, out _);

        public IMatchmakingServer GetMatchmakingServer(string secret) =>
            _matchmakingServers[secret];

        public bool TryGetMatchmakingServer(string secret, [MaybeNullWhen(false)] out IMatchmakingServer matchmakingServer) =>
            _matchmakingServers.TryGetValue(secret, out matchmakingServer);
    }
}
