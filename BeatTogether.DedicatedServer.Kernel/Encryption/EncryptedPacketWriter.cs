using System;
using System.Security.Cryptography;
using BeatTogether.DedicatedServer.Kernel.Encryption.Abstractions;
using BeatTogether.LiteNetLib.Util;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Kernel.Encryption
{
    public sealed class EncryptedPacketWriter : IEncryptedPacketWriter
    {
        private readonly RandomNumberGenerator _rngCryptoServiceProvider;

        public EncryptedPacketWriter(
            RandomNumberGenerator rngCryptoServiceProvider)
        {
            _rngCryptoServiceProvider = rngCryptoServiceProvider;
        }
        public void WriteTo(ref SpanBufferWriter bufferWriter, ReadOnlySpan<byte> data, uint sequenceId, byte[] key, HMAC hmac)
        {
            var unencryptedBufferWriter = new SpanBuffer(stackalloc byte[data.Length + 128 + 10], false);
            unencryptedBufferWriter.WriteBytes(data);
            unencryptedBufferWriter.WriteUInt32(sequenceId);
            Span<byte> hash = stackalloc byte[32];
            if (!hmac.TryComputeHash(unencryptedBufferWriter.Data, hash, out _))
                throw new Exception("Failed to compute message hash.");
            unencryptedBufferWriter.SetOffset(data.Length);
            unencryptedBufferWriter.WriteBytes(hash.Slice(0, 10));

            var iv = new byte[16];
            _rngCryptoServiceProvider.GetBytes(iv);

            var paddingByteCount = (byte)((16 - ((unencryptedBufferWriter.Size + 1) & 15)) & 15);
            for (var i = 0; i < paddingByteCount + 1; i++)
                unencryptedBufferWriter.WriteUInt8(paddingByteCount);

            var encryptedBuffer = unencryptedBufferWriter.Data.ToArray();
            using (var aes = Aes.Create())
            {
                aes.Padding = PaddingMode.None;
                using (var cryptoTransform = aes.CreateEncryptor(key, iv))
                {
                    var bytesWritten = 0;
                    for (var i = encryptedBuffer.Length; i >= cryptoTransform.InputBlockSize; i -= bytesWritten)
                    {
                        var inputCount = cryptoTransform.CanTransformMultipleBlocks
                            ? (i / cryptoTransform.InputBlockSize * cryptoTransform.InputBlockSize)
                            : cryptoTransform.InputBlockSize;
                        bytesWritten = cryptoTransform.TransformBlock(
                            encryptedBuffer, bytesWritten, inputCount,
                            encryptedBuffer, bytesWritten
                        );
                    }
                }
            }

            bufferWriter.WriteUInt32(sequenceId);
            bufferWriter.WriteBytes(iv);
            bufferWriter.WriteBytes(encryptedBuffer);
        }
    }
}
