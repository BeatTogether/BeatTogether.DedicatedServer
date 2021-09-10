using BinaryRecords;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Abstractions
{
    public interface IPacketReader
    {
        INetSerializable ReadFrom(NetDataReader reader);
    }
}
