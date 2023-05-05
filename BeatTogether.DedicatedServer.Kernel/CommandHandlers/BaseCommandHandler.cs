using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.CommandHandlers
{
    public interface ICommandHandler
    {
        void Handle(IPlayer sender, ITextCommand command);
    }

    public interface ICommandHandler<TCommand> : ICommandHandler
        where TCommand : class, ITextCommand
    {
        void Handle(IPlayer sender, TCommand command);
    }

    public abstract class BaseCommandHandler<Tcommand> : ICommandHandler<Tcommand> where Tcommand : class, ITextCommand
    {
        public abstract void Handle(IPlayer sender, Tcommand command);

        public void Handle(IPlayer sender, ITextCommand command)
            => Handle(sender, (Tcommand)command);
    }
}
