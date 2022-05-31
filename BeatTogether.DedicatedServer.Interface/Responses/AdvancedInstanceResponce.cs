using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.DedicatedServer.Interface.Responses
{
    public record AdvancedInstanceResponce(AdvancedInstance? _AdvancedInstance, Beatmap? Beatmap)
    {
        public bool success = _AdvancedInstance != null;
    }
}
