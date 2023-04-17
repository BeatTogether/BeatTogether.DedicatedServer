using BeatTogether.LiteNetLib.Abstractions;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public struct ProcessingPacketInfo //TODO this isnt used?
    {
        public uint length;
        public int startPosition;
        public INetSerializable packet;
    }
}
