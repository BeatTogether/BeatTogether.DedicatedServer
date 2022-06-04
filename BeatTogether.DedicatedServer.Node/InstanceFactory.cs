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
            GameplayServerConfiguration config,
            bool permanentManager = false,//If a user links there account to discord and uses a bot to make a lobby, then can enter there userId
            float instanceTimeout = 0f,
            string ServerName = "")
        {
            var port = _portAllocator.AcquirePort();
            if (!port.HasValue)
                return null;

            var scope = _serviceProvider.CreateScope();

            var instanceConfig = scope.ServiceProvider.GetRequiredService<InstanceConfiguration>();
            instanceConfig.Port = (int)port!;
            instanceConfig.Secret = secret;
            instanceConfig.ManagerId = managerId;
            instanceConfig.MaxPlayerCount = Math.Min(config.MaxPlayerCount,127); //max size of 127
            instanceConfig.DiscoveryPolicy = (DiscoveryPolicy)config.DiscoveryPolicy;
            instanceConfig.InvitePolicy = (InvitePolicy)config.InvitePolicy;
            instanceConfig.GameplayServerMode = (GameplayServerMode)config.GameplayServerMode;
            instanceConfig.SongSelectionMode = (SongSelectionMode)config.SongSelectionMode;
            instanceConfig.GameplayServerControlSettings = (GameplayServerControlSettings)config.GameplayServerControlSettings;

            var instance = scope.ServiceProvider.GetRequiredService<IDedicatedInstance>();
            if (!_instanceRegistry.AddInstance(instance))
                return null;
            instance.StopEvent += () => _instanceRegistry.RemoveInstance(instance);
            if(permanentManager)
                instance.SetupPermanentManager(managerId);
            instance.SetupInstance(instanceTimeout, ServerName);
            return instance;
        }
    }
}
