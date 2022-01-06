using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class ConnectionRequestData : INetSerializable
    {
        public string Secret { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public bool IsConnectionOwner { get; set; }

        public void ReadFrom(ref SpanBufferReader reader)
        {
            Secret = reader.ReadString();
            UserId = reader.ReadString();
            UserName = reader.ReadString();
            IsConnectionOwner = reader.ReadBool();
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteString(Secret);
            writer.WriteString(UserId);
            writer.WriteString(UserName);
            writer.WriteBool(IsConnectionOwner);
        }
    }
}
