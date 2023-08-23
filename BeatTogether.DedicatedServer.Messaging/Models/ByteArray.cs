using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;
using Krypton.Buffers;
using System;

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
