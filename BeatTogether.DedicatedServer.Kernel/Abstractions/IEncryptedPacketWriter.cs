using System;
using System.Security.Cryptography;
using BinaryRecords;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IEncryptedPacketWriter
    {
        void WriteTo(ref BinaryBufferWriter bufferWriter, ReadOnlySpan<byte> data, uint sequenceId, byte[] key, HMAC hmac);
    }
}
