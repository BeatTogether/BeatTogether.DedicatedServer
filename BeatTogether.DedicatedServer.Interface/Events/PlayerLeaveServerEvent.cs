using System.Net;

namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record PlayerLeaveServerEvent(string Secret, string UserId, string EndPoint);
}
