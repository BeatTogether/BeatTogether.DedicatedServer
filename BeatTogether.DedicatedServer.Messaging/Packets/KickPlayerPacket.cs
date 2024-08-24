using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class KickPlayerPacket : INetSerializable
    {
        public DisconnectedReason DisconnectedReason { get; set; }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteVarInt((int)DisconnectedReason);
        }

        public void ReadFrom(ref SpanBuffer reader)
        {
            DisconnectedReason = (DisconnectedReason)reader.ReadVarInt();
        }
    }
}
