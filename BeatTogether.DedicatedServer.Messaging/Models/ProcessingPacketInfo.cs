using BeatTogether.LiteNetLib.Abstractions;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public struct ProcessingPacketInfo
    {
        public uint length;
        public int startPosition;
        public INetSerializable packet;
    }
}
