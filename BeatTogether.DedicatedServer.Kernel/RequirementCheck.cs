using BeatSaverSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatSaverSharp.Models;
using System.Threading;

namespace BeatTogether.DedicatedServer.Kernel
{
    internal class RequirementCheck : IRequirementCheck
    {
        private BeatSaver beatSaverAPI = new("BeatTogetherDedicatedInstance", new Version("1.0.0"));

        public async Task<bool> DoesPlayerMeetMapRequirements(Player player, BeatmapIdentifier? beatmap)
        {
            Task<Beatmap?> FetchBeatmap = beatSaverAPI.Beatmap(beatmap!.LevelId);
            await FetchBeatmap.ConfigureAwait(false);
            if (FetchBeatmap.Result != null)
            {
                BeatSaverSharp.Models.BeatmapDifficulty beatmapDifficulty = FetchBeatmap.Result.LatestVersion.Difficulties[((int)beatmap.Difficulty)];
                bool chroma = beatmapDifficulty.Chroma;
                bool ME = beatmapDifficulty.MappingExtensions;
                bool NE = beatmapDifficulty.NoodleExtensions;
                Console.WriteLine("The map is: " + FetchBeatmap.Result.Name + " And has requirements: NE: " + NE + " ME: " + ME + " Chroma: " + chroma);
                if (chroma != player.Chroma_Installed)
                {
                    return false;
                }
                if (ME != player.ME_Installed)
                {
                    return false;
                }
                if (NE != player.NE_Installed)
                {
                    return false;
                }
                return true; //all requirements met
            }
            else
                return false; //map not found
        }

        public async Task<bool> DoAllPlayersMeetRequirements(PlayerRegistry players, BeatmapIdentifier? beatmap)
        {
            Task<Beatmap?> FetchBeatmap = beatSaverAPI.Beatmap(beatmap!.LevelId);
            await FetchBeatmap.ConfigureAwait(false);
            if(FetchBeatmap.Result != null)
            {
                BeatSaverSharp.Models.BeatmapDifficulty beatmapDifficulty = FetchBeatmap.Result.LatestVersion.Difficulties[((int)beatmap.Difficulty)];
                bool chroma = beatmapDifficulty.Chroma;
                bool ME = beatmapDifficulty.MappingExtensions;
                bool NE = beatmapDifficulty.NoodleExtensions;
                Console.WriteLine("The map is: " + FetchBeatmap.Result.Name + " And has requirements: NE: " + NE + " ME: " + ME + " Chroma: " + chroma);
                foreach (Player player in players.Players)
                {
                    if (chroma != player.Chroma_Installed)
                    {
                        return false;
                    }
                    if (ME != player.ME_Installed)
                    {
                        return false;
                    }
                    if (NE != player.NE_Installed)
                    {
                        return false;
                    }
                }
                return true; //when all players have the requirements
            }
            else
            {
                return false; //if map was not found on beatsaver This would make base game un-playable
            }
        }
    }
}
