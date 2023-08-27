using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Commands
{
    public class ForceStartCommand : ITextCommand
    {
        public string CommandName => "forcestart";
        public string ShortHandName => "fs";
        public string Description => "force starts the beatmap ignoring all players entitlements. Could cause players to have issues";

        public void ReadValues(string[] Values)
        {
        }
    }
}
