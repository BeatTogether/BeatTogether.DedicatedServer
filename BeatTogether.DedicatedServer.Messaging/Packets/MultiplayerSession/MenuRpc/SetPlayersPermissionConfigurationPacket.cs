using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetPlayersPermissionConfigurationPacket : BaseRpcWithValuesPacket
    {
        public PlayersPermissionConfiguration PermissionConfiguration { get; set; } = new();

        public override void ReadFrom(ref SpanBuffer reader, Version version)
        {
            base.ReadFrom(ref reader, version);
            if (HasValue0)
                PermissionConfiguration.ReadFrom(ref reader);
        }

        public override void WriteTo(ref SpanBuffer writer, Version version)
        {
            base.WriteTo(ref writer, version);
            PermissionConfiguration.WriteTo(ref writer);
        }
    }
}
