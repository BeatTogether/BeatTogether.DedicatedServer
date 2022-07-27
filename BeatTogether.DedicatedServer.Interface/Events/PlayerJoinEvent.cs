using BeatTogether.DedicatedServer.Interface.Models;
using System.Net;

namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record PlayerJoinEvent(string Secret, string EndPoint, string UserId, string UserName, byte ConnectionId, int SortId, AvatarData AvatarData);
}
