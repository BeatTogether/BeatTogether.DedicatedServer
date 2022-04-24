using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Node.Abstractions;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace BeatTogether.DedicatedServer.Node
{
    public sealed class InstanceRegistry : IInstanceRegistry
    {
        private readonly ConcurrentDictionary<string, IDedicatedInstance> _instances = new();

        public bool AddInstance(IDedicatedInstance instance) =>
            _instances.TryAdd(instance.Configuration.Secret, instance);

        public bool RemoveInstance(IDedicatedInstance instance) =>
            _instances.TryRemove(instance.Configuration.Secret, out _);

        public IDedicatedInstance GetInstance(string secret) =>
            _instances[secret];

        public bool TryGetInstance(string secret, [MaybeNullWhen(false)] out IDedicatedInstance instance) =>
            _instances.TryGetValue(secret, out instance);
    }
}
