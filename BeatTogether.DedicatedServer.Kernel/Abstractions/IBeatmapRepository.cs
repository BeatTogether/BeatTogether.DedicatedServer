using BeatTogether.DedicatedServer.Messaging.Models;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IBeatmapRepository
    {
        Task<bool> CheckBeatmap(BeatmapIdentifier beatmap, bool AllowNonBeatSaver, bool AllowChroma, bool AllowME, bool AllowNE);
        Task<bool> FetchBeatmap(BeatmapIdentifier beatmap);
        void CleanCachedBeatmapsByActivity();
        void ClearCachedBeatmaps();
        bool IsPrefferedDifficultyValid(BeatmapIdentifier beatmap, BeatmapDifficulty? difficulty);
    }
}
