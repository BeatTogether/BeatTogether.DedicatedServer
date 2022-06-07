using System.Net;

namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record NodeOnlineEvent(string endPoint);
}