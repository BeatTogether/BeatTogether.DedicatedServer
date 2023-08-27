using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Commands
{
    public class SetNoteRouting : ITextCommand
    {
        public string CommandName => "snotesvisible";
        public string ShortHandName => "n";
        public string Description => "disables beatmap notes if set to true. Notes will be disabled automaticly if there are over 14 players, default false";

        public bool Enabled = true;

        public void ReadValues(string[] Values)
        {
            if (Values != null)
                Enabled = Values[0] == "true" || Values[0] == "t";
        }
    }
}
