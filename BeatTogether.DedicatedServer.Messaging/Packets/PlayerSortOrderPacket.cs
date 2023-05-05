using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerSortOrderPacket : INetSerializable
    {
        public string UserId { get; set; } = null!;
        public int SortIndex { get; set; }

        public void ReadFrom(ref SpanBuffer reader)
        {
            UserId = reader.ReadString();
            SortIndex = reader.ReadVarInt();
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteString(UserId);
            writer.WriteVarInt(SortIndex);
        }
    }
}
