using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.DedicatedServer.Interface.Requests
{
    public record KickPlayerRequest(string Secret, string UserId);
}
