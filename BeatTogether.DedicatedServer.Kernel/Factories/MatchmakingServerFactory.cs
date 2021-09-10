using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Models;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BeatTogether.DedicatedServer.Kernel.Factories
{
    public sealed class MatchmakingServerFactory : IMatchmakingServerFactory
    {
        private readonly IMatchmakingServerRegistry _matchmakingServerRegistry;
        private readonly IServiceProvider _serviceProvider;

        public MatchmakingServerFactory(
            IMatchmakingServerRegistry matchmakingServerRegistry,
            IServiceProvider serviceProvider)
        {
            _matchmakingServerRegistry = matchmakingServerRegistry;
            _serviceProvider = serviceProvider;
        }

        public IMatchmakingServer? CreateMatchmakingServer(
            string secret,
            string managerId,
            GameplayServerConfiguration configuration)
        {
            var matchmakingServer = _serviceProvider.GetRequiredService<MatchmakingServer>();
            matchmakingServer.Init(secret, managerId, configuration);
            if (!_matchmakingServerRegistry.AddMatchmakingServer(matchmakingServer))
                return null;
            return matchmakingServer;
        }
    }
}
