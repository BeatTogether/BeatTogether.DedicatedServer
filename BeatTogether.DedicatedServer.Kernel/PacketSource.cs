using System;
using System.Text;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.Extensions;
using LiteNetLib;
using LiteNetLib.Utils;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class PacketSource : IPacketSource
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPacketReader _packetReader;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly ILogger _logger = Log.ForContext<PacketSource>();

        public PacketSource(
            IServiceProvider serviceProvider,
            IPacketReader packetReader,
            IPlayerRegistry playerRegistry)
        {
            _serviceProvider = serviceProvider;
            _packetReader = packetReader;
            _playerRegistry = playerRegistry;
        }

        public void Signal(NetPeer peer, NetDataReader reader, DeliveryMethod deliveryMethod)
        {
            if (!reader.TryGetRoutingHeader(out var routingHeader))
            {
                _logger.Warning(
                    "Failed to read routing header " +
                    $"(RemoteEndPoint='{peer.EndPoint}')."
                );
                return;
            }

            if (!_playerRegistry.TryGetPlayer(peer.EndPoint, out var sender))
            {
                _logger.Warning(
                    "Failed to retrieve sender " +
                    $"(RemoteEndPoint='{peer.EndPoint}')."
                );
                return;
            }

            // Is this packet meant to be routed?
            if (routingHeader.ReceiverId != 0)
            {
                RoutePacket(sender, routingHeader, reader, deliveryMethod);
            }

            while (!reader.EndOfData)
            {
                try
                {
                    var packet = _packetReader.ReadFrom(reader);
                    var packetType = packet.GetType();
                    var packetHandlerType = typeof(IPacketHandler<>)
                        .MakeGenericType(packetType);
                    var packetHandler = _serviceProvider.GetService(packetHandlerType);
                    if (packetHandler is null)
                    {
                        _logger.Warning($"No handler exists for packet of type '{packetType.Name}'.");
                        continue;
                    }

                    Task.Run(async () =>
                    {
                        try
                        {
                            await ((IPacketHandler)packetHandler).Handle(sender, packet);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, $"Failed to handle packet of type '{packetType.Name}'.");
                        }
                    });
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Failed to read packet.");

                    var builder = new StringBuilder("new byte[] { ");
                    foreach (var b in reader.RawData)
                    {
                        builder.Append(b + ", ");
                    }
                    builder.Append("}");

                    _logger.Debug(builder.ToString());
                    //return;
                }
            }
        }

        #region Private Methods

        private void RoutePacket(IPlayer sender,
            (byte SenderId, byte ReceiverId) routingHeader,
            NetDataReader reader, DeliveryMethod deliveryMethod)
        {
            routingHeader.SenderId = sender.ConnectionId;
            var writer = new NetDataWriter();
            writer.PutRoutingHeader(routingHeader.SenderId, routingHeader.ReceiverId);
            writer.Put(reader.RawData, reader.Position, reader.AvailableBytes);
            if (routingHeader.ReceiverId == 127)
            {
                _logger.Verbose(
                    $"Routing packet from {routingHeader.SenderId} -> all players " +
                    $"(Secret='{sender.Secret}', DeliveryMethod={deliveryMethod})."
                );
                sender.NetPeer.NetManager.SendToAll(writer, deliveryMethod, sender.NetPeer);
            }
            else
            {
                if (!_playerRegistry.TryGetPlayer(routingHeader.ReceiverId, out var receiver))
                {
                    _logger.Warning(
                        "Failed to retrieve receiver " +
                        $"(Secret='{sender.Secret}', ReceiverId={routingHeader.ReceiverId})."
                    );
                    return;
                }
                //_logger.Verbose(
                //    $"Routing packet from {routingHeader.SenderId} -> {routingHeader.ReceiverId} " +
                //    $"(Secret='{sender.Secret}', DeliveryMethod={deliveryMethod})."
                //);
                receiver.NetPeer.Send(writer, deliveryMethod);
            }
        }

        #endregion
    }
}
