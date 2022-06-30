using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Interface.Requests;
using BeatTogether.DedicatedServer.Interface.Responses;
using Autobus;
using BeatTogether.DedicatedServer.Interface.Events;

namespace BeatTogether.DedicatedServer.Interface
{
    public interface IMatchmakingService
    {
        Task<CreateMatchmakingServerResponse> CreateMatchmakingServer(CreateMatchmakingServerRequest request);
        //TODO convert all of these into event handlers for api use
        /*
        Task<StopMatchmakingServerResponse> StopMatchmakingServer(StopMatchmakingServerRequest request);
        Task<SimplePlayersListResponce>? GetSimplePlayerList(GetPlayersSimpleRequest request);
        Task<AdvancedPlayersListResponce>? GetAdvancedPlayerList(GetPlayersAdvancedRequest request);
        Task<AdvancedPlayerResponce> GetAdvancedPlayer(GetPlayerAdvancedRequest request);
        Task<KickPlayerResponse> KickPlayer(KickPlayerRequest request);
        Task<AdvancedInstanceResponce> GetAdvancedInstance(GetAdvancedInstanceRequest request);
        Task<SetInstanceBeatmapResponse> SetInstanceBeatmap(SetInstanceBeatmapRequest request);
        Task<SetAllowedRequirementsResponse> SetAllowedRequirements(SetAllowedRequirementsRequest request);
        Task<GetAllowedRequirementsResponse> GetAllowedRequirements(GetAllowedRequirementsRequest request);
        Task<ClearCachedBeatmapsResponse> ClearCachedBeatmaps(ClearCachedBeatmapsRequest request);
        Task<DoesServerExistResponse> DoesServerExist(DoesServerExistRequest request);
        */

        public class DedicatedServerServiceContract : BaseServiceContract
        {
            public override void Build(IServiceContractBuilder builder) =>
                builder
                    .UseName("DedicatedServer")
                    .AddInterface<IMatchmakingService>()
                    .AddEvent<PlayerLeaveServerEvent>()
                    .AddEvent<NodeStartedEvent>()
                    .AddEvent<NodeOnlineEvent>()
                    .AddEvent<NodeReceivedPlayerEncryptionEvent>()
                    .AddEvent<MatchmakingServerStoppedEvent>();
        }
    }
}
