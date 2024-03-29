﻿using BeatTogether.Core.Messaging.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Messages.Handshake
{
    public sealed class ClientHelloRequest : IMessage, IReliableRequest
    {
        public uint RequestId { get; set; }
        public byte[] Random { get; set; }

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteBytes(Random);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Random = bufferReader.ReadBytes(32).ToArray();
        }
    }
}