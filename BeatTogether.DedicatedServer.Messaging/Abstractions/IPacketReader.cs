using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Abstractions
{
	public interface IPacketReader
	{
		INetSerializable ReadDataFrom(ref SpanBufferReader reader, ProcessingPacketInfo packetInfo);
		ProcessingPacketInfo ReadFrom(ref SpanBufferReader reader);
		void SkipPacket(ref SpanBufferReader reader, ProcessingPacketInfo packetInfo);
	}
}
