using BeatTogether.DedicatedServer.Domain.Models;

namespace BeatTogether.DedicatedServer.Data.Abstractions
{
    public interface IMatchmakingServerRepository
    {
        bool CreateMatchmakingServer(MatchmakingServer matchmakingServer);
        bool DeleteMatchmakingServer(string secret);
        MatchmakingServer? GetMatchmakingServer(string secret);
    }
}
