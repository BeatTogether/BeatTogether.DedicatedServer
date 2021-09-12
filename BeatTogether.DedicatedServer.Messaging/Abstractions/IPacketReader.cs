using BeatTogether.DedicatedServer.Messaging.Models;
using BinaryRecords;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Abstractions
{
	public interface IPacketReader
	{
		INetSerializable ReadDataFrom(NetDataReader reader, ProcessingPacketInfo packetInfo);
		ProcessingPacketInfo ReadFrom(NetDataReader reader);
		void SkipPacket(NetDataReader reader, ProcessingPacketInfo packetInfo);
	}
}
