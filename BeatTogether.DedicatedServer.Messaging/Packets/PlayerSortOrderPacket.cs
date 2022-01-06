using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerSortOrderPacket : INetSerializable
    {
        public string UserId { get; set; } = null!;
        public int SortIndex { get; set; }

        public void ReadFrom(ref SpanBufferReader reader)
        {
            UserId = reader.ReadString();
            SortIndex = reader.ReadVarInt();
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteString(UserId);
            writer.WriteVarInt(SortIndex);
        }
    }
}
