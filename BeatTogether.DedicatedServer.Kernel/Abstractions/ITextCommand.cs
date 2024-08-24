namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface ITextCommand
    {
        string CommandName { get; }
        string ShortHandName { get; }
        string Description { get; }
        public void ReadValues(string[] Values);
    }
}
