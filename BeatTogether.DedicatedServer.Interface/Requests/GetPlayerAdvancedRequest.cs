using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.DedicatedServer.Interface.Requests
{
    public record GetPlayerAdvancedRequest(string Secret, string UserId);
}
