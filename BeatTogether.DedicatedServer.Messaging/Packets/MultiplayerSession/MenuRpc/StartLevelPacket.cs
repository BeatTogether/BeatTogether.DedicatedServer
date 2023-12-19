using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class StartLevelPacket : BaseRpcWithValuesPacket
    {
        public BeatmapIdentifier Beatmap { get; set; } = new();
        public GameplayModifiers Modifiers { get; set; } = new();
        public long StartTime { get; set; }

        public override void ReadFrom(ref SpanBuffer reader)
        {
            base.ReadFrom(ref reader);
            if (HasValue0)
                Beatmap.ReadFrom(ref reader);
            if (HasValue1)
                Modifiers.ReadFrom(ref reader);
            if (HasValue2)
                StartTime = (long)reader.ReadVarULong();
        }

        public override void WriteTo(ref SpanBuffer writer)
        {
            base.WriteTo(ref writer);
            Beatmap.WriteTo(ref writer);
            Modifiers.WriteTo(ref writer);
            writer.WriteVarULong((ulong)StartTime);
        }
    }
}
