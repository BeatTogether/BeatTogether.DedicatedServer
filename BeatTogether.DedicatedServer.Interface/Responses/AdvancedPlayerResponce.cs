using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.DedicatedServer.Interface.Responses
{
    public record AdvancedPlayerResponce(AdvancedPlayer? AdvancedPlayer)
    {
        public bool success = AdvancedPlayer != null;
    }
}
