using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.DedicatedServer.Interface.Requests
{
    public record SetInstanceBeatmapRequest(string Secret, Beatmap beatmap, GameplayModifiers modifiers, float countdown, CountdownState countdownState);
}
