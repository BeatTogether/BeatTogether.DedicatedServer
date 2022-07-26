
using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record UpdateInstanceConfigEvent(
        string Secret, //Cannot change the secret
        string Code, //Use special mod to change this (patreon only or something)
        string ServerName,
        GameplayServerConfiguration Configuration);
}
