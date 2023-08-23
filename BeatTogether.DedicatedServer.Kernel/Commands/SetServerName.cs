using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Commands
{
    public class SetServerName : ITextCommand
    {
        public string CommandName => "setservername";
        public string ShortHandName => "ssn";
        public string Description => "Enter some text to set the name of the server";

        public string Name = string.Empty;

        public void ReadValues(string[] Values)
        {
            if (Values != null)
                Name = Values[0];
        }
    }
}
