using BeatTogether.DedicatedServer.Interface.Models;
using System;

namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record SelectedBeatmapEvent(
        string Secret,
        string LevelId,
        string Characteristic,
        uint Difficulty,
        bool Gameplay,
        GameplayModifiers GameplayModifiers,
        DateTime CountdownEnd //If in gameplay then this is when the beatmap started
        );
}
