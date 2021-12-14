using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class ConnectionRequestData : INetSerializable
    {
        public string? Secret { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public bool IsConnectionOwner { get; set; }

        public void ReadFrom(ref SpanBufferReader reader)
        {
            Secret = reader.ReadUTF8String();
            UserId = reader.ReadUTF8String();
            UserName = reader.ReadUTF8String();
            IsConnectionOwner = reader.ReadBool();
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteUTF8String(Secret);
            writer.WriteUTF8String(UserId);
            writer.WriteUTF8String(UserName);
            writer.WriteBool(IsConnectionOwner);
        }
    }
}
