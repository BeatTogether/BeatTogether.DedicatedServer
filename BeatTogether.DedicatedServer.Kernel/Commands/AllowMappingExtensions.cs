using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Commands
{
    public class AllowMappingExtensions : ITextCommand
    {
        public string CommandName => "allowme";
        public string ShortHandName => "me";
        public string Description => "if set to false, then mapping extensions maps will be unplayable, default true";

        public bool Enabled = true;

        public void ReadValues(string[] Values)
        {
            if (Values != null)
                Enabled = Values[0] == "true";
        }
    }
}
