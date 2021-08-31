using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IPlayerRegistry
    {
        void AddPlayer(IPlayer player);
        void RemovePlayer(IPlayer player);

        IPlayer GetPlayer(EndPoint remoteEndPoint);

        bool TryGetPlayer(EndPoint remoteEndPoint, [MaybeNullWhen(false)] out IPlayer player);
    }
}
