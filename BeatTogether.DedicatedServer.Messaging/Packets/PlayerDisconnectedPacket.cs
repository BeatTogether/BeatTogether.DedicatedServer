using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerDisconnectedPacket : INetSerializable
    {
        public DisconnectedReason DisconnectedReason { get; set; }

        public void ReadFrom(ref SpanBufferReader reader)
        {
            DisconnectedReason = (DisconnectedReason)reader.ReadVarInt();
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteVarInt((int)DisconnectedReason);
        }
    }
}
