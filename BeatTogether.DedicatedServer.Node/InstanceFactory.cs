using BeatTogether.DedicatedServer.Interface.Models;
using BeatTogether.DedicatedServer.Kernel;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Node.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BeatTogether.DedicatedServer.Node
{
    public sealed class InstanceFactory : IInstanceFactory
    {
        private readonly IInstanceRegistry _instanceRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPortAllocator _portAllocator;

        public InstanceFactory(
            IInstanceRegistry instanceRegistry,
            IServiceProvider serviceProvider,
            IPortAllocator portAllocator)
        {
            _instanceRegistry = instanceRegistry;
            _serviceProvider = serviceProvider;
            _portAllocator = portAllocator;
        }

        public IDedicatedInstance? CreateInstance(
            string secret,
            string managerId,
            GameplayServerConfiguration config)
        {
            var port = _portAllocator.AcquirePort();
            if (!port.HasValue)
                return null;

            var scope = _serviceProvider.CreateScope();

            var instanceConfig = scope.ServiceProvider.GetRequiredService<InstanceConfiguration>();
            instanceConfig.Port = (int)port!;
            instanceConfig.Secret = secret;
            instanceConfig.ManagerId = managerId;
            instanceConfig.MaxPlayerCount = config.MaxPlayerCount;
            instanceConfig.DiscoveryPolicy = (DiscoveryPolicy)config.DiscoveryPolicy;
            instanceConfig.InvitePolicy = (InvitePolicy)config.InvitePolicy;
            instanceConfig.GameplayServerMode = (GameplayServerMode)config.GameplayServerMode;
            instanceConfig.SongSelectionMode = (SongSelectionMode)config.SongSelectionMode;
            instanceConfig.GameplayServerControlSettings = (GameplayServerControlSettings)config.GameplayServerControlSettings;

            var instance = scope.ServiceProvider.GetRequiredService<IDedicatedInstance>();
            if (!_instanceRegistry.AddInstance(instance))
                return null;
            return instance;
        }
    }
}
