using System;

namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record NodeOnlineEvent(string endPoint, string NodeVersion);
}