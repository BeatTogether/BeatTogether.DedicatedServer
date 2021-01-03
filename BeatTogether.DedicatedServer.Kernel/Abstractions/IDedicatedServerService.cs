using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Messaging.Requests;
using BeatTogether.DedicatedServer.Messaging.Responses;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IDedicatedServerService
    {
        Task<GetAvailableRelayServerResponse> GetAvailableRelayServer(GetAvailableRelayServerRequest request);
    }
}
