using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record FromServerCreateServerEvent(
        GameplayServerConfiguration GameplayServerConfiguration,
        string secret,
        string code,
        string UserId
     );
}
