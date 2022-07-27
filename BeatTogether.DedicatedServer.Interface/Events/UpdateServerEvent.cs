using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record UpdateServerEvent(string Secret, GameplayServerConfiguration Configuration, int Port, string ManagerId, string ServerId, string ServerName, float DestroyInstanceTimeout, string ConstantManager, bool PerPlayerDifficulties, bool PerPlayerModifiers, bool Chroma, bool MappingExtensions, bool NoodleExtensions, float KickBeforeEntitlementTimeout, float ReadyCountdownTime, float MapStartCountdownTime, float ResultsTime);
}
