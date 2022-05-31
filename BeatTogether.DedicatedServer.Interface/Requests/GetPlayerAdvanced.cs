using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.DedicatedServer.Interface.Requests
{
    public record GetPlayerAdvanced(string Secret, string UserId);
}
