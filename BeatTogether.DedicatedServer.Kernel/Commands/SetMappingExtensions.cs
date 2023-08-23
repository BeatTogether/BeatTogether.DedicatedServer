using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Commands
{
    public class SetMappingExtensions : ITextCommand
    {
        public string CommandName => "setmappingextensions";
        public string ShortHandName => "me";
        public string Description => "if set to false, then mapping extensions maps will be unplayable, default true";

        public bool Enabled = true;

        public void ReadValues(string[] Values)
        {
            if (Values != null)
                Enabled = Values[0] == "true" || Values[0] == "t";
        }
    }
}
