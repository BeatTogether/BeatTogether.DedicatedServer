using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.DedicatedServer.Interface.Responses
{
    public record SimplePlayersListResponce(SimplePlayer[]? SimplePlayers)
    {
        public bool Success => SimplePlayers != null;
    }
}
