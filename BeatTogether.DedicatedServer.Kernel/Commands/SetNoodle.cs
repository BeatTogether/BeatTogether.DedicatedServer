using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Commands
{
    public class SetNoodleExtensions : ITextCommand
    {
        public string CommandName => "setnoodleextensions";
        public string ShortHandName => "ne";
        public string Description => "if set to false, then noodle maps will be unplayable, default true";

        public bool Enabled = true;

        public void ReadValues(string[] Values)
        {
            if (Values != null)
                Enabled = Values[0] == "true" || Values[0] == "t";
        }
    }
}
