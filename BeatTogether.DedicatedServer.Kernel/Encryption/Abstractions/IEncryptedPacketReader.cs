using Krypton.Buffers;
using System;
using System.Security.Cryptography;

namespace BeatTogether.DedicatedServer.Kernel.Encryption.Abstractions
{
    public interface IEncryptedPacketReader
    {
        ReadOnlyMemory<byte> ReadFrom(ref SpanBufferReader bufferReader, byte[] key, HMAC hmac);
    }
}
