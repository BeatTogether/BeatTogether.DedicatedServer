using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Commands
{
    public class SetCountdown : ITextCommand
    {
        public string CommandName => "setcountdown";
        public string ShortHandName => "sc";
        public string Description => "enter a number to set the countdown, default is 30 seconds";

        public int Countdown = 30;

        public void ReadValues(string[] Values)
        {
            if (Values != null)
                Countdown = int.Parse(Values[0]);
        }
    }
}
