using System.Diagnostics.CodeAnalysis;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IMatchmakingServerRegistry
    {
        bool AddMatchmakingServer(IMatchmakingServer matchmakingServer);
        bool RemoveMatchmakingServer(IMatchmakingServer matchmakingServer);

        IMatchmakingServer GetMatchmakingServer(string secret);

        bool TryGetMatchmakingServer(string secret, [MaybeNullWhen(false)] out IMatchmakingServer matchmakingServer);
    }
}
