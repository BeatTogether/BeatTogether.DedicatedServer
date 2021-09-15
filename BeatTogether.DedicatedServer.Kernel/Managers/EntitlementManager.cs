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

        private IPlayerRegistry _playerRegistry;

        public EntitlementManager(
            IPlayerRegistry playerRegistry)
        {
            _playerRegistry = playerRegistry;
        }

        // If all players are in the entitlement dictionary and have the song entitlement registered with 'OK'
        public bool AllPlayersHaveBeatmap(string beatmap)
            => _playerRegistry.Players.All(p => userEntitlements.TryGetValue(p.UserId, out var userEntitlement) && userEntitlement.TryGetValue(beatmap, out var entitlement) && entitlement == EntitlementStatus.Ok);

        // If all players are in the entitlement dictionary and have a song entitlement other than 'NotOwned'
        public bool AllPlayersOwnBeatmap(string beatmap)
            => _playerRegistry.Players.All(p => userEntitlements.TryGetValue(p.UserId, out var userEntitlement) && userEntitlement.TryGetValue(beatmap, out var entitlement) && entitlement != EntitlementStatus.NotOwned);

        // Gets a list of playerIds that have registered 'NotOwned' entitlement
        public List<string> GetPlayersWithoutBeatmap(string beatmap)
            => userEntitlements.Where(userEntitlement => userEntitlement.Value.ContainsKey(beatmap) && userEntitlement.Value[beatmap] == EntitlementStatus.NotOwned).Select(userEntitlement => userEntitlement.Key).ToList();

        // Handles an entitlement
        public void HandleEntitlement(string userId, string beatmap, EntitlementStatus entitlement)
            => userEntitlements.GetOrAdd(userId, _ => new ConcurrentDictionary<string, EntitlementStatus>())[beatmap] = entitlement;
    }
}
