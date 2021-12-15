using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerStatePacket : INetSerializable
    {
        public PlayerStateHash PlayerState { get; set; } = new();

        public void ReadFrom(ref SpanBufferReader reader)
        {
            PlayerState.ReadFrom(ref reader);
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            PlayerState.WriteTo(ref writer);
        }
    }
}
