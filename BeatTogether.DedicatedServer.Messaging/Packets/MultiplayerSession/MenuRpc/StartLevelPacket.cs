using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class StartLevelPacket : BaseRpcPacket
    {
        public BeatmapIdentifier Beatmap { get; set; } = new();
        public GameplayModifiers Modifiers { get; set; } = new();
        public float StartTime { get; set; }

        public override void ReadFrom(ref SpanBufferReader reader)
        {
            base.ReadFrom(ref reader);
            
            if (reader.ReadUInt8() == 1)
                Beatmap.ReadFrom(ref reader);
            
            if (reader.ReadUInt8() == 1)
                Modifiers.ReadFrom(ref reader);
            
            if (reader.ReadUInt8() == 1)
                StartTime = reader.ReadFloat32();
        }

        public override void WriteTo(ref SpanBufferWriter writer)
        {
            base.WriteTo(ref writer);
            
            writer.WriteUInt8(1);
            Beatmap.WriteTo(ref writer);
            
            writer.WriteUInt8(1);
            Modifiers.WriteTo(ref writer);
            
            writer.WriteUInt8(1);
            writer.WriteFloat32(StartTime);
        }
    }
}
