using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BeatTogether.DedicatedServer.Kernel.Managers
{
    public sealed class EntitlementManager : IEntitlementManager
    {
        public ConcurrentDictionary<string, ConcurrentDictionary<string, EntitlementStatus>> userEntitlements { get; } = new();

        public bool AllPlayersHaveBeatmap(string beatmap)
            => userEntitlements.Values.All(p => p[beatmap] != EntitlementStatus.NotOwned);

        public List<string> GetPlayersWithoutBeatmap(string beatmap)
            => userEntitlements.Where(userEntitlement => !userEntitlement.Value.ContainsKey(beatmap)).Select(userEntitlement => userEntitlement.Key).ToList();

        public void HandleEntitlement(string userId, string beatmap, EntitlementStatus entitlement)
            => userEntitlements.GetOrAdd(userId, _ => new ConcurrentDictionary<string, EntitlementStatus>())[beatmap] = entitlement;
    }
}
