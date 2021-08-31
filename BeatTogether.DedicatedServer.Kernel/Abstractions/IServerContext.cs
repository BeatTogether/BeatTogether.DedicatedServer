using BeatTogether.DedicatedServer.Kernel.Models;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IServerContext
    {
        List<IPlayer> Players { get; }
        string Secret { get; set; }
        string ManagerId { get; set; }
        GameplayServerConfiguration Configuration { get; set; }
        PlayersPermissionConfiguration Permissions { get; set; }
        MultiplayerGameState State { get; set; }

        void AddPlayer(IPlayer player);
        void RemovePlayer(IPlayer player);

        IPlayer GetPlayer(byte connectionId);
        IPlayer GetPlayer(string userId);
        bool TryGetPlayer(byte connectionId, [MaybeNullWhen(false)] out IPlayer player);
        bool TryGetPlayer(string userId, [MaybeNullWhen(false)] out IPlayer player);
    }
}
