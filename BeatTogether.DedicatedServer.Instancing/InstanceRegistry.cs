using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Instancing.Abstractions;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using BeatTogether.Core.Enums;
using System.Linq;

namespace BeatTogether.DedicatedServer.Instancing
{
    public sealed class InstanceRegistry : IInstanceRegistry
    {
        private readonly ConcurrentDictionary<string, IDedicatedInstance> _instances = new();
        private readonly ConcurrentDictionary<string, IDedicatedInstance> _instancesByCode = new();

        public bool AddInstance(IDedicatedInstance instance){
            if (_instances.TryAdd(instance._configuration.Secret, instance)) {
                if(_instancesByCode.TryAdd(instance._configuration.Code, instance))
                    return true;
                _instances.TryRemove(instance._configuration.Secret, out _);
            }
            return false;
        }

        public bool RemoveInstance(IDedicatedInstance instance) => _instances.TryRemove(instance._configuration.Secret, out _) && _instancesByCode.TryRemove(instance._configuration.Code, out _);

        public bool TryGetAvailablePublicServer(InvitePolicy invitePolicy, GameplayServerMode serverMode, SongSelectionMode songMode, GameplayServerControlSettings serverControlSettings, BeatmapDifficultyMask difficultyMask, GameplayModifiersMask modifiersMask, string songPackMasks, [MaybeNullWhen(false)] out IDedicatedInstance instance)
        {
            instance = null;
            var AvaliableServers = _instances.Values.Where(s =>
                s._configuration.GameplayServerConfiguration.InvitePolicy == invitePolicy &&
                s._configuration.GameplayServerConfiguration.GameplayServerMode == serverMode &&
                s._configuration.GameplayServerConfiguration.SongSelectionMode == songMode &&
                s._configuration.GameplayServerConfiguration.GameplayServerControlSettings == serverControlSettings &&
                s._configuration.BeatmapDifficultyMask == difficultyMask &&
                s._configuration.GameplayModifiersMask == modifiersMask &&
                s._configuration.SongPacksMask == songPackMasks
                );
            if (!AvaliableServers.Any())
                return false;
            var server = AvaliableServers.First();
            foreach (var publicServer in AvaliableServers)
            {
                if ((publicServer.GetPlayerRegistry().GetPlayerCount() < publicServer._configuration.GameplayServerConfiguration.MaxPlayerCount && publicServer.GetPlayerRegistry().GetPlayerCount() > server.GetPlayerRegistry().GetPlayerCount()))
                {
                    server = publicServer;
                }
            }
            if (server.GetPlayerRegistry().GetPlayerCount() >= server._configuration.GameplayServerConfiguration.MaxPlayerCount)
                return false;
            instance = server;
            return true;
        }

        public bool TryGetInstance(string secret, [MaybeNullWhen(false)] out IDedicatedInstance instance) =>
            _instances.TryGetValue(secret, out instance);

        public bool TryGetInstanceByCode(string code, [MaybeNullWhen(false)] out IDedicatedInstance instance) =>
            _instancesByCode.TryGetValue(code, out instance);
    }
}
