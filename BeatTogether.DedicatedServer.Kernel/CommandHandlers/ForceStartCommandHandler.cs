using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.CommandHandlers;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.Commands.CommandHandlers
{
    class ForceStartCommandHandler : BaseCommandHandler<ForceStartCommand>
    {
        private readonly ILobbyManager _lobbyManager;
        private readonly Internal_Logger _logger;

        public ForceStartCommandHandler(ILobbyManager lobbyManager, Kernel_Logger kernel_Logger)
        {
            _lobbyManager = lobbyManager;
            _logger = kernel_Logger.ForContext<ForceStartCommandHandler>();
        }

        public override void Handle(IPlayer player, ForceStartCommand command)
        {
            _logger.Information(player.UserName + "Has force started a beatmap");
            _lobbyManager.ForceStartSelectedBeatmap = true;
        }
    }
}
