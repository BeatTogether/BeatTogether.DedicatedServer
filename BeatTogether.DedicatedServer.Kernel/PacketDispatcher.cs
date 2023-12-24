using System;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Configuration;
using BeatTogether.LiteNetLib.Dispatchers;
using BeatTogether.LiteNetLib.Enums;
using BeatTogether.LiteNetLib.Util;
using Serilog;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Messaging.Packets.Legacy;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class PacketDispatcher : ConnectedMessageDispatcher, IPacketDispatcher
    {
        public const byte LocalConnectionId = 0;
        public const byte ServerId = 0;
        public const byte AllConnectionIds = 127;

        private readonly IPacketRegistry _packetRegistry;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly DedicatedInstance _dedicatedInstance;
        private readonly ILogger _logger = Log.ForContext<PacketDispatcher>();

        public PacketDispatcher(
            IPacketRegistry packetRegistry,
            IPlayerRegistry playerRegistry,
            LiteNetConfiguration configuration,
            DedicatedInstance dedicatedInstance)
            : base (
                  configuration,
                  dedicatedInstance)
        {
            _packetRegistry = packetRegistry;
            _playerRegistry = playerRegistry;
            _dedicatedInstance = dedicatedInstance;
        }

        private void SendInternal(IPlayer player, ref SpanBuffer writer, DeliveryMethod deliveryMethod)
        {
            if (player.IsENetConnection)
            {
                // ENet send
                _logger.Verbose($"Sending packet (SenderId={ServerId}) to player {player.ConnectionId} with UserId {player.UserId} via ENet");
                _dedicatedInstance.ENetServer.Send(player.ENetPeerId!.Value, writer.Data, deliveryMethod);
                return;
            }
            
            // LiteNet send
            _logger.Verbose($"Sending packet (SenderId={ServerId}) to player {player.ConnectionId} with UserId {player.UserId} via LiteNet");
            Send(player.Endpoint, writer.Data, deliveryMethod);
        }

        #region Sends
        public void SendToNearbyPlayers(INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={ServerId})"
            );

            var writer = new SpanBuffer(stackalloc byte[412]);
            var legacyWriter = new SpanBuffer(stackalloc byte[412]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            legacyWriter.WriteLegacyRoutingHeader(ServerId, LocalConnectionId);
            WriteOne(ref writer, packet, ClientVersions.NewPacketVersion);
            WriteOne(ref legacyWriter, packet, ClientVersions.DefaultVersion);
            _logger.Verbose("Packet: " + packet.GetType().Name + " Was entered into the spanbuffer correctly, now sending once to each player");
            foreach (var player in _playerRegistry.Players)
                if (ClientVersions.ParseGameVersion(player.ClientVersion) < ClientVersions.NewPacketVersion)
                    SendInternal(player, ref legacyWriter, deliveryMethod);
                else
                    SendInternal(player, ref writer, deliveryMethod);
        }
        public void SendToNearbyPlayers(INetSerializable[] packets, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(SenderId={ServerId})"
            );

            var writer = new SpanBuffer(stackalloc byte[1024]);
            var legacyWriter = new SpanBuffer(stackalloc byte[1024]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            legacyWriter.WriteLegacyRoutingHeader(ServerId, LocalConnectionId);
            WriteMany(ref writer, packets, ClientVersions.NewPacketVersion);
            WriteMany(ref legacyWriter, packets, ClientVersions.DefaultVersion);
            _logger.Verbose("Packets were entered into the spanbuffer correctly, now sending once to each player");
            foreach (var player in _playerRegistry.Players)
                if (ClientVersions.ParseGameVersion(player.ClientVersion) < ClientVersions.NewPacketVersion)
                    SendInternal(player, ref legacyWriter, deliveryMethod);
                else
                    SendInternal(player, ref writer, deliveryMethod);
        }

        public void SendExcludingPlayer(IPlayer excludedPlayer, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(ExcludedId={excludedPlayer.ConnectionId})"
            );

            var writer = new SpanBuffer(stackalloc byte[412]);
            var legacyWriter = new SpanBuffer(stackalloc byte[412]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            legacyWriter.WriteLegacyRoutingHeader(ServerId, LocalConnectionId);
            WriteOne(ref writer, packet, ClientVersions.NewPacketVersion);
            WriteOne(ref legacyWriter, packet, ClientVersions.DefaultVersion);

            foreach (IPlayer player in _playerRegistry.Players)
                if (player.ConnectionId != excludedPlayer.ConnectionId)
                    if (ClientVersions.ParseGameVersion(player.ClientVersion) < ClientVersions.NewPacketVersion)
                        SendInternal(player, ref legacyWriter, deliveryMethod);
                    else
                        SendInternal(player, ref writer, deliveryMethod);
        }

        public void SendExcludingPlayer(IPlayer excludedPlayer, INetSerializable[] packets, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(ExcludedId={excludedPlayer.ConnectionId})"
            );
            if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Verbose))
                for (int i = 0; i < packets.Length; i++)
                {
                    _logger.Verbose(
                        $"Packet {i} is of type '{packets[i].GetType().Name}' "
                    );
                }

            var writer = new SpanBuffer(stackalloc byte[1024]);
            var legacyWriter = new SpanBuffer(stackalloc byte[1024]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            legacyWriter.WriteLegacyRoutingHeader(ServerId, LocalConnectionId);
            WriteMany(ref writer, packets, ClientVersions.NewPacketVersion);
            WriteMany(ref legacyWriter, packets, ClientVersions.DefaultVersion);

            foreach (IPlayer player in _playerRegistry.Players)
                if (player.ConnectionId != excludedPlayer.ConnectionId)
                    if (ClientVersions.ParseGameVersion(player.ClientVersion) < ClientVersions.NewPacketVersion)
                        SendInternal(player, ref legacyWriter, deliveryMethod);
                    else
                        SendInternal(player, ref writer, deliveryMethod);
        }

        public void RouteExcludingPlayer(IPlayer excludedPlayer, ref SpanBuffer writer, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending routed packet " +
                $"(ExcludedId={excludedPlayer.ConnectionId})"
            );

            foreach (IPlayer player in _playerRegistry.Players)
                if (player.ConnectionId != excludedPlayer.ConnectionId)
                    SendInternal(player, ref writer, deliveryMethod);
        }


        public void SendFromPlayer(IPlayer fromPlayer, INetSerializable packet, DeliveryMethod deliveryMethod)
		{
			_logger.Debug(
			    $"Sending packet of type '{packet.GetType().Name}' " + 
                $"(SenderId={fromPlayer.ConnectionId})"
            );

            var writer = new SpanBuffer(stackalloc byte[412]);
            var legacyWriter = new SpanBuffer(stackalloc byte[412]);
            writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            legacyWriter.WriteLegacyRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            WriteOne(ref writer, packet, ClientVersions.NewPacketVersion);
            WriteOne(ref legacyWriter, packet, ClientVersions.DefaultVersion);

            foreach (var player in _playerRegistry.Players)
                if (ClientVersions.ParseGameVersion(player.ClientVersion) < ClientVersions.NewPacketVersion)
                    SendInternal(player, ref legacyWriter, deliveryMethod);
                else
                    SendInternal(player, ref writer, deliveryMethod);
        }
        public void SendFromPlayer(IPlayer fromPlayer, INetSerializable[] packets, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(SenderId={fromPlayer.ConnectionId})"
            );

            var writer = new SpanBuffer(stackalloc byte[1024]);
            var legacyWriter = new SpanBuffer(stackalloc byte[1024]);
            writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            legacyWriter.WriteLegacyRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            WriteMany(ref writer, packets, ClientVersions.NewPacketVersion);
            WriteMany(ref legacyWriter, packets, ClientVersions.DefaultVersion);

            foreach (var player in _playerRegistry.Players)
                if (ClientVersions.ParseGameVersion(player.ClientVersion) < ClientVersions.NewPacketVersion)
                    SendInternal(player, ref legacyWriter, deliveryMethod);
                else
                    SendInternal(player, ref writer, deliveryMethod);
        }

        public void SendFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={fromPlayer.ConnectionId}, ReceiverId={LocalConnectionId})."
            );

            var writer = new SpanBuffer(stackalloc byte[412]);
            Version clientVersion = ClientVersions.ParseGameVersion(toPlayer.ClientVersion);
            if (clientVersion < ClientVersions.NewPacketVersion)
                writer.WriteLegacyRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            else
                writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            WriteOne(ref writer, packet, clientVersion);
            SendInternal(toPlayer, ref writer, deliveryMethod);
        }

        public void SendFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable[] packets, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket" +
                $"(SenderId={fromPlayer.ConnectionId}, ReceiverId={LocalConnectionId})."
            );

            var writer = new SpanBuffer(stackalloc byte[1024]);
            Version clientVersion = ClientVersions.ParseGameVersion(toPlayer.ClientVersion);
            if (clientVersion < ClientVersions.NewPacketVersion)
                writer.WriteLegacyRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            else
                writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);

            WriteMany(ref writer, packets, clientVersion);
            SendInternal(toPlayer, ref writer, deliveryMethod);
        }

        public void RouteFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, ref SpanBuffer writer, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending routed packet " +
                $"(SenderId={fromPlayer.ConnectionId}, ReceiverId={LocalConnectionId})."
            );

            SendInternal(toPlayer, ref writer, deliveryMethod);
        }

        public void SendToPlayer(IPlayer player, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={ServerId}, ReceiverId={LocalConnectionId})."
            );

            var writer = new SpanBuffer(stackalloc byte[412]);
            Version clientVersion = ClientVersions.ParseGameVersion(player.ClientVersion);
            if (clientVersion < ClientVersions.NewPacketVersion)
                writer.WriteLegacyRoutingHeader(ServerId, LocalConnectionId);
            else
                writer.WriteRoutingHeader(ServerId, LocalConnectionId);

            WriteOne(ref writer, packet, clientVersion);
            SendInternal(player, ref writer, deliveryMethod);
        }
        public void SendToPlayer(IPlayer player, INetSerializable[] packets, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(SenderId={ServerId}, ReceiverId={LocalConnectionId})."
            );

            var writer = new SpanBuffer(stackalloc byte[1024]);
            Version clientVersion = ClientVersions.ParseGameVersion(player.ClientVersion);
            if (clientVersion < ClientVersions.NewPacketVersion)
                writer.WriteLegacyRoutingHeader(ServerId, LocalConnectionId);
            else
                writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteMany(ref writer, packets, clientVersion);
            Send(player.Endpoint, writer.Data, deliveryMethod);
        }
        #endregion

        #region AwaitableSends
        //Sends with a task that completes when the packet/s have been rec
        public Task SendToNearbyPlayersAndAwait(INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={ServerId})"
            );

            var writer = new SpanBuffer(stackalloc byte[412]);
            var legacyWriter = new SpanBuffer(stackalloc byte[412]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            legacyWriter.WriteLegacyRoutingHeader(ServerId, LocalConnectionId);
            WriteOne(ref writer, packet, ClientVersions.NewPacketVersion);
            WriteOne(ref legacyWriter, packet, ClientVersions.DefaultVersion);
            _logger.Verbose("Packet: " + packet.GetType().Name + " Was entered into the spanbuffer correctly, now sending once to each player");
            var players = _playerRegistry.Players;
            Task[] tasks = new Task[players.Length];
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].IsENetConnection)
                {
                    tasks[i] = Task.CompletedTask;
                    if (ClientVersions.ParseGameVersion(players[i].ClientVersion) < ClientVersions.NewPacketVersion)
                        SendInternal(players[i], ref legacyWriter, deliveryMethod);
                    else
                        SendInternal(players[i], ref writer, deliveryMethod);
                    continue;
                }
                if (ClientVersions.ParseGameVersion(players[i].ClientVersion) < ClientVersions.NewPacketVersion)
                    tasks[i] = Send(players[i].Endpoint, legacyWriter.Data, deliveryMethod);
                else
                    tasks[i] = Send(players[i].Endpoint, writer.Data, deliveryMethod);
            }
            return Task.WhenAll(tasks);
        }
        public Task SendToNearbyPlayersAndAwait(INetSerializable[] packets, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(SenderId={ServerId})"
            );

            var writer = new SpanBuffer(stackalloc byte[1024]);
            var legacyWriter = new SpanBuffer(stackalloc byte[1024]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            legacyWriter.WriteLegacyRoutingHeader(ServerId, LocalConnectionId);
            WriteMany(ref writer, packets, ClientVersions.NewPacketVersion);
            WriteMany(ref legacyWriter, packets, ClientVersions.DefaultVersion);
            _logger.Verbose("Packets were entered into the spanbuffer correctly, now sending once to each player");
            var players = _playerRegistry.Players;
            Task[] tasks = new Task[players.Length];
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].IsENetConnection)
                {
                    tasks[i] = Task.CompletedTask;
                    if (ClientVersions.ParseGameVersion(players[i].ClientVersion) < ClientVersions.NewPacketVersion)
                        SendInternal(players[i], ref legacyWriter, deliveryMethod);
                    else
                        SendInternal(players[i], ref writer, deliveryMethod);
                    continue;
                }
                if (ClientVersions.ParseGameVersion(players[i].ClientVersion) < ClientVersions.NewPacketVersion)
                    tasks[i] = Send(players[i].Endpoint, legacyWriter.Data, deliveryMethod);
                else
                    tasks[i] = Send(players[i].Endpoint, writer.Data, deliveryMethod);
            }
            return Task.WhenAll(tasks);
        }

        public Task SendExcludingPlayerAndAwait(IPlayer excludedPlayer, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(ExcludedId={excludedPlayer.ConnectionId})"
            );

            var writer = new SpanBuffer(stackalloc byte[412]);
            var legacyWriter = new SpanBuffer(stackalloc byte[412]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            legacyWriter.WriteLegacyRoutingHeader(ServerId, LocalConnectionId);
            WriteOne(ref writer, packet, ClientVersions.NewPacketVersion);
            WriteOne(ref legacyWriter, packet, ClientVersions.DefaultVersion);

            var players = _playerRegistry.Players;
            Task[] tasks = new Task[players.Length - 1];
            int i = 0;
            foreach (var player in players)
                if (player.ConnectionId != excludedPlayer.ConnectionId)
                {
                    if (!player.IsENetConnection)
                    {
                        if (ClientVersions.ParseGameVersion(player.ClientVersion) < ClientVersions.NewPacketVersion)
                            tasks[i] = Send(player.Endpoint, legacyWriter.Data, deliveryMethod);
                        else
                            tasks[i] = Send(player.Endpoint, writer.Data, deliveryMethod);
                    }
                    else
                    {
                        tasks[i] = Task.CompletedTask;
                        if (ClientVersions.ParseGameVersion(player.ClientVersion) < ClientVersions.NewPacketVersion)
                            SendInternal(player, ref legacyWriter, deliveryMethod);
                        else
                            SendInternal(player, ref writer, deliveryMethod);
                    }
                    i++;
                }
            return Task.WhenAll(tasks);
        }
        public Task SendExcludingPlayerAndAwait(IPlayer excludedPlayer, INetSerializable[] packets, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(ExcludedId={excludedPlayer.ConnectionId})"
            );

            var writer = new SpanBuffer(stackalloc byte[1024]);
            var legacyWriter = new SpanBuffer(stackalloc byte[1024]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            legacyWriter.WriteLegacyRoutingHeader(ServerId, LocalConnectionId);
            WriteMany(ref writer, packets, ClientVersions.NewPacketVersion);
            WriteMany(ref legacyWriter, packets, ClientVersions.DefaultVersion);
            var players = _playerRegistry.Players;
            Task[] tasks = new Task[players.Length-1];
            int i = 0;
            foreach (var player in players)
                if (player.ConnectionId != excludedPlayer.ConnectionId)
                {
                    if (!player.IsENetConnection)
                    {
                        if (ClientVersions.ParseGameVersion(player.ClientVersion) < ClientVersions.NewPacketVersion)
                            tasks[i] = Send(player.Endpoint, legacyWriter.Data, deliveryMethod);
                        else
                            tasks[i] = Send(player.Endpoint, writer.Data, deliveryMethod);
                    }
                    else
                    {
                        tasks[i] = Task.CompletedTask;
                        if (ClientVersions.ParseGameVersion(player.ClientVersion) < ClientVersions.NewPacketVersion)
                            SendInternal(player, ref legacyWriter, deliveryMethod);
                        else
                            SendInternal(player, ref writer, deliveryMethod);
                    }
                    i++;
                }
            return Task.WhenAll(tasks);
        }

        public Task SendFromPlayerAndAwait(IPlayer fromPlayer, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={fromPlayer.ConnectionId})"
            );

            var writer = new SpanBuffer(stackalloc byte[412]);
            var legacyWriter = new SpanBuffer(stackalloc byte[412]);
            writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            legacyWriter.WriteLegacyRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            WriteOne(ref writer, packet, ClientVersions.NewPacketVersion);
            WriteOne(ref legacyWriter, packet, ClientVersions.DefaultVersion);
            var players = _playerRegistry.Players;
            Task[] tasks = new Task[players.Length];
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].IsENetConnection)
                {
                    tasks[i] = Task.CompletedTask;
                    if (ClientVersions.ParseGameVersion(players[i].ClientVersion) < ClientVersions.NewPacketVersion)
                        SendInternal(players[i], ref legacyWriter, deliveryMethod);
                    else
                        SendInternal(players[i], ref writer, deliveryMethod);
                    continue;
                }
                if (ClientVersions.ParseGameVersion(players[i].ClientVersion) < ClientVersions.NewPacketVersion)
                    tasks[i] = Send(players[i].Endpoint, legacyWriter.Data, deliveryMethod);
                else
                    tasks[i] = Send(players[i].Endpoint, writer.Data, deliveryMethod);
            }
            return Task.WhenAll(tasks);
        }
        public Task SendFromPlayerAndAwait(IPlayer fromPlayer, INetSerializable[] packets, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(SenderId={fromPlayer.ConnectionId})"
            );

            var writer = new SpanBuffer(stackalloc byte[1024]);
            var legacyWriter = new SpanBuffer(stackalloc byte[1024]);
            writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            legacyWriter.WriteLegacyRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            // TODO: We need a better implementation here, this doesn't work well with multiple versions
            WriteMany(ref writer, packets, ClientVersions.NewPacketVersion);
            WriteMany(ref legacyWriter, packets, ClientVersions.DefaultVersion);

            //foreach (var player in _playerRegistry.Players)
            //    if (ClientVersions.ParseGameVersion(player.ClientVersion) < ClientVersions.NewPacketVersion)
            //        SendInternal(player, ref legacyWriter, deliveryMethod);
            //    else
            //        SendInternal(player, ref writer, deliveryMethod);
            var players = _playerRegistry.Players;
            Task[] tasks = new Task[players.Length];
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].IsENetConnection)
                {
                    tasks[i] = Task.CompletedTask;
                    if (ClientVersions.ParseGameVersion(players[i].ClientVersion) < ClientVersions.NewPacketVersion)
                        SendInternal(players[i], ref legacyWriter, deliveryMethod);
                    else
                        SendInternal(players[i], ref writer, deliveryMethod);
                    continue;
                }
                if (ClientVersions.ParseGameVersion(players[i].ClientVersion) < ClientVersions.NewPacketVersion)
                    tasks[i] = Send(players[i].Endpoint, legacyWriter.Data, deliveryMethod);
                else
                    tasks[i] = Send(players[i].Endpoint, writer.Data, deliveryMethod);
            }
            return Task.WhenAll(tasks);
        }

        public Task SendFromPlayerToPlayerAndAwait(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={fromPlayer.ConnectionId}, ReceiverId={LocalConnectionId})."
            );

            var writer = new SpanBuffer(stackalloc byte[412]);
            Version clientVersion = ClientVersions.ParseGameVersion(toPlayer.ClientVersion);
            if (clientVersion < ClientVersions.NewPacketVersion)
                writer.WriteLegacyRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            else
                writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            WriteOne(ref writer, packet, clientVersion);
            if (toPlayer.IsENetConnection)
            {
                SendInternal(toPlayer, ref writer, deliveryMethod);
                return Task.CompletedTask;
            }
            return Send(toPlayer.Endpoint, writer.Data, deliveryMethod);
        }
        public Task SendFromPlayerToPlayerAndAwait(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable[] packets, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket" +
                $"(SenderId={fromPlayer.ConnectionId}, ReceiverId={LocalConnectionId})."
            );

            var writer = new SpanBuffer(stackalloc byte[1024]);
            Version clientVersion = ClientVersions.ParseGameVersion(toPlayer.ClientVersion);
            if (clientVersion < ClientVersions.NewPacketVersion)
                writer.WriteLegacyRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            else
                writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            WriteMany(ref writer, packets, clientVersion);
            SendInternal(toPlayer, ref writer, deliveryMethod);
            return Send(toPlayer.Endpoint, writer.Data, deliveryMethod);
        }

        public Task SendToPlayerAndAwait(IPlayer player, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={ServerId}, ReceiverId={LocalConnectionId})."
            );

            var writer = new SpanBuffer(stackalloc byte[412]);
            Version clientVersion = ClientVersions.ParseGameVersion(player.ClientVersion);
            if (clientVersion < ClientVersions.NewPacketVersion)
                writer.WriteLegacyRoutingHeader(ServerId, LocalConnectionId);
            else
                writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteOne(ref writer, packet, clientVersion);
            if (player.IsENetConnection)
            {
                SendInternal(player, ref writer, deliveryMethod);
                return Task.CompletedTask;
            }
            return Send(player.Endpoint, writer.Data, deliveryMethod);
        }
        public Task SendToPlayerAndAwait(IPlayer player, INetSerializable[] packets, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(SenderId={ServerId}, ReceiverId={LocalConnectionId})."
            );

            var writer = new SpanBuffer(stackalloc byte[1024]);
            Version clientVersion = ClientVersions.ParseGameVersion(player.ClientVersion);
            if (clientVersion < ClientVersions.NewPacketVersion)
                writer.WriteLegacyRoutingHeader(ServerId, LocalConnectionId);
            else
                writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteMany(ref writer, packets, clientVersion);
            if (player.IsENetConnection)
            {
                SendInternal(player, ref writer, deliveryMethod);
                return Task.CompletedTask;
            }
            return Send(player.Endpoint, writer.Data, deliveryMethod);
        }
        #endregion

        #region Writers
        public void WriteOne(ref SpanBuffer writer, INetSerializable packet, Version version)
        {
            var type = packet.GetType();
            var packetWriter = new SpanBuffer(stackalloc byte[412]);

            if (_packetRegistry.TryGetPacketIds(type, out var packetIds))
            {
                foreach (byte packetId in packetIds)
                    packetWriter.WriteUInt8(packetId);
            }
            else
            {
                _logger.Verbose($"Writing MpCore Packet {type.Name}");
                packetWriter.WriteUInt8(7);
                packetWriter.WriteUInt8(100);
                packetWriter.WriteString(type.Name);
                //Presume it is a mpcore packet and use the mpcore packet ID, would thow an exeption here if not
                //throw new Exception($"Failed to retrieve identifier for packet of type '{type.Name}'");
            }
            if (packet is IVersionedNetSerializable)
            {
                _logger.Verbose($"Packet {type.Name} is a versioned packet, writing with version {version}");
                ((IVersionedNetSerializable)packet).WriteTo(ref packetWriter, version);
            }
            else
            {
                _logger.Verbose($"Packet {type.Name} is not a versioned packet, writing default version");
                packet.WriteTo(ref packetWriter);
            }
            writer.WriteVarUInt((uint)packetWriter.Size);
            writer.WriteBytes(packetWriter.Data.ToArray());
        }
        public void WriteMany(ref SpanBuffer writer, INetSerializable[] packets, Version version)
        {
            for (int i = 0; i < packets.Length; i++)
                WriteOne(ref writer, packets[i], version);
        }

        #endregion
    }
}
