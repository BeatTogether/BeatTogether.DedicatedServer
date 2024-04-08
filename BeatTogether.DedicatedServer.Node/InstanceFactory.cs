﻿using System;
using BeatTogether.DedicatedServer.Interface.Models;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Node.Abstractions;
using Microsoft.Extensions.DependencyInjection;

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
            bool permanentManager,
            float instanceTimeout,
            string ServerName,
            long resultScreenTime,
            long BeatmapStartTime,
            long PlayersReadyCountdownTime,
            bool AllowPerPlayerModifiers,
            bool AllowPerPlayerDifficulties,
            bool AllowChroma,
            bool AllowME,
            bool AllowNE
            )
        {
            var Port = _portAllocator.AcquirePort();
            
            if (!Port.HasValue)
                return null;

            var scope = _serviceProvider.CreateScope();

            var instanceConfig = scope.ServiceProvider.GetRequiredService<InstanceConfiguration>();
            instanceConfig.Port = (int)Port!;
            instanceConfig.Secret = secret;
            instanceConfig.ServerOwnerId = managerId;
            instanceConfig.MaxPlayerCount = Math.Min(config.MaxPlayerCount,250); //max size of 254, id 127 routes packets to all, max is 250, last 4 ID's will be reserved for future features
            instanceConfig.DiscoveryPolicy = (DiscoveryPolicy)config.DiscoveryPolicy;
            instanceConfig.InvitePolicy = (InvitePolicy)config.InvitePolicy;
            instanceConfig.GameplayServerMode = (GameplayServerMode)config.GameplayServerMode;
            instanceConfig.SongSelectionMode = (SongSelectionMode)config.SongSelectionMode;
            instanceConfig.GameplayServerControlSettings = (GameplayServerControlSettings)config.GameplayServerControlSettings;
            instanceConfig.DestroyInstanceTimeout = instanceTimeout;
            instanceConfig.ServerName = ServerName;
            instanceConfig.CountdownConfig.BeatMapStartCountdownTime = Math.Max(BeatmapStartTime,0);
            instanceConfig.CountdownConfig.ResultsScreenTime = Math.Max(resultScreenTime,0);
            instanceConfig.AllowChroma = AllowChroma;
            instanceConfig.AllowMappingExtensions = AllowME;
            instanceConfig.AllowNoodleExtensions = AllowNE;
            instanceConfig.AllowPerPlayerDifficulties = AllowPerPlayerDifficulties;
            instanceConfig.AllowPerPlayerModifiers = AllowPerPlayerModifiers;
            if (permanentManager)
                instanceConfig.SetConstantManagerFromUserId = managerId;
            instanceConfig.CountdownConfig.CountdownTimePlayersReady = Math.Max(PlayersReadyCountdownTime,0L);
            if (instanceConfig.CountdownConfig.CountdownTimePlayersReady == 0L)
                instanceConfig.CountdownConfig.CountdownTimePlayersReady = instanceConfig.GameplayServerMode == GameplayServerMode.Managed ? 15000L : 30000L;
            var instance = scope.ServiceProvider.GetRequiredService<IDedicatedInstance>();
            if (!_instanceRegistry.AddInstance(instance))
                return null;
            instance.StopEvent += HandleStopEvent;
            return instance;
        }

        private void HandleStopEvent(IDedicatedInstance Instance)
        {
            _instanceRegistry.RemoveInstance(Instance);
            _portAllocator.ReleasePort(Instance.Port);
        }
    }
}
