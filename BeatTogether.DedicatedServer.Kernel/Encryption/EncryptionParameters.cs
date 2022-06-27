using System.Security.Cryptography;
using System.Threading;

namespace BeatTogether.DedicatedServer.Kernel.Encryption
{
    public sealed class EncryptionParameters
    {
        public byte[] ReceiveKey { get; }
        public byte[] SendKey { get; }
        public byte[] ReceiveMac { get; }
        public byte[] SendMac { get; }

        private uint _lastSequenceId = 0U;

        public EncryptionParameters(byte[] receiveKey, byte[] sendKey, byte[] receiveMac, byte[] sendMac)
        {
            ReceiveKey = receiveKey;
            SendKey = sendKey;
            ReceiveMac = receiveMac;
            SendMac = sendMac;
        }

        public uint GetNextSequenceId() =>
            unchecked(Interlocked.Increment(ref _lastSequenceId));
    }
}
