using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Commands
{
    public class SetDebugEnable : ITextCommand
    {
        public string CommandName => "setdebugenable";
        public string ShortHandName => "dbg";
        public string Description => "Enabled getting debug messages for this server instance through mpchat";

        public bool Enabled = false;

        public void ReadValues(string[] Values)
        {
            if (Values != null)
                Enabled = Values[0] == "true" || Values[0] == "t";
        }
    }
}
