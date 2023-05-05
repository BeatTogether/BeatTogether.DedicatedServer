using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Commands
{
    public class DisableBeatmapNotes : ITextCommand
    {
        public string CommandName => "disablenotes";
        public string ShortHandName => "dn";
        public string Description => "disables beatmap notes if set to true. Notes will be disabled automaticly if there are over 14 players, default false";

        public bool Disabled = false;

        public void ReadValues(string[] Values)
        {
            if (Values != null)
               Disabled = Values[0] == "true";
        }
    }
}
