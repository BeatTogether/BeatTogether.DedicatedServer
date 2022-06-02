using BeatTogether.DedicatedServer.Messaging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Node.Abstractions
{
    public interface IBeatmapRepository
    {
        Task<bool> CheckBeatmap(BeatmapIdentifier beatmap, bool AllowChroma, bool AllowMappingExtensions, bool AllowNoodleExtensions);
        float GetBeatmapLength(BeatmapIdentifier beatmap);
        Task<bool> FetchBeatmap(BeatmapIdentifier beatmap);
    }
}
