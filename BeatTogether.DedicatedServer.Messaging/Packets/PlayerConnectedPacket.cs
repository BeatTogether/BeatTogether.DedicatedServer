using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerConnectedPacket : INetSerializable
    {
        public byte RemoteConnectionId { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public bool IsConnectionOwner { get; set; }

        public void ReadFrom(ref SpanBuffer reader)
        {
            RemoteConnectionId = reader.ReadUInt8();
            UserId = reader.ReadString();
            UserName = reader.ReadString();
            IsConnectionOwner = reader.ReadBool();
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteUInt8(RemoteConnectionId);
            writer.WriteString(UserId);
            writer.WriteString(UserName);
            writer.WriteBool(IsConnectionOwner);
        }
    }
}
