﻿using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Interface.Requests;
using BeatTogether.DedicatedServer.Interface.Responses;
using Autobus;
using BeatTogether.DedicatedServer.Interface.Events;

namespace BeatTogether.DedicatedServer.Interface
{
    public interface IMatchmakingService
    {
        Task<CreateMatchmakingServerResponse> CreateMatchmakingServer(CreateMatchmakingServerRequest request);

        public class DedicatedServerServiceContract : BaseServiceContract
        {
            public override void Build(IServiceContractBuilder builder) =>
                builder
                    .UseName("DedicatedServer")
                    .AddInterface<IMatchmakingService>()
                    .AddEvent<PlayerLeaveServerEvent>()
                    .AddEvent<PlayerJoinEvent>()
                    .AddEvent<UpdatePlayersEvent>()
                    .AddEvent<NodeStartedEvent>()
                    .AddEvent<NodeOnlineEvent>()
                    .AddEvent<NodeReceivedPlayerSessionDataEvent>()
                    .AddEvent<MatchmakingServerStoppedEvent>()
                    .AddEvent<UpdateInstanceConfigEvent>() 
                    .AddEvent<ServerInGameplayEvent>();
        }
    }
}
