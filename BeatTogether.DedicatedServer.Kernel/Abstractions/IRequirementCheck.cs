using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Messaging.Models;

namespace BeatTogether.DedicatedServer.Kernel
{
    public interface IRequirementCheck
    {
        Task<bool> DoesPlayerMeetMapRequirements(Player player, BeatmapIdentifier? beatmap);
        Task<bool> DoAllPlayersMeetRequirements(PlayerRegistry players, BeatmapIdentifier? beatmap);

    }
}
