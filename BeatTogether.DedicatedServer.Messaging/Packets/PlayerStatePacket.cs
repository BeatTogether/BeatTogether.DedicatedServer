using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerStatePacket : INetSerializable
    {
        public PlayerStateHash PlayerState { get; set; } = new();

        public void ReadFrom(ref SpanBuffer reader)
        {
            PlayerState.ReadFrom(ref reader);
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            PlayerState.WriteTo(ref writer);
        }
        public void ReadFrom(ref MemoryBuffer reader)
        {
            PlayerState.ReadFrom(ref reader);
        }

        public void WriteTo(ref MemoryBuffer writer)
        {
            PlayerState.WriteTo(ref writer);
        }
    }
}
