using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Interface.Requests;
using BeatTogether.DedicatedServer.Interface.Responses;

namespace BeatTogether.DedicatedServer.Interface
{
    public interface IMatchmakingService
    {
        Task<CreateMatchmakingServerResponse> CreateMatchmakingServer(CreateMatchmakingServerRequest request);
    }
}
