using Autobus;
using BeatTogether.Core.Abstractions;
using BeatTogether.Core.ServerMessaging;
using BeatTogether.DedicatedServer.Interface.Events;
using System;
using System.Linq;

namespace BeatTogether.DedicatedServer.Node
{
    public class ForwardServerEventsLayer : ILayer1
    {

        private readonly IAutobus _autobus;
        //private readonly ILogger _logger = Log.ForContext<ForwardServerEventsLayer>();

        public ForwardServerEventsLayer(
            IAutobus autobus)
        {
            _autobus = autobus;
        }

        public void InstanceClosed(IServerInstance instance)
        {
            _autobus.Publish(new MatchmakingServerStoppedEvent(instance.Secret));
        }

        public void InstanceConfigChanged(IServerInstance instance)
        {
            _autobus.Publish(new UpdateInstanceConfigEvent(new Core.ServerMessaging.Models.Server(instance)));
        }

        public void InstanceCreated(IServerInstance instance)
        {
            throw new NotImplementedException();
        }

        public void InstancePlayersChanged(IServerInstance instance)
        {
            _autobus.Publish(new UpdatePlayersEvent(instance.Secret, instance.PlayerHashes.ToArray()));
        }

        public void InstanceStateChanged(IServerInstance instance)
        {
            _autobus.Publish(new ServerInGameplayEvent(instance.Secret, instance.GameState));
        }

        public void PlayerJoined(IServerInstance instance, IPlayer player)
        {
            _autobus.Publish(new PlayerJoinEvent(instance.Secret, player.HashedUserId));
        }

        public void PlayerLeft(IServerInstance instance, IPlayer player)
        {
            _autobus.Publish(new PlayerLeaveServerEvent(instance.Secret, player.HashedUserId));
        }
    }
}
