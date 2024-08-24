using BeatTogether.Core.ServerMessaging.Models;

namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record UpdateInstanceConfigEvent(
        Server ServerInsance);
}
