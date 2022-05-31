using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Interface.Requests;
using BeatTogether.DedicatedServer.Interface.Responses;

namespace BeatTogether.DedicatedServer.Interface
{
    public interface IMatchmakingService
    {
        Task<CreateMatchmakingServerResponse> CreateMatchmakingServer(CreateMatchmakingServerRequest request);
        Task<StopMatchmakingServerResponse> StopMatchmakingServer(StopMatchmakingServerRequest request);
        Task<PublicMatchmakingServerListResponse> GetPublicMatchmakingServerList(GetPublicMatchmakingServerListRequest request);
        Task<PublicServerCountResponse> GetPublicServerCount(GetPublicMatchmakingServerCountRequest request);
        Task<ServerCountResponse> GetServerCount(GetMatchmakingServerCountRequest request);
        Task<SimplePlayersListResponce>? GetSimplePlayerList(GetPlayersSimpleRequest request);
        Task<AdvancedPlayersListResponce>? GetAdvancedPlayerList(GetPlayersAdvancedRequest request);
        Task<AdvancedPlayerResponce> GetAdvancedPlayer(GetPlayerAdvancedRequest request);
        Task<KickPlayerResponse> KickPlayer(KickPlayerRequest request);
        Task<AdvancedInstanceResponce> GetAdvancedInstance(GetAdvancedInstanceRequest request);
        Task<SetInstanceBeatmapResponse> SetInstanceBeatmap(SetInstanceBeatmapRequest request);
        //TODO add tasks to set beatmaps, modifiers, countdowns though autobus
    }
}
