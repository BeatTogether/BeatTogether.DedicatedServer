using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Commands
{
    public class SetWelcomeMessage : ITextCommand
    {
        public string CommandName => "setwelcomemessage";
        public string ShortHandName => "swm";
        public string Description => "Enter some text to set the welcome message for this server";

        public string Text = string.Empty;

        public void ReadValues(string[] Values)
        {
            if (Values != null)
                Text = Values[0];
        }
    }
}
