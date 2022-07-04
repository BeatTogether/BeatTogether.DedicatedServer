using BeatTogether.DedicatedServer.Messaging.Models;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IBeatmapRepository
    {
        bool AllowChroma { get; }
        bool AllowMappingExtensions { get; }
        bool AllowNoodleExtensions { get; }
        Task<bool> CheckBeatmap(BeatmapIdentifier beatmap, bool AllowNonBeatSaver);
        Task<bool> FetchBeatmap(BeatmapIdentifier beatmap);
        void CleanCachedBeatmapsByActivity();
        void ClearCachedBeatmaps();
        void SetRequirements(bool chroma, bool MappingExtensions, bool NoodleExtensions);
        bool IsPrefferedDifficultyValid(BeatmapIdentifier beatmap, BeatmapDifficulty? difficulty);
    }
}
