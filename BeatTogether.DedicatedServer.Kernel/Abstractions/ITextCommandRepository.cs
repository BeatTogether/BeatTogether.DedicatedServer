using BeatTogether.DedicatedServer.Kernel.Enums;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface ITextCommandRepository
    {
        bool GetCommand(string[] commandValues, AccessLevel accessLevel, out ITextCommand Command);
        void RegisterCommand<T>(AccessLevel accessLevel) where T : class, ITextCommand, new();
        string[] GetTextCommandNames(AccessLevel accessLevel);
    }
}
