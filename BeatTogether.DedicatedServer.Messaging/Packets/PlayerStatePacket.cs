using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Util;

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
    }
}
