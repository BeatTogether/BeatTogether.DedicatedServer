using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class ObstacleSpawnPacket : BaseRpcWithValuesPacket
    {
        public override void ReadFrom(ref SpanBuffer reader, Version version)
        {
            base.ReadFrom(ref reader, version);
        }

        public override void WriteTo(ref SpanBuffer writer, Version version)
        {
            base.WriteTo(ref writer, version);
        }
    }
}
