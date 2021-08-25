using System;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.Extensions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging
{
    public sealed class PacketWriter : IPacketWriter
    {
        private readonly IPacketRegistry _packetRegistry;

        public PacketWriter(IPacketRegistry packetRegistry)
        {
            _packetRegistry = packetRegistry;
        }

        public void WriteTo(NetDataWriter writer, INetSerializable packet)
        {
            
            var type = packet.GetType();
            if (!_packetRegistry.TryGetPacketIds(type, out var packetIds))
                throw new Exception($"Failed to retrieve identifier for packet of type '{type.Name}'.");
            var packetWriter = new NetDataWriter();
            foreach (var packetId in packetIds)
                packetWriter.Put(packetId);
            packet.Serialize(packetWriter);
            writer.PutVarUInt((uint)packetWriter.Length);
            writer.Put(packetWriter.Data);
        }
    }
}
