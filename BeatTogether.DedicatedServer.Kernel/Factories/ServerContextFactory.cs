using BeatTogether.DedicatedServer.Kernel.Models;
using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Factories
{
    public sealed class ServerContextFactory : IServerContextFactory
    {
        private readonly IServerContextAccessor _serverContextAccessor;

        public ServerContextFactory(IServerContextAccessor serverContextAccessor)
        {
            _serverContextAccessor = serverContextAccessor;
        }

        public IServerContext Create(string secret, string managerId, GameplayServerConfiguration configuration)
        {
            var serverContext = new ServerContext(secret, managerId, configuration);
            _serverContextAccessor.Context = serverContext;
            return serverContext;
        }
    }
}
