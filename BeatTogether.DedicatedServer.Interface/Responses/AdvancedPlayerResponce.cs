using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.DedicatedServer.Interface.Responses
{
    public record AdvancedPlayerResponce(AdvancedPlayer? AdvancedPlayer)
    {
        public bool Success => AdvancedPlayer != null;
    }
}
