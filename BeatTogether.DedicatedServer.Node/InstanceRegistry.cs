using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Node.Abstractions;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WinFormsLibrary;

namespace BeatTogether.DedicatedServer.Node
{
    public sealed class InstanceRegistry : IInstanceRegistry
    {
        public readonly ConcurrentDictionary<string, IDedicatedInstance> _instances = new();

        public bool AddInstance(IDedicatedInstance instance) {
            bool a = _instances.TryAdd(instance.Configuration.Secret, instance);
            Task.Factory.StartNew(() => {
                Messenger.Default.Send<Boolean>(true);
            });
            return a;
        }

        public bool RemoveInstance(IDedicatedInstance instance)
        {
            bool a = _instances.TryRemove(instance.Configuration.Secret, out _);
            Task.Factory.StartNew(() => {
                Messenger.Default.Send<Boolean>(true);
            });
            return a;
        }

        public IDedicatedInstance GetInstance(string secret) =>
            _instances[secret];

        public bool TryGetInstance(string secret, [MaybeNullWhen(false)] out IDedicatedInstance instance) =>
            _instances.TryGetValue(secret, out instance);
    }
}
