using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class LevelFinishedPacket : BaseRpcWithValuesPacket
    {
        public MultiplayerLevelCompletionResults Results { get; set; } = new();

        public override void ReadFrom(ref SpanBuffer reader, Version version)
        {
            base.ReadFrom(ref reader, version);
            if (HasValue0)
                Results.ReadFrom(ref reader);
        }

        public override void WriteTo(ref SpanBuffer writer, Version version)
        {
            base.WriteTo(ref writer, version);
            Results.WriteTo(ref writer);
        }
    }
}
