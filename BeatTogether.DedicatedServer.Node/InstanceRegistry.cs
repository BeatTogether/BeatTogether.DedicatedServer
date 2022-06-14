using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Node.Abstractions;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Node
{
    public sealed class InstanceRegistry : IInstanceRegistry
    {
        private readonly ILogger _logger = Log.ForContext<InstanceRegistry>();

        private readonly CancellationTokenSource _stopCts = new();
        private readonly int TimeBetweenLoopStarts = 100;

        public InstanceRegistry()
        {
            Task.Run(() => UpdateLoop(_stopCts.Token));
        }

        private readonly ConcurrentDictionary<string, IDedicatedInstance> _instances = new();

        public bool AddInstance(IDedicatedInstance instance) =>
            _instances.TryAdd(instance.Configuration.Secret, instance);

        public bool RemoveInstance(IDedicatedInstance instance) =>
            _instances.TryRemove(instance.Configuration.Secret, out _);

        public IDedicatedInstance GetInstance(string secret) =>
            _instances[secret];

        public bool TryGetInstance(string secret, [MaybeNullWhen(false)] out IDedicatedInstance instance) =>
            _instances.TryGetValue(secret, out instance);

        public bool DoesInstanceExist(string secret) => _instances.ContainsKey(secret);

        public string[] ListPublicInstanceSecrets()
        {
            List<string> instances = new();
            foreach (var item in _instances)
                if (item.Value.Configuration.DiscoveryPolicy == Kernel.Enums.DiscoveryPolicy.Public)
                    instances.Add(item.Key);
            return instances.ToArray();
        }

        public int GetServerCount() { return _instances.Count; }

        public int GetPublicServerCount()
        {
            int count = 0;
            foreach (var item in _instances)
                if (item.Value.Configuration.DiscoveryPolicy == Kernel.Enums.DiscoveryPolicy.Public)
                    count++;
            return count;
        }


        private readonly Stopwatch TimeLoop = Stopwatch.StartNew();

        private async void UpdateLoop(CancellationToken cancellationToken)
        {
            TimeLoop.Restart();
            LobbyUpdateLoop(cancellationToken);
            _logger.Verbose("Time to process lobby updates: " + TimeLoop.ElapsedMilliseconds);
            int delay = Math.Max(TimeBetweenLoopStarts - (int)TimeLoop.ElapsedMilliseconds, 10);
            await Task.Delay(delay, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return;
            UpdateLoop(cancellationToken);
        }


        private void LobbyUpdateLoop(CancellationToken cancellationToken)
        {
            foreach(var instance in _instances)
            {
                try
                {
                    if (instance.Value == null || !instance.Value.IsRunning || instance.Value.GetPlayerRegistry().Players.Count <= 0)
                        continue;
                    switch (instance.Value.State)
                    {
                        case Messaging.Enums.MultiplayerGameState.Lobby:
                            var lobby = (ILobbyManager)instance.Value.GetServiceProvider().GetService(typeof(ILobbyManager))!;
                            if (lobby != null && (instance.Value.RunUpdate || lobby.CountdownEndTime < instance.Value.RunTime))
                            {
                                lobby!.Update();
                                instance.Value.RunUpdate = false;
                            }
                            continue;
                        default:
                            continue;
                    }
                }
                catch { }
            }
            return;
        }


    }
}
