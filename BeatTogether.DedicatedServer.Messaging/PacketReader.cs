using System.Runtime.Serialization;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
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

        public INetSerializable ReadFrom(NetDataReader reader)
        {
            var length = reader.GetVarUInt();
            if (reader.AvailableBytes < length)
                throw new InvalidDataContractException($"Packet fragmented (AvailableBytes={reader.AvailableBytes}, Expected={length}).");
            IPacketRegistry packetRegistry = _packetRegistry;
            while (true)
            {
                var packetId = reader.GetByte();
                if (packetRegistry.TryCreatePacket(packetId, out var packet))
                {
                    packet.Deserialize(reader);
                    return packet;
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
    }
}
