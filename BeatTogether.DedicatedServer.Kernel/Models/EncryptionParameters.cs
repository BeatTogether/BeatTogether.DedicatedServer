using System.Security.Cryptography;
using System.Threading;

namespace BeatTogether.DedicatedServer.Kernel.Models
{
    public sealed class EncryptionParameters
    {
        public byte[] ReceiveKey { get; }
        public byte[] SendKey { get; }
        public HMAC ReceiveMac { get; }
        public HMAC SendMac { get; }

        private uint _lastSequenceId = 0U;

        public EncryptionParameters(byte[] receiveKey, byte[] sendKey, HMAC receiveMac, HMAC sendMac)
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
