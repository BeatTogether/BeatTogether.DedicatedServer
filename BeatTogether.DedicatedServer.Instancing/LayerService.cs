using BeatTogether.Core.Abstractions;
using BeatTogether.Core.Enums;
using BeatTogether.DedicatedServer.Instancing.Abstractions;
using BeatTogether.DedicatedServer.Instancing.Configuration;
using BeatTogether.DedicatedServer.Instancing.Implimentations;
using Serilog;
using System.Net;
using System.Threading.Tasks;
using BeatTogether.Core.Models;

namespace BeatTogether.DedicatedServer.Instancing
{
    public class LayerService : ILayer2
    {
        private readonly IInstanceRegistry _instanceRegistry;
        private readonly IInstanceFactory _instanceFactory;
        private readonly InstancingConfiguration _instancingConfiguration;
        private readonly ILogger _logger = Log.ForContext<LayerService>();


        public LayerService(IInstanceRegistry instanceRegistry, IInstanceFactory instanceFactory, InstancingConfiguration instancingConfiguration)
        {
            _instanceRegistry = instanceRegistry;
            _instancingConfiguration = instancingConfiguration;
            _instanceFactory = instanceFactory;
        }

        public Task CloseInstance(string InstanceSecret)
        {
            if(_instanceRegistry.TryGetInstance(InstanceSecret, out var Instance)){
                Instance.Stop();
            }
            return Task.CompletedTask;
        }

        public async Task<bool> CreateInstance(IServerInstance serverInstance)
        {
            var inst = _instanceFactory.CreateInstance(serverInstance);
            if(inst != null)
                await inst.Start();
            return inst != null;
        }

        public Task DisconnectPlayer(string InstanceSecret, string PlayerUserId)
        {
            if (!_instanceRegistry.TryGetInstance(InstanceSecret, out var instance))
                return Task.CompletedTask;
            if (instance.GetPlayerRegistry().TryGetPlayer(PlayerUserId, out var player))
                instance.DisconnectPlayer(player);

            return Task.CompletedTask;
        }

        public Task<IServerInstance?> GetAvailablePublicServer(InvitePolicy invitePolicy, GameplayServerMode serverMode, SongSelectionMode songMode, GameplayServerControlSettings serverControlSettings, BeatmapDifficultyMask difficultyMask, GameplayModifiersMask modifiersMask, string songPackMasks, VersionRange versionRange)
        {
            IServerInstance? serverInstance = null;
            if (_instanceRegistry.TryGetAvailablePublicServer(invitePolicy, serverMode, songMode, serverControlSettings, difficultyMask, modifiersMask, songPackMasks, versionRange, out var instance))
            {
                serverInstance = new ServerInstance(instance, IPEndPoint.Parse($"{_instancingConfiguration.HostEndpoint}:{instance._configuration.Port}"));
            }
            return Task.FromResult(serverInstance);
        }

        public Task<IServerInstance?> GetServer(string secret)
        {
            IServerInstance? serverInstance = null;
            if (_instanceRegistry.TryGetInstance(secret, out var instance))
            {
                serverInstance = new ServerInstance(instance, IPEndPoint.Parse($"{_instancingConfiguration.HostEndpoint}:{instance._configuration.Port}"));
            }
            return Task.FromResult(serverInstance);
        }

        public Task<IServerInstance?> GetServerByCode(string code)
        {
            IServerInstance? serverInstance = null;
            if (_instanceRegistry.TryGetInstanceByCode(code, out var instance))
            {
                serverInstance = new ServerInstance(instance, IPEndPoint.Parse($"{_instancingConfiguration.HostEndpoint}:{instance._configuration.Port}"));
            }
            return Task.FromResult(serverInstance);
        }

        public Task<bool> SetPlayerSessionData(string serverSecret, IPlayer playerSessionData)
        {
            _logger.Information("Setting playerSessionData: " + playerSessionData.PlayerSessionId + " In instance: " + serverSecret);
            if (!_instanceRegistry.TryGetInstance(serverSecret, out var instance))
                return Task.FromResult(false);
            _logger.Information("Found instance, setting session data");
            instance.GetPlayerRegistry().AddExtraPlayerSessionData(playerSessionData);
            return Task.FromResult(true);
        }
    }
}
