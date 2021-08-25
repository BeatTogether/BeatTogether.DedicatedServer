using BeatTogether.DedicatedServer.Kernel.Models;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IMatchmakingServerFactory
    {
        IMatchmakingServer? CreateMatchmakingServer(
            string secret,
            string managerId,
            GameplayServerConfiguration configuration);
    }
}
