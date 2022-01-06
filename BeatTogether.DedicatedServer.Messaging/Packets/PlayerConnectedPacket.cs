using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
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
            UserId = reader.ReadString();
            UserName = reader.ReadString();
            IsConnectionOwner = reader.ReadBool();
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteUInt8(RemoteConnectionId);
            writer.WriteString(UserId);
            writer.WriteString(UserName);
            writer.WriteBool(IsConnectionOwner);
        }
    }
}
