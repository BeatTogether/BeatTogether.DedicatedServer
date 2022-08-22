using BeatTogether.DedicatedServer.Interface.Models;
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
            string ServerName = "",
            float resultScreenTime = 20.0f,
            float BeatmapStartTime = 5.0f,
            float PlayersReadyCountdownTime = 0f,
            bool AllowPerPlayerModifiers = false,
            bool AllowPerPlayerDifficulties = false,
            bool AllowChroma = true,
            bool AllowME = true,
            bool AllowNE = true
            )
        {
            var port = _portAllocator.AcquirePort();
            if (!port.HasValue)
                return null;

            var scope = _serviceProvider.CreateScope();

            var instanceConfig = scope.ServiceProvider.GetRequiredService<InstanceConfiguration>();
            instanceConfig.Port = (int)port!;
            instanceConfig.Secret = secret;
            instanceConfig.ManagerId = managerId;
            instanceConfig.MaxPlayerCount = Math.Min(config.MaxPlayerCount,250); //max size of 254, id 127 routes packets to all, max is 250, last 4 ID's will be reserved for future features
            instanceConfig.DiscoveryPolicy = (DiscoveryPolicy)config.DiscoveryPolicy;
            instanceConfig.InvitePolicy = (InvitePolicy)config.InvitePolicy;
            instanceConfig.GameplayServerMode = (GameplayServerMode)config.GameplayServerMode;
            instanceConfig.SongSelectionMode = (SongSelectionMode)config.SongSelectionMode;
            instanceConfig.GameplayServerControlSettings = (GameplayServerControlSettings)config.GameplayServerControlSettings;
            instanceConfig.DestroyInstanceTimeout = instanceTimeout;
            instanceConfig.ServerName = ServerName;
            instanceConfig.CountdownConfig.BeatMapStartCountdownTime = Math.Max(BeatmapStartTime,0f);
            instanceConfig.CountdownConfig.ResultsScreenTime = Math.Max(resultScreenTime,0f);
            instanceConfig.AllowChroma = AllowChroma;
            instanceConfig.AllowMappingExtensions = AllowME;
            instanceConfig.AllowNoodleExtensions = AllowNE;
            instanceConfig.AllowPerPlayerDifficulties = AllowPerPlayerDifficulties;
            instanceConfig.AllowPerPlayerModifiers = AllowPerPlayerModifiers;
            if (permanentManager)
                instanceConfig.SetConstantManagerFromUserId = managerId;
            instanceConfig.CountdownConfig.CountdownTimePlayersReady = Math.Max(PlayersReadyCountdownTime,0f);
            if (instanceConfig.CountdownConfig.CountdownTimePlayersReady == 0f)
                instanceConfig.CountdownConfig.CountdownTimePlayersReady = instanceConfig.GameplayServerMode == GameplayServerMode.Managed ? 15.0f : 30.0f;
            var instance = scope.ServiceProvider.GetRequiredService<IDedicatedInstance>();
            if (!_instanceRegistry.AddInstance(instance))
                return null;
            instance.StopEvent += HandleStopEvent;
            return instance;
        }

        private void HandleStopEvent(IDedicatedInstance Instance)
        {
            _portAllocator.ReleasePort(Instance.Port);
            _instanceRegistry.RemoveInstance(Instance);
        }
    }
}
