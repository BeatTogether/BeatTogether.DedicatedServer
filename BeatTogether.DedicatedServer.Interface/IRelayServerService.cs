using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Messaging.Requests;
using BeatTogether.DedicatedServer.Messaging.Responses;

namespace BeatTogether.DedicatedServer.Interface
{
    public interface IRelayServerService
    {
        Task<GetAvailableRelayServerResponse> GetAvailableRelayServer(GetAvailableRelayServerRequest request);
    }
}
