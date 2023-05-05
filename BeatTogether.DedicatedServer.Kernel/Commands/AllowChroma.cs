using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Commands
{
    public class AllowChroma : ITextCommand
    {
        public string CommandName => "allowchroma";
        public string ShortHandName => "ch";
        public string Description => "if set to false, then chroma maps will be unplayable, default true";

        public bool Enabled = true;

        public void ReadValues(string[] Values)
        {
            if (Values != null)
                Enabled = Values[0] == "true";
        }
    }
}
