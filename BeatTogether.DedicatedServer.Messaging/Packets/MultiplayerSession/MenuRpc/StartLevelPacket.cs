using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class StartLevelPacket : BaseRpcWithValuesPacket
    {
        public BeatmapIdentifier Beatmap { get; set; } = new();
        public GameplayModifiers Modifiers { get; set; } = new();
        public float StartTime { get; set; }

        public override void ReadFrom(ref SpanBufferReader reader)
        {
            base.ReadFrom(ref reader);
            if (HasValue0)
                Beatmap.ReadFrom(ref reader);
            if (HasValue1)
                Modifiers.ReadFrom(ref reader);
            if (HasValue2)
                StartTime = reader.ReadFloat32();
        }

        public override void WriteTo(ref SpanBufferWriter writer)
        {
            base.WriteTo(ref writer);
            Beatmap.WriteTo(ref writer);
            Modifiers.WriteTo(ref writer);
            writer.WriteFloat32(StartTime);
        }
    }
}
