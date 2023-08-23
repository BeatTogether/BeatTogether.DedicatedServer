using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Commands
{
    public class SetBeatmapStartTime : ITextCommand
    {
        public string CommandName => "setbeatmapstart";
        public string ShortHandName => "sbs";
        public string Description => "this is the countdown that triggers once everyone is ready, and when the beatmap starts downloading, default is 5 seconds";

        public int Countdown = 5;

        public void ReadValues(string[] Values)
        {
            if (Values != null)
                Countdown = int.Parse(Values[0]);
        }
    }
}
