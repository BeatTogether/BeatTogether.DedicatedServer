using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Commands
{
    public class SetResultsTime : ITextCommand
    {
        public string CommandName => "setresultstime";
        public string ShortHandName => "srt";
        public string Description => "the length of the results screen, default is 20 seconds";

        public int Countdown = 20;

        public void ReadValues(string[] Values)
        {
            if (Values != null)
                Countdown = int.Parse(Values[0]);
        }
    }
}
