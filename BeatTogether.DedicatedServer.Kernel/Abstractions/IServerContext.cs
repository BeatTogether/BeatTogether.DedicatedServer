using BeatTogether.DedicatedServer.Kernel.Models;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IServerContext
    {
        string Secret { get; set; }
        string ManagerId { get; set; }
        GameplayServerConfiguration Configuration { get; set; }
        PlayersPermissionConfiguration Permissions { get; set; }
    }
}
