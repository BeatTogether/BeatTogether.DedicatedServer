using BeatSaverSharp;
using BeatSaverSharp.Models;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Node.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Node
{
    public class BeatmapDifficultyStats
    {
        public bool Chroma { get; }
        public bool MappingExtensions { get; }
        public bool NoodleExtensions { get; }
        //public int Plays { get; set; }
        public BeatmapDifficultyStats(bool chroma, bool mappingExtensions, bool noodleExtensions)
        {
            Chroma = chroma;
            MappingExtensions = mappingExtensions;
            NoodleExtensions = noodleExtensions;
            //Plays = 0;
        }
    }
    public class BeatmapData
    {
        public float BeatmapLength { get; }
        public ConcurrentDictionary<BeatmapIdentifier, BeatmapDifficultyStats> Difficulties;

        public BeatmapData(float beatmapLength, ConcurrentDictionary<BeatmapIdentifier, BeatmapDifficultyStats> difficulties)
        {
            BeatmapLength = beatmapLength;
            Difficulties = difficulties;
        }
    }

    public class BeatmapRepository : IBeatmapRepository
    {
        private BeatSaver beatSaverAPI = new("BeatTogetherDedicatedInstance", new Version("1.0.0"));
        private ConcurrentDictionary<string, BeatmapData> _BeatmapRepository = new();

        public async Task<bool> CheckBeatmap(BeatmapIdentifier beatmap, bool AllowChroma, bool AllowMappingExtensions, bool AllowNoodleExtensions)
        {
            if(_BeatmapRepository.TryGetValue(beatmap.LevelId, out var beatmapData))
                return CheckDifficulties(beatmap, beatmapData, AllowChroma, AllowMappingExtensions, AllowNoodleExtensions);
            if(await FetchBeatmap(beatmap))
                return await CheckBeatmap(beatmap, AllowChroma, AllowMappingExtensions, AllowNoodleExtensions);
            return false;
        }

        private bool CheckDifficulties(BeatmapIdentifier beatmap, BeatmapData beatmapData, bool AllowChroma, bool AllowMappingExtensions, bool AllowNoodleExtensions)
        {
            if(beatmapData.Difficulties.TryGetValue(beatmap, out var beatmapDifficulty))
                return !(beatmapDifficulty.Chroma && !AllowChroma) && !(beatmapDifficulty.MappingExtensions && !AllowMappingExtensions) && !(beatmapDifficulty.NoodleExtensions && !AllowNoodleExtensions);
            return false;
        }

        public float GetBeatmapLength(BeatmapIdentifier beatmap)
        {
            if (_BeatmapRepository.TryGetValue(beatmap.LevelId, out var beatmapData))
                return beatmapData.BeatmapLength;
            return -1;
        }

        public async Task<bool> FetchBeatmap(BeatmapIdentifier beatmap)
        {
            Beatmap? FetchBeatmap = await beatSaverAPI.Beatmap(beatmap!.LevelId);
            if (FetchBeatmap != null)
            {
                ConcurrentDictionary<BeatmapIdentifier, BeatmapDifficultyStats> difficulties = new();
                for (int i = 0; i < FetchBeatmap.LatestVersion.Difficulties.Count; i++)
                {
                    BeatmapIdentifier identifier = new();
                    identifier.Difficulty = (Messaging.Models.BeatmapDifficulty)FetchBeatmap.LatestVersion.Difficulties[i].Difficulty;
                    identifier.Characteristic = FetchBeatmap.LatestVersion.Difficulties[i].Characteristic.ToString();
                    identifier.LevelId = beatmap.LevelId;
                    BeatmapDifficultyStats difficultyStats = new BeatmapDifficultyStats(FetchBeatmap.LatestVersion.Difficulties[i].Chroma,
                        FetchBeatmap.LatestVersion.Difficulties[i].MappingExtensions,
                        FetchBeatmap.LatestVersion.Difficulties[i].NoodleExtensions
                        );
                    difficulties.TryAdd(identifier, difficultyStats);

                }
                BeatmapData data = new BeatmapData((float)FetchBeatmap.Metadata.Duration, difficulties);
                return _BeatmapRepository.TryAdd(beatmap.LevelId, data);
            }
            return false;
        }
    }
}
