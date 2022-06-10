using System.Net;

namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record NodeStartedEvent(string endPoint);
}