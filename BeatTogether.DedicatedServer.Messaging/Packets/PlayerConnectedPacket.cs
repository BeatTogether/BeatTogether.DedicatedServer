using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerConnectedPacket : INetSerializable
    {
        public byte RemoteConnectionId { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public bool IsConnectionOwner { get; set; }

        public void ReadFrom(ref SpanBufferReader reader)
        {
            RemoteConnectionId = reader.ReadUInt8();
            UserId = reader.ReadUTF8String();
            UserName = reader.ReadUTF8String();
            IsConnectionOwner = reader.ReadBool();
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteUInt8(RemoteConnectionId);
            writer.WriteUTF8String(UserId);
            writer.WriteUTF8String(UserName);
            writer.WriteBool(IsConnectionOwner);
        }
    }
}
