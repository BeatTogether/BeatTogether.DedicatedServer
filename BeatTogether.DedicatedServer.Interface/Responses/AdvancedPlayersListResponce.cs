using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.DedicatedServer.Interface.Responses
{
    public record AdvancedPlayersListResponce(AdvancedPlayer[]? AdvancedPlayers)
    {
        public bool Success => AdvancedPlayers != null;
    }
}
