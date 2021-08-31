using BeatTogether.DedicatedServer.Kernel.Models;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IServerContextFactory
    {
        IServerContext Create(string secret, string managerId, GameplayServerConfiguration configuration);
    }
}
