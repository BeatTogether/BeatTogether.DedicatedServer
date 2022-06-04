using BeatTogether.DedicatedServer.Messaging.Models;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IBeatmapRepository
    {
        Task<bool> CheckBeatmap(BeatmapIdentifier beatmap, bool AllowChroma, bool AllowMappingExtensions, bool AllowNoodleExtensions);
        int GetBeatmapLength(BeatmapIdentifier beatmap);
        Task<bool> FetchBeatmap(BeatmapIdentifier beatmap);
    }
}
