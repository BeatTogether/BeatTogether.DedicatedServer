using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class SetGameplaySceneReadyPacket : BaseRpcPacket
    {
        public PlayerSpecificSettings PlayerSpecificSettings { get; set; } = new();

        public override void ReadFrom(ref SpanBufferReader reader)
        {
            base.ReadFrom(ref reader);
            
            if (reader.ReadUInt8() == 1)
                PlayerSpecificSettings.ReadFrom(ref reader);
        }

        public override void WriteTo(ref SpanBufferWriter writer)
        {
            base.WriteTo(ref writer);
            
            writer.WriteUInt8(1);
            PlayerSpecificSettings.WriteTo(ref writer);
        }
    }
}
