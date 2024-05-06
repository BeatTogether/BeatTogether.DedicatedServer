using BeatTogether.Core.ServerMessaging.Models;

namespace BeatTogether.DedicatedServer.Interface.Requests
{
    public record CreateMatchmakingServerRequest(
        Server Server);
}
