using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Configuration;
using BeatTogether.LiteNetLib.Enums;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Sources;
using Krypton.Buffers;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class PacketSource : ConnectedMessageSource
    {
        public const byte LocalConnectionId = 0;
        public const byte AllConnectionIds = 127;

        private readonly IServiceProvider _serviceProvider;
        private readonly IPacketRegistry _packetRegistry;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly PacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<PacketSource>();

        public PacketSource(
            IServiceProvider serviceProvider,
            IPacketRegistry packetRegistry,
            IPlayerRegistry playerRegistry,
            PacketDispatcher packetDispatcher,
            LiteNetConfiguration configuration,
            LiteNetServer server)
            : base (
                  configuration,
                  server)
        {
            _serviceProvider = serviceProvider;
            _packetRegistry = packetRegistry;
            _playerRegistry = playerRegistry;
            _packetDispatcher = packetDispatcher;
        }

        public override void OnReceive(EndPoint remoteEndPoint, ref SpanBufferReader reader, DeliveryMethod method)
        {
            if (!reader.TryReadRoutingHeader(out var routingHeader))
            {
                _logger.Warning(
                    "Failed to read routing header " +
                    $"(RemoteEndPoint='{remoteEndPoint}')."
                );
                return;
            }

            if (!_playerRegistry.TryGetPlayer(remoteEndPoint, out var sender))
            {
                _logger.Warning(
                    "Failed to retrieve sender, They are not in this instance" +
                    $"(RemoteEndPoint='{remoteEndPoint}')."
                );
                return;
            }

            // Is this packet meant to be routed?
            if (routingHeader.ReceiverId != 0)
                RoutePacket(sender, routingHeader, ref reader, method);

            while (reader.RemainingSize > 0)
            {
                var length = reader.ReadVarUInt();
                if (reader.RemainingSize < length)
                {
                    _logger.Warning($"Packet fragmented (RemainingSize={reader.RemainingSize}, Expected={length}).");
                    return;
                }

                var prevPosition = reader.Offset;
                INetSerializable? packet;
                var packetRegistry = _packetRegistry;
                while (true)
                {
                    var packetId = reader.ReadByte();
                    if (packetRegistry.TryCreatePacket(packetId, out packet))
                        break;
                    if (packetRegistry.TryGetSubPacketRegistry(packetId, out var subPacketRegistry))
                    {
                        packetRegistry = subPacketRegistry;
                        continue;
                    }
                    break;
                }

                if (packet == null)
                {
                    // skip any unprocessed bytes
                    var processedBytes = reader.Offset - prevPosition;
                    reader.SkipBytes((int)length - processedBytes);
                    continue;
                }

                var packetType = packet.GetType();
                var packetHandlerType = typeof(Abstractions.IPacketHandler<>)
                    .MakeGenericType(packetType);
                var packetHandler = _serviceProvider.GetService(packetHandlerType);
                if (packetHandler is null)
                {
                    _logger.Verbose($"No handler exists for packet of type '{packetType.Name}'.");

                    // skip any unprocessed bytes
                    var processedBytes = reader.Offset - prevPosition;
                    reader.SkipBytes((int)length - processedBytes);
                    continue;
                }

                try
                {
                    packet.ReadFrom(ref reader);
                }
                catch
                {
                    // skip any unprocessed bytes
                    var processedBytes = reader.Offset - prevPosition;
                    reader.SkipBytes((int)length - processedBytes);
                    continue;
                }

                ((Abstractions.IPacketHandler)packetHandler).Handle(sender, packet);
            }
        }

        #region Private Methods

        private void RoutePacket(IPlayer sender,
            (byte SenderId, byte ReceiverId) routingHeader,
            ref SpanBufferReader reader, DeliveryMethod deliveryMethod)
        {
            routingHeader.SenderId = sender.ConnectionId;
            var writer = new SpanBufferWriter(stackalloc byte[412]);
            if (routingHeader.ReceiverId == AllConnectionIds)
            {
                writer.WriteRoutingHeader(routingHeader.SenderId, routingHeader.ReceiverId);
                writer.WriteBytes(reader.RemainingData);

                _logger.Verbose(
                    $"Routing packet from {routingHeader.SenderId} -> all players " +
                    $"(Secret='{sender.Secret}', DeliveryMethod={deliveryMethod})."
                );
                foreach (var player in _playerRegistry.Players)
                    if (player != sender)
                        _packetDispatcher.Send(player.Endpoint, writer, deliveryMethod);
            }
            else
            {
                writer.WriteRoutingHeader(routingHeader.SenderId, LocalConnectionId);
                writer.WriteBytes(reader.RemainingData);

                if (!_playerRegistry.TryGetPlayer(routingHeader.ReceiverId, out var receiver))
                {
                    _logger.Warning(
                        "Failed to retrieve receiver " +
                        $"(Secret='{sender.Secret}', ReceiverId={routingHeader.ReceiverId})."
                    );
                    return;
                }
                _logger.Verbose(
                    $"Routing packet from {routingHeader.SenderId} -> {routingHeader.ReceiverId} " +
                    $"(Secret='{sender.Secret}', DeliveryMethod={deliveryMethod})."
                );
                _packetDispatcher.Send(receiver.Endpoint, writer, deliveryMethod);
            }
        }

        #endregion
    }
}
