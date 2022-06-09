using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using BeatTogether.Core.Security.Abstractions;
using BeatTogether.Core.Security.Models;
using BeatTogether.DedicatedServer.Kernel.Encryption.Abstractions;
using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.Encryption
{
    public sealed class PacketEncryptionLayer : IPacketLayer
    {
        public byte[] Random { get; } = new byte[32];
        public ECKeyPair KeyPair { get; }

        private readonly IEncryptedPacketReader _encryptedPacketReader;
        private readonly IEncryptedPacketWriter _encryptedPacketWriter;
        private readonly IDiffieHellmanService _diffieHellmanService;
        private readonly ILogger _logger = Log.ForContext<PacketEncryptionLayer>();

        private readonly ConcurrentDictionary<IPAddress, EncryptionParameters> _potentialEncryptionParameters = new();
        private readonly ConcurrentDictionary<EndPoint, EncryptionParameters> _encryptionParameters = new();

        private static byte[] _masterSecretSeed = Encoding.UTF8.GetBytes("master secret");
        private static byte[] _keyExpansionSeed = Encoding.UTF8.GetBytes("key expansion");

        public PacketEncryptionLayer(
            IEncryptedPacketReader encryptedPacketReader,
            IEncryptedPacketWriter encryptedPacketWriter,
            IDiffieHellmanService diffieHellmanService,
            RNGCryptoServiceProvider rngCryptoServiceProvider)
        {
            _encryptedPacketReader = encryptedPacketReader;
            _encryptedPacketWriter = encryptedPacketWriter;
            _diffieHellmanService = diffieHellmanService;

            rngCryptoServiceProvider.GetBytes(Random);
            KeyPair = _diffieHellmanService.GetECKeyPair();
        }

        #region Public Methods

        public void AddEncryptedEndPoint(
            IPEndPoint endPoint,
            byte[] clientRandom,
            byte[] clientPublicKey)
        {
            var clientPublicKeyParameters = _diffieHellmanService.DeserializeECPublicKey(clientPublicKey);
            var preMasterSecret = _diffieHellmanService.GetPreMasterSecret(clientPublicKeyParameters, KeyPair.PrivateKeyParameters);
            var sendKey = new byte[32];
            var receiveKey = new byte[32];
            var sendMacSourceArray = new byte[64];
            var receiveMacSourceArray = new byte[64];
            var masterSecretSeed = MakeSeed(_masterSecretSeed, Random, clientRandom);
            var keyExpansionSeed = MakeSeed(_keyExpansionSeed, Random, clientRandom);
            var sourceArray = PRF(
                PRF(preMasterSecret, masterSecretSeed, 48),
                keyExpansionSeed,
                192
            );
            Array.Copy(sourceArray, 0, sendKey, 0, 32);
            Array.Copy(sourceArray, 32, receiveKey, 0, 32);
            Array.Copy(sourceArray, 64, sendMacSourceArray, 0, 64);
            Array.Copy(sourceArray, 128, receiveMacSourceArray, 0, 64);
            var encryptionParameters = new EncryptionParameters(
                receiveKey,
                sendKey,
                new HMACSHA256(receiveMacSourceArray),
                new HMACSHA256(sendMacSourceArray)
            );
            _potentialEncryptionParameters[endPoint.Address] = encryptionParameters;
            _encryptionParameters.TryRemove(endPoint, out _);
        }

        public void RemoveEncryptedEndPoint(IPEndPoint endPoint)
        {
            _potentialEncryptionParameters.TryRemove(endPoint.Address, out _);
            _encryptionParameters.TryRemove(endPoint, out _);
        }

        public void ProcessInboundPacket(EndPoint endPoint, ref Span<byte> data)
        {
            var address = ((IPEndPoint)endPoint).Address;

            if (data.Length == 0)
                return;

            var bufferReader = new SpanBufferReader(data);
            if (!bufferReader.ReadBool())  // isEncrypted
            {
                _logger.Warning($"Received an unencrypted packet (RemoteEndPoint='{endPoint}').");
                return;
            }

            byte[]? decryptedData;

            if (_encryptionParameters.TryGetValue(endPoint, out var encryptionParameters))
            {
                if (TryDecrypt(ref bufferReader, encryptionParameters, out decryptedData))
                    data = decryptedData;
                else
                    data = Array.Empty<byte>();
                return;
            }

            if (_potentialEncryptionParameters.TryGetValue(address, out encryptionParameters)) {
                if (TryDecrypt(ref bufferReader, encryptionParameters, out decryptedData))
                {
                    _encryptionParameters[endPoint] = encryptionParameters;
                    _potentialEncryptionParameters.TryRemove(address, out _);
                    data = decryptedData;
                }
                else
                    data = Array.Empty<byte>();
                return;
            }

            _logger.Verbose(
                "Failed to retrieve decryption parameters " +
                $"(RemoteEndPoint='{endPoint}')."
            );
            data = Array.Empty<byte>();
        }

        public void ProcessOutBoundPacket(EndPoint endPoint, ref Span<byte> data)
        {
            if (!_encryptionParameters.TryGetValue(endPoint, out var encryptionParameters))
            {
                _logger.Verbose(
                    "Failed to retrieve encryption parameters, defaulting to no encryption " +
                    $"(RemoteEndPoint='{endPoint}')."
                );
                return;
            }

            var bufferWriter = new SpanBufferWriter(stackalloc byte[412]);
            bufferWriter.WriteBool(true);  // isEncrypted
            _encryptedPacketWriter.WriteTo(
                ref bufferWriter, data.Slice(0, data.Length),
                encryptionParameters.GetNextSequenceId(),
                encryptionParameters.SendKey, encryptionParameters.SendMac);
            data = bufferWriter.Data.ToArray();
        }

        #endregion

        #region Private Methods

        private byte[] MakeSeed(byte[] baseSeed, byte[] serverSeed, byte[] clientSeed)
        {
            var seed = new byte[baseSeed.Length + serverSeed.Length + clientSeed.Length];
            Array.Copy(baseSeed, 0, seed, 0, baseSeed.Length);
            Array.Copy(serverSeed, 0, seed, baseSeed.Length, serverSeed.Length);
            Array.Copy(clientSeed, 0, seed, baseSeed.Length + serverSeed.Length, clientSeed.Length);
            return seed;
        }

        private byte[] PRF(byte[] key, byte[] seed, int length)
        {
            var i = 0;
            var array = new byte[length + seed.Length];
            while (i < length)
            {
                Array.Copy(seed, 0, array, i, seed.Length);
                PRFHash(key, array, ref i);
            }
            var array2 = new byte[length];
            Array.Copy(array, 0, array2, 0, length);
            return array2;
        }

        private void PRFHash(byte[] key, byte[] seed, ref int length)
        {
            using var hmacsha256 = new HMACSHA256(key);
            var array = hmacsha256.ComputeHash(seed, 0, length);
            var num = Math.Min(length + array.Length, seed.Length);
            Array.Copy(array, 0, seed, length, num - length);
            length = num;
        }

        private bool TryDecrypt(
            ref SpanBufferReader bufferReader,
            EncryptionParameters encryptionParameters,
            [MaybeNullWhen(false)] out byte[] data)
        {
            try
            {
                data = _encryptedPacketReader
                    .ReadFrom(ref bufferReader, encryptionParameters.ReceiveKey, encryptionParameters.ReceiveMac)
                    .ToArray();
                return true;
            }
            catch (Exception e)
            {
                _logger.Warning($"Failed to decrypt packet: {e.Message}");
                data = null;
                return false;
            }
        }

        #endregion
    }
}
