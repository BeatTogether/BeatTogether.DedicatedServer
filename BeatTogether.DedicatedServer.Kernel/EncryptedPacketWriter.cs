using System;
using System.Security.Cryptography;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BinaryRecords;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class EncryptedPacketWriter : IEncryptedPacketWriter
    {
        private readonly RNGCryptoServiceProvider _rngCryptoServiceProvider;
        private readonly AesCryptoServiceProvider _aesCryptoServiceProvider;

        public EncryptedPacketWriter(
            RNGCryptoServiceProvider rngCryptoServiceProvider,
            AesCryptoServiceProvider aesCryptoServiceProvider)
        {
            _rngCryptoServiceProvider = rngCryptoServiceProvider;
            _aesCryptoServiceProvider = aesCryptoServiceProvider;
        }

        public void WriteTo(ref BinaryBufferWriter bufferWriter, ReadOnlySpan<byte> data, uint sequenceId, byte[] key, HMAC hmac)
        {
            var unencryptedBufferWriter = new BinaryBufferWriter(stackalloc byte[data.Length]);
            unencryptedBufferWriter.WriteBytes(data);

            var hashBufferWriter = new BinaryBufferWriter(stackalloc byte[data.Length + 4]);
            hashBufferWriter.WriteBytes(data);
            hashBufferWriter.WriteUInt32(sequenceId);
            Span<byte> hash = stackalloc byte[32];
            if (!hmac.TryComputeHash(hashBufferWriter.Data, hash, out _))
                throw new Exception("Failed to compute message hash.");
            unencryptedBufferWriter.WriteBytes(hash.Slice(0, 10));

            var iv = new byte[16];
            _rngCryptoServiceProvider.GetBytes(iv);

            var paddingByteCount = (byte)((16 - ((unencryptedBufferWriter.Size + 1) & 15)) & 15);
            for (var i = 0; i < paddingByteCount + 1; i++)
                unencryptedBufferWriter.WriteUInt8(paddingByteCount);

            var encryptedBuffer = unencryptedBufferWriter.Data.ToArray();
            using (var cryptoTransform = _aesCryptoServiceProvider.CreateEncryptor(key, iv))
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

            bufferWriter.WriteUInt32(sequenceId);
            bufferWriter.WriteBytes(iv);
            bufferWriter.WriteBytes(encryptedBuffer);
        }
    }
}
