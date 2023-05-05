using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Commands
{
    public class AllowNoodle : ITextCommand
    {
        public string CommandName => "allownoodle";
        public string ShortHandName => "ne";
        public string Description => "if set to false, then noodle maps will be unplayable, default true";

        public bool Enabled = true;

        public void ReadValues(string[] Values)
        {
            if (Values != null)
                Enabled = Values[0] == "true";
        }
    }
}
