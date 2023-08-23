using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record UpdateInstanceConfigEvent(
        string Secret, //Cannot change the secret
        string ServerName,
        GameplayServerConfiguration Configuration);
}
