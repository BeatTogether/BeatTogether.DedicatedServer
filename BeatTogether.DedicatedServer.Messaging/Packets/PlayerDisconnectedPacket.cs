using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerDisconnectedPacket : INetSerializable
    {
        public DisconnectedReason DisconnectedReason { get; set; }

        public void ReadFrom(ref SpanBuffer reader)
        {
            DisconnectedReason = (DisconnectedReason)reader.ReadVarInt();
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteVarInt((int)DisconnectedReason);
        }
    }
}
