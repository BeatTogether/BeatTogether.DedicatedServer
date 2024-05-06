using System;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.Core.Enums;
using BeatTogether.DedicatedServer.Instancing.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using BeatTogether.Core.Abstractions;
using System.Net;
using BeatTogether.DedicatedServer.Instancing.Configuration;
using BeatTogether.Core.ServerMessaging;
using BeatTogether.DedicatedServer.Instancing.Implimentations;
using Serilog;

namespace BeatTogether.DedicatedServer.Instancing
{
    public sealed class InstanceFactory : IInstanceFactory
    {
        private readonly IInstanceRegistry _instanceRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPortAllocator _portAllocator;
        private readonly InstancingConfiguration _config;
        private readonly ILayer1? _SendEventsLayer;
        private readonly ILogger _logger = Log.ForContext<InstanceFactory>();

        public InstanceFactory(
            IInstanceRegistry instanceRegistry,
            IServiceProvider serviceProvider,
            IPortAllocator portAllocator,
            InstancingConfiguration instancingConfiguration)
        {
            _instanceRegistry = instanceRegistry;
            _serviceProvider = serviceProvider;
            _portAllocator = portAllocator;
            _config = instancingConfiguration;

            _SendEventsLayer = _serviceProvider.GetService<ILayer1>();
        }

        public IDedicatedInstance? CreateInstance(IServerInstance serverInstance)
        {
            var Port = _portAllocator.AcquirePort();

            if (!Port.HasValue)
                return null;

            var scope = _serviceProvider.CreateScope();

            var instanceConfig = scope.ServiceProvider.GetRequiredService<InstanceConfiguration>();
            instanceConfig.Port = (int)Port!;
            instanceConfig.Secret = serverInstance.Secret;
            instanceConfig.Code = serverInstance.Code;
            instanceConfig.ServerId = serverInstance.InstanceId;
            _logger.Information("Server ID: " + instanceConfig.ServerId);
            instanceConfig.ServerOwnerId = serverInstance.ManagerId;
            _logger.Information("Server owner ID: " + instanceConfig.ServerOwnerId);
            instanceConfig.GameplayServerConfiguration = serverInstance.GameplayServerConfiguration;
            instanceConfig.GameplayModifiersMask = serverInstance.GameplayModifiersMask;
            instanceConfig.BeatmapDifficultyMask = serverInstance.BeatmapDifficultyMask;
            instanceConfig.SongPacksMask = serverInstance.SongPackMasks;
            instanceConfig.DestroyInstanceTimeout = serverInstance.ServerStartJoinTimeout;
            instanceConfig.ServerName = serverInstance.ServerName;
            instanceConfig.CountdownConfig.BeatMapStartCountdownTime = Math.Max(serverInstance.BeatmapStartTime, 0L);
            instanceConfig.CountdownConfig.ResultsScreenTime = Math.Max(serverInstance.ResultScreenTime, 0L); //TODO convert the dedi logic to use long instead of float
            instanceConfig.AllowChroma = serverInstance.AllowChroma;
            instanceConfig.AllowMappingExtensions = serverInstance.AllowME;
            instanceConfig.AllowNoodleExtensions = serverInstance.AllowNE;
            instanceConfig.AllowPerPlayerDifficulties = serverInstance.AllowPerPlayerDifficulties;
            instanceConfig.AllowPerPlayerModifiers = serverInstance.AllowPerPlayerModifiers;
            if (serverInstance.PermanentManager)
                instanceConfig.SetConstantManagerFromUserId = serverInstance.ManagerId;
            instanceConfig.CountdownConfig.CountdownTimePlayersReady = Math.Max(serverInstance.PlayersReadyCountdownTime, 0L);
            if (instanceConfig.CountdownConfig.CountdownTimePlayersReady == 0L)
                instanceConfig.CountdownConfig.CountdownTimePlayersReady = instanceConfig.GameplayServerConfiguration.GameplayServerMode == GameplayServerMode.Managed ? 15000L : 30000L;
            var instance = scope.ServiceProvider.GetRequiredService<IDedicatedInstance>();
            if (!_instanceRegistry.AddInstance(instance))
            {
                return null;

            }
            instance.StopEvent += HandleStopEvent;

            serverInstance.InstanceEndPoint = IPEndPoint.Parse($"{_config.HostName}:{instanceConfig.Port}");

            //Subscribe to server events if the layer above allows this.
            if(_SendEventsLayer != null)
            {
                instance.StopEvent += (dedi) => _SendEventsLayer.InstanceClosed(new ServerInstance(instance, serverInstance.InstanceEndPoint));
                instance.PlayerConnectedEvent += (player) => _SendEventsLayer.PlayerJoined(new ServerInstance(instance, serverInstance.InstanceEndPoint), player);
                instance.PlayerDisconnectedEvent += (player) => _SendEventsLayer.PlayerLeft(new ServerInstance(instance, serverInstance.InstanceEndPoint), player);
                instance.PlayerDisconnectBeforeJoining += (a, b, c) => _SendEventsLayer.InstancePlayersChanged(new ServerInstance(instance, serverInstance.InstanceEndPoint));
                instance.GameIsInLobby += (a, b) => _SendEventsLayer.InstanceStateChanged(new ServerInstance(instance, serverInstance.InstanceEndPoint));
                instance.UpdateInstanceEvent += (dedi) => _SendEventsLayer.InstanceConfigChanged(new ServerInstance(instance, serverInstance.InstanceEndPoint));
            }

            return instance;
        }

        private void HandleStopEvent(IDedicatedInstance Instance)
        {
            _instanceRegistry.RemoveInstance(Instance);
            _portAllocator.ReleasePort(Instance.Port);
        }
    }
}
