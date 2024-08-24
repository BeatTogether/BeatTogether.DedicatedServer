using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class ByteArray : INetSerializable
    {
        public byte[]? Data { get; set; } = null;

        public void ReadFrom(ref SpanBuffer reader)
        {
            int length = (int)reader.ReadVarUInt();
            Data = reader.ReadBytes(length).ToArray();
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteVarUInt((Data != null) ? (uint)Data.Length : 0);
            writer.WriteBytes(Data);
        }
    }
}
