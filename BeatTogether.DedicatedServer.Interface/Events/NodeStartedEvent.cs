using System;

namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record NodeStartedEvent(string endPoint, string NodeVersion);
}