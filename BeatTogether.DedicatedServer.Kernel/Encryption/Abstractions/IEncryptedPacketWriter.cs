using System;
using System.Security.Cryptography;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Kernel.Encryption.Abstractions
{
    public interface IEncryptedPacketWriter
    {
        void WriteTo(ref SpanBufferWriter bufferWriter, ReadOnlySpan<byte> data, uint sequenceId, byte[] key, HMAC hmac);
    }
}
