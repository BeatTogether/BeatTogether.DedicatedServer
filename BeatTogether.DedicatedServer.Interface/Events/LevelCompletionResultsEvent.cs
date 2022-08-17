using BeatTogether.DedicatedServer.Interface.Models;
using System.Collections.Generic;

namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record LevelCompletionResultsEvent(
        string Secret,
        BeatmapIdentifier Beatmap,
        List<(string, BeatmapDifficulty, LevelCompletionResults)> Results);
}
