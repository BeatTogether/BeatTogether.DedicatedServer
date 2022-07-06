using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Node.Abstractions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BeatTogether.DedicatedServer.Node
{
    public sealed class InstanceRegistry : IInstanceRegistry
    {
        private readonly ConcurrentDictionary<string, IDedicatedInstance> _instances = new();

        public bool AddInstance(IDedicatedInstance instance) =>
            _instances.TryAdd(instance._configuration.Secret, instance);

        public bool RemoveInstance(IDedicatedInstance instance) =>
            _instances.TryRemove(instance._configuration.Secret, out _);

        public IDedicatedInstance GetInstance(string secret) =>
            _instances[secret];

        public bool TryGetInstance(string secret, [MaybeNullWhen(false)] out IDedicatedInstance instance) =>
            _instances.TryGetValue(secret, out instance);
        
        public bool DoesInstanceExist(string secret) => _instances.ContainsKey(secret);

        public string[] ListPublicInstanceSecrets()
        {
            List<string> instances = new();
            foreach (var item in _instances)
                if (item.Value._configuration.DiscoveryPolicy == Kernel.Enums.DiscoveryPolicy.Public)
                    instances.Add(item.Key);
            return instances.ToArray();
        }

        public int GetServerCount() { return _instances.Count; }

        public int GetPublicServerCount()
        {
            int count = 0;
            foreach (var item in _instances)
                if (item.Value._configuration.DiscoveryPolicy == Kernel.Enums.DiscoveryPolicy.Public)
                    count++;
            return count;
        }
    }
}
