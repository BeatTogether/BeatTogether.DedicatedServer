using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Configuration;
using BeatTogether.LiteNetLib.Dispatchers;
using BeatTogether.LiteNetLib.Enums;
using Krypton.Buffers;
using Serilog;
using System;
using System.Net;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class PacketDispatcher : ConnectedMessageDispatcher, IPacketDispatcher
    {
        public const byte LocalConnectionId = 0;
        public const byte ServerId = 0;
        public const byte AllConnectionIds = 127;

        private readonly IPacketRegistry _packetRegistry;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly ILogger _logger = Log.ForContext<PacketDispatcher>();

        public PacketDispatcher(
            IPacketRegistry packetRegistry,
            IPlayerRegistry playerRegistry,
            LiteNetConfiguration configuration,
            LiteNetServer server)
            : base (
                  configuration,
                  server)
        {
            _packetRegistry = packetRegistry;
            _playerRegistry = playerRegistry;
        }

        public void SendToNearbyPlayers(INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={ServerId})"
            );

            var writer = new SpanBufferWriter(stackalloc byte[412]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteOne(ref writer, packet);
            _logger.Verbose("Packet: " + packet.GetType().Name + " Was entered into the spanbuffer correctly, now sending once to each player");
            foreach (var player in _playerRegistry.Players)
                    Send(player.Endpoint, writer.Data, deliveryMethod);
        }
        public void SendToNearbyPlayers(INetSerializable[] packets, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(SenderId={ServerId})"
            );

            var writer = new SpanBufferWriter(stackalloc byte[412]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteMany(ref writer, packets);
            _logger.Verbose("Packets were entered into the spanbuffer correctly, now sending once to each player");
            foreach (var player in _playerRegistry.Players)
                Send(player.Endpoint, writer.Data, deliveryMethod);
        }

        public void SendExcludingPlayer(IPlayer excludedPlayer, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(ExcludedId={excludedPlayer.ConnectionId})"
            );

            var writer = new SpanBufferWriter(stackalloc byte[412]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteOne(ref writer, packet);

            foreach (IPlayer player in _playerRegistry.Players)
                if (player.ConnectionId != excludedPlayer.ConnectionId)
                    Send(player.Endpoint, writer.Data, deliveryMethod);
        }
        public void SendExcludingPlayer(IPlayer excludedPlayer, INetSerializable[] packets, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(ExcludedId={excludedPlayer.ConnectionId})"
            );

            var writer = new SpanBufferWriter(stackalloc byte[412]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteMany(ref writer, packets);

            foreach (IPlayer player in _playerRegistry.Players)
                if (player.ConnectionId != excludedPlayer.ConnectionId)
                    Send(player.Endpoint, writer.Data, deliveryMethod);
        }

        public void SendToEndpoint(EndPoint endpoint, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(To endpoint ={endpoint})"
            );

            var writer = new SpanBufferWriter(stackalloc byte[412]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteOne(ref writer, packet);

            Send(endpoint, writer.Data, deliveryMethod);
        }
        public void SendToEndpoint(EndPoint endpoint, INetSerializable[] packets, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(To endpoint ={endpoint})"
            );

            var writer = new SpanBufferWriter(stackalloc byte[412]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteMany(ref writer, packets);

            Send(endpoint, writer.Data, deliveryMethod);
        }

        public void SendFromPlayer(IPlayer fromPlayer, INetSerializable packet, DeliveryMethod deliveryMethod)
		{
			_logger.Debug(
			    $"Sending packet of type '{packet.GetType().Name}' " + 
                $"(SenderId={fromPlayer.ConnectionId})"
            );

            var writer = new SpanBufferWriter(stackalloc byte[412]);
            writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            WriteOne(ref writer, packet);

            foreach (var player in _playerRegistry.Players)
                Send(player.Endpoint, writer.Data, deliveryMethod);
		}
        public void SendFromPlayer(IPlayer fromPlayer, INetSerializable[] packets, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(SenderId={fromPlayer.ConnectionId})"
            );

            var writer = new SpanBufferWriter(stackalloc byte[412]);
            writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            WriteMany(ref writer, packets);

            foreach (var player in _playerRegistry.Players)
                Send(player.Endpoint, writer.Data, deliveryMethod);
        }

        public void SendFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={fromPlayer.ConnectionId}, ReceiverId={LocalConnectionId})."
            );

            var writer = new SpanBufferWriter(stackalloc byte[412]);
            writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            WriteOne(ref writer, packet);
            Send(toPlayer.Endpoint, writer.Data, deliveryMethod);
        }

        public void SendFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable[] packets, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket" +
                $"(SenderId={fromPlayer.ConnectionId}, ReceiverId={LocalConnectionId})."
            );

            var writer = new SpanBufferWriter(stackalloc byte[412]);
            writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            WriteMany(ref writer, packets);
            Send(toPlayer.Endpoint, writer.Data, deliveryMethod);
        }

        public void SendToPlayer(IPlayer player, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={ServerId}, ReceiverId={LocalConnectionId})."
            );

            var writer = new SpanBufferWriter(stackalloc byte[412]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteOne(ref writer, packet);
            Send(player.Endpoint, writer, deliveryMethod);
        }

        public void SendToPlayer(IPlayer player, INetSerializable[] packets, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(SenderId={ServerId}, ReceiverId={LocalConnectionId})."
            );

            var writer = new SpanBufferWriter(stackalloc byte[412]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteMany(ref writer, packets);
            Send(player.Endpoint, writer, deliveryMethod);
        }

        public void WriteOne(ref SpanBufferWriter writer, INetSerializable packet)
        {
            var type = packet.GetType();
            if (!_packetRegistry.TryGetPacketIds(type, out var packetIds))
                throw new Exception($"Failed to retrieve identifier for packet of type '{type.Name}'");
            var packetWriter = new SpanBufferWriter(stackalloc byte[412]);
            foreach (byte packetId in packetIds)
                packetWriter.WriteUInt8(packetId);
            packet.WriteTo(ref packetWriter);
            writer.WriteVarUInt((uint)packetWriter.Size);
            writer.WriteBytes(packetWriter.Data.ToArray());
        }

        public void WriteMany(ref SpanBufferWriter writer, INetSerializable[] packets)
        {
            for (int i = 0; i < packets.Length; i++)
                WriteOne(ref writer, packets[i]);
        }
    }
}
