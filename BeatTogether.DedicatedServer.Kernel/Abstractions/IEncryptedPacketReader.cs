﻿using System;
using System.Security.Cryptography;
using BinaryRecords;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IEncryptedPacketReader
    {
        ReadOnlyMemory<byte> ReadFrom(ref BinaryBufferReader bufferReader, byte[] key, HMAC hmac);
    }
}