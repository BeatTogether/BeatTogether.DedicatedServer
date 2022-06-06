using BeatSaverSharp;
using BeatSaverSharp.Models;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel
{
    /**
     * Stores commonly played beatmaps so lots of beatsaver requests are not going to be made
     * Checks beatsaver to find out if a map contains requirements and if the requirement is allowed or not
     */

    public class BeatmapDifficultyStats
    {
        public bool Chroma { get; }
        public bool MappingExtensions { get; }
        public bool NoodleExtensions { get; }

        public BeatmapDifficultyStats(bool chroma, bool mappingExtensions, bool noodleExtensions)
        {
            Chroma = chroma;
            MappingExtensions = mappingExtensions;
            NoodleExtensions = noodleExtensions;
        }
    }
    public class BeatmapData
    {
        public bool Exists { get; }
        public int BeatmapLength { get; }
        public ConcurrentDictionary<string, BeatmapDifficultyStats> Difficulties;
        public int Activity { get; set; }

        public BeatmapData(int beatmapLength, ConcurrentDictionary<string, BeatmapDifficultyStats> difficulties)
        {
            BeatmapLength = beatmapLength;
            Difficulties = difficulties;
            Activity = 0;
            Exists = true;
        }
        public BeatmapData()
        {
            Exists = false;
            BeatmapLength=0;
            Activity = 0;
            Difficulties = new ConcurrentDictionary<string, BeatmapDifficultyStats>();
        }
    }

    public class BeatmapRepository : IBeatmapRepository
    {
        public bool AllowChroma { get; private set; }
        public bool AllowMappingExtensions { get; private set; }
        public bool AllowNoodleExtensions { get; private set; }

        private int CleanUpCounter = 0;
        private BeatSaver beatSaverAPI = new("BeatTogetherDedicatedInstance", new Version("1.0.0"));
        private ConcurrentDictionary<string, BeatmapData> _BeatmapRepository = new();

        public BeatmapRepository()
        {
            AllowChroma = true;
            AllowMappingExtensions = true;
            AllowNoodleExtensions = true;
        }

        public async Task<bool> CheckBeatmap(BeatmapIdentifier beatmap)
        {
            if (!beatmap.LevelId.StartsWith("custom_level_"))
                return true;//Returns true for base game levels
            if (_BeatmapRepository.TryGetValue(beatmap.LevelId, out var beatmapData))
                if (beatmapData.Exists)
                {
                    return CheckDifficulties(beatmap, beatmapData, AllowChroma, AllowMappingExtensions, AllowNoodleExtensions);
                }
                else
                    return false;
            if (await FetchBeatmap(beatmap)) //Fetches beatmap
                return await CheckBeatmap(beatmap);
            return false; //Not found beatmap or not met requirements
        }

        private bool CheckDifficulties(BeatmapIdentifier beatmap, BeatmapData beatmapData, bool AllowChroma, bool AllowMappingExtensions, bool AllowNoodleExtensions)
        {
            if (beatmapData.Difficulties.TryGetValue((beatmap.Difficulty + beatmap.Characteristic), out var beatmapDifficulty))
            {
                bool passed = !((beatmapDifficulty.Chroma && !AllowChroma) || (beatmapDifficulty.MappingExtensions && !AllowMappingExtensions) || (beatmapDifficulty.NoodleExtensions && !AllowNoodleExtensions));
                if (passed)
                {
                    CleanUpCounter++;
                    beatmapData.Activity++;
                    if (CleanUpCounter > 300)
                    {
                        beatmapData.Activity++;
                        CleanCachedBeatmapsByActivity();
                    }
                }
                return passed;
            }
            return false;
        }

        public int GetBeatmapLength(BeatmapIdentifier beatmap)
        {
            if (_BeatmapRepository.TryGetValue(beatmap.LevelId, out var beatmapData))
                return beatmapData.BeatmapLength;
            return -1;
        }

        public async Task<bool> FetchBeatmap(BeatmapIdentifier beatmap) //Fetches beatmap from beatsaver and stores it how we need it
        {
            Beatmap? FetchBeatmap = await beatSaverAPI.BeatmapByHash(beatmap!.LevelId[13..]);
            if (FetchBeatmap != null)
            {
                ConcurrentDictionary<string, BeatmapDifficultyStats> difficulties = new();
                for (int i = 0; i < FetchBeatmap.LatestVersion.Difficulties.Count; i++)
                {
                    BeatmapDifficultyStats difficultyStats = new BeatmapDifficultyStats(
                        FetchBeatmap.LatestVersion.Difficulties[i].Chroma,
                        FetchBeatmap.LatestVersion.Difficulties[i].MappingExtensions,
                        FetchBeatmap.LatestVersion.Difficulties[i].NoodleExtensions
                        );
                    difficulties.TryAdd((FetchBeatmap.LatestVersion.Difficulties[i].Difficulty.ToString() + FetchBeatmap.LatestVersion.Difficulties[i].Characteristic.ToString()), difficultyStats);

                }
                BeatmapData data = new BeatmapData(FetchBeatmap.Metadata.Duration, difficulties);
                return _BeatmapRepository.TryAdd(beatmap.LevelId, data);
            }
            else
            {
                _BeatmapRepository.TryAdd(beatmap.LevelId, new BeatmapData());
                return false;
            }
        }

        public void CleanCachedBeatmapsByActivity() //Halves the activity on all the beatmaps and if it hits 0 then it removes the beatmap
        {
            List<string> ToRemove = new();
            foreach (var Beatmap in _BeatmapRepository)
            {
                Beatmap.Value.Activity = Beatmap.Value.Activity / 2;
                if (Beatmap.Value.Activity == 0)
                    ToRemove.Add(Beatmap.Key);
            }
            foreach (string LevelId in ToRemove)
            {
                _BeatmapRepository[LevelId].Difficulties.Clear();
                _BeatmapRepository.Remove(LevelId, out _);
            }
        }
        public void ClearCachedBeatmaps()
        {
            foreach (var Beatmap in _BeatmapRepository)
            {
                _BeatmapRepository[Beatmap.Key].Difficulties.Clear();
            }
            _BeatmapRepository.Clear();
            CleanUpCounter = 0;
        }

        public void SetRequirements(bool chroma, bool MappingExtensions, bool NoodleExtensions)
        {
            AllowChroma = chroma;
            AllowMappingExtensions = MappingExtensions;
            AllowNoodleExtensions = NoodleExtensions;
        }

    }
}
