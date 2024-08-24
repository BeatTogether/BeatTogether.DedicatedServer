using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.CommandHandlers;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.Commands.CommandHandlers
{
    class ForceStartCommandHandler : BaseCommandHandler<ForceStartCommand>
    {
        private readonly ILogger _logger = Log.ForContext<ForceStartCommandHandler>();
        private readonly ILobbyManager _lobbyManager;

        public ForceStartCommandHandler(ILobbyManager lobbyManager)
        {
            _lobbyManager = lobbyManager;
        }

        public override void Handle(IPlayer player, ForceStartCommand command)
        {
            _logger.Information(player.UserName + "Has force started a beatmap");
            _lobbyManager.ForceStartSelectedBeatmap = true;
        }
    }
}
