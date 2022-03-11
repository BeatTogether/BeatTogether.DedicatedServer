using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class SetGameplaySceneReadyPacket : BaseRpcWithValuesPacket
    {
        public PlayerSpecificSettings PlayerSpecificSettings { get; set; } = new();

        public override void ReadFrom(ref SpanBufferReader reader)
        {
            base.ReadFrom(ref reader);
            if (HasValue0)
                PlayerSpecificSettings.ReadFrom(ref reader);
        }

        public override void WriteTo(ref SpanBufferWriter writer)
        {
            base.WriteTo(ref writer);
            PlayerSpecificSettings.WriteTo(ref writer);
        }
    }
}
