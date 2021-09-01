using BeatTogether.DedicatedServer.Kernel.Models;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class ServerContext : IServerContext
    {
        public string Secret { get; set; }
        public string ManagerId { get; set; }
        public GameplayServerConfiguration Configuration { get; set; }
        public PlayersPermissionConfiguration Permissions { get; set; } = new();
    }
}
