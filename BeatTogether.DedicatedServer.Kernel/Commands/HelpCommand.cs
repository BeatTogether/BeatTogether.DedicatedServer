using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Commands
{
    public class HelpCommand : ITextCommand
    {
        public string CommandName => "help";
        public string ShortHandName => "h";
        public string Description => "Displays a list of useable commands, Type the name of the command after to get its description";

        public string[]? SpecificCommandName = null;

        public void ReadValues(string[] Values)
        {
            if (Values != null)
                SpecificCommandName = Values;
        }
    }
}
