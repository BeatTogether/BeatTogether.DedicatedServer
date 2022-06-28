using System;
using System.Linq;
using System.Security.Cryptography;
using BeatTogether.DedicatedServer.Kernel.Encryption.Abstractions;
using Krypton.Buffers;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.Encryption
{
    public sealed class EncryptedPacketReader : IEncryptedPacketReader
    {
        private readonly ILogger _logger = Log.ForContext<EncryptedPacketReader>();

        /// <inheritdoc cref="IEncryptedMessageReader.ReadFrom"/>
        public ReadOnlyMemory<byte> ReadFrom(ref SpanBufferReader bufferReader, byte[] key, HMAC hmac)
        {
            var sequenceId = bufferReader.ReadUInt32();
            var iv = bufferReader.ReadBytes(16).ToArray();
            var decryptedBuffer = bufferReader.RemainingData.ToArray();
            using (var aes = Aes.Create())
            {
                aes.Padding = PaddingMode.None;
                using (var cryptoTransform = aes.CreateDecryptor(key, iv))
                {
                    var bytesWritten = 0;
                    var offset = 0;
                    for (var i = decryptedBuffer.Length; i >= cryptoTransform.InputBlockSize; i -= bytesWritten)
                    {
                        var inputCount = cryptoTransform.CanTransformMultipleBlocks
                            ? (i / cryptoTransform.InputBlockSize * cryptoTransform.InputBlockSize)
                            : cryptoTransform.InputBlockSize;
                        bytesWritten = cryptoTransform.TransformBlock(
                            decryptedBuffer, offset, inputCount,
                            decryptedBuffer, offset
                        );
                        offset += bytesWritten;
                    }
                }
            }

            var paddingByteCount = decryptedBuffer[decryptedBuffer.Length - 1] + 1;
            var hmacStart = decryptedBuffer.Length - paddingByteCount - 10;
            var decryptedBufferSpan = decryptedBuffer.AsSpan();
            var hash = decryptedBufferSpan.Slice(hmacStart, 10);
            var hashBufferWriter = new SpanBufferWriter(stackalloc byte[decryptedBuffer.Length + 4]);
            hashBufferWriter.WriteBytes(decryptedBufferSpan.Slice(0, hmacStart));
            hashBufferWriter.WriteUInt32(sequenceId);
            Span<byte> computedHash = stackalloc byte[32];
            if (!hmac.TryComputeHash(hashBufferWriter.Data, computedHash, out _))
                throw new Exception("Failed to compute message hash.");
            if (!hash.SequenceEqual(computedHash.Slice(0, 10)))
                throw new Exception("Message hash does not match the computed hash.");

            return decryptedBufferSpan.Slice(0, hmacStart).ToArray();
        }
    }
}
