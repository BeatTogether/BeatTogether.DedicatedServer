using System.Runtime.Serialization;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.Extensions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging
{
    public sealed class PacketReader : IPacketReader
    {
        private readonly IPacketRegistry _packetRegistry;

        public PacketReader(IPacketRegistry packetRegistry)
        {
            _packetRegistry = packetRegistry;
        }

        public ProcessingPacketInfo ReadFrom(NetDataReader reader)
        {
            var length = reader.GetVarUInt();
            if (reader.AvailableBytes < length)
                throw new InvalidDataContractException($"Packet fragmented (AvailableBytes={reader.AvailableBytes}, Expected={length}).");

            var prevPosition = reader.Position;

            try
            {
                IPacketRegistry packetRegistry = _packetRegistry;
                while (true)
                {
                    var packetId = reader.GetByte();
                    if (packetRegistry.TryCreatePacket(packetId, out var packet))
                    {
                        return new ProcessingPacketInfo
                        {
                            length = length,
                            startPosition = prevPosition,
                            packet = packet
                        };
                    }
                    if (packetRegistry.TryGetSubPacketRegistry(packetId, out var subPacketRegistry))
                    {
                        packetRegistry = subPacketRegistry;
                        continue;
                    }

                    throw new InvalidDataContractException(
                        $"Packet identifier not registered with '{packetRegistry.GetType().Name}' " +
                        $"(PacketId={packetId})."
                    );
                }
            }
            catch
            {
                // skip any unprocessed bytes (or rewind the reader if too many bytes were read)
                var processedBytes = reader.Position - prevPosition;
                reader.SkipBytes((int)length - processedBytes);
                throw;
            }
        }

        public INetSerializable ReadDataFrom(NetDataReader reader, ProcessingPacketInfo packetInfo)
		{
            try
			{
                packetInfo.packet.Deserialize(reader);
                return packetInfo.packet;
			}
            catch
			{
                // skip any unprocess bytes (or rewind the reader if too many bytes were read)
                var processedBytes = reader.Position - packetInfo.startPosition;
                reader.SkipBytes((int)packetInfo.length - processedBytes);
                throw;
			}
		}

        public void SkipPacket(NetDataReader reader, ProcessingPacketInfo packetInfo)
		{
            // skip any unprocess bytes (or rewind the reader if too many bytes were read)
            var processedBytes = reader.Position - packetInfo.startPosition;
            reader.SkipBytes((int)packetInfo.length - processedBytes);
        }
    }
}
