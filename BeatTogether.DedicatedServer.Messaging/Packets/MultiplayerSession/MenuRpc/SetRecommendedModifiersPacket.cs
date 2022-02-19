using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetRecommendedModifiersPacket : BaseRpcPacket
    {
        public GameplayModifiers Modifiers { get; set; } = new();

        public override void ReadFrom(ref SpanBufferReader reader)
        {
            base.ReadFrom(ref reader);
            
            if (reader.ReadUInt8() == 1)
                Modifiers.ReadFrom(ref reader);
        }

        public override void WriteTo(ref SpanBufferWriter writer)
        {
            base.WriteTo(ref writer);
            
            writer.WriteUInt8(1);
            Modifiers.WriteTo(ref writer);
        }
    }
}
