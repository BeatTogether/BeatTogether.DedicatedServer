
using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record UpdateInstanceConfigEvent(
        string Secret, //Cannot change the secret
        string Code, //Use special mod to change this (patreon only)
        string ServerName,
        GameplayServerConfiguration Configuration);
}
