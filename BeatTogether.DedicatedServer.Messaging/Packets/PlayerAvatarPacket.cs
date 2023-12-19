using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerAvatarPacket : INetSerializable
    {
        public MultiplayerAvatarsData PlayerAvatar { get; set; } = new();

        public void ReadFrom(ref SpanBuffer reader)
        {
            PlayerAvatar.ReadFrom(ref reader);
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            PlayerAvatar.WriteTo(ref writer);
        }
    }
}
