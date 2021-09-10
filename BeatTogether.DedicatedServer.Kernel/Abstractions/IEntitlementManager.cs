using BeatTogether.DedicatedServer.Messaging.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IEntitlementManager
    {
        ConcurrentDictionary<string, ConcurrentDictionary<string, EntitlementStatus>> userEntitlements { get; }

        bool AllPlayersHaveBeatmap(string beatmap);
        List<string> GetPlayersWithoutBeatmap(string beatmap);
        void HandleEntitlement(string userId, string beatmap, EntitlementStatus entitlement);
    }
}
