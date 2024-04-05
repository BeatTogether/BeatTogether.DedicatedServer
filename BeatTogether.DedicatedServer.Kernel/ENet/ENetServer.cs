using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Ignorance.ENet;
using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Ignorance.Util;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Messaging.Util;
using Krypton.Buffers;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.ENet
{
    /// <summary>
    /// Temporary secondary server/socket for ENet (1.31+) connectivity.
    /// </summary>
    public class ENetServer : IDisposable
    {
        private readonly int Port;
        private readonly IgnoranceServer _ignorance;
        private readonly ILogger _logger;
        private CancellationTokenSource _runtimeCts;
        private readonly Thread _workerThread;
        private readonly Dictionary<uint, ENetConnection> _connections;
        
        private byte[]? _receiveBuffer;

        public IPEndPoint EndPoint => new(IPAddress.Any, Port);
        public bool IsAlive => _ignorance.IsAlive;

        public ENetServer(int port)
        {
            Port = port;

            _ignorance = new IgnoranceServer();
            _logger = Log.ForContext<ENetServer>();
            _runtimeCts = new();
            _workerThread = new(ThreadWorker);
            _connections = new();
            
            EnsureReceiveBufferSize(_ignorance.IncomingOutgoingBufferSize);

            IgnoranceDebug.Logger = _logger;
        }

        #region Start / Stop

        public async Task Start()
        {
            await Stop();

            _runtimeCts = new();

            _logger.Information("Starting companion ENet socket (Endpoint={Endpoint})", EndPoint);

            _ignorance.BindPort = Port;
            _ignorance.BindAddress = IPAddress.Any.ToString();
            _ignorance.BindAllInterfaces = true;
            _ignorance.Start();
            
            _workerThread.Start();
        }

        public async Task Stop()
        {
            _runtimeCts.Cancel();
            _ignorance.Stop();

            while (_ignorance.IsAlive)
                await Task.Delay(1);
        }

        public void Dispose()
        {
            _runtimeCts.Cancel();
            _ignorance.Stop();
            
            ReturnReceiveBuffer();
        }
        
        #endregion

        private void ThreadWorker()
        {
            _connections.Clear();
            
            while (!_runtimeCts!.IsCancellationRequested)
            {
                HandleConnectionEvents();
                HandleDisconnectionEvents();
                HandleIncomingEvents();
                
                Thread.Sleep(1);
            }
        }

        #region Ignorance event queues

        private void HandleConnectionEvents()
        {
            var ctsToken = _runtimeCts!.Token;

            while (!ctsToken.IsCancellationRequested
                   && (_ignorance.ConnectionEvents?.TryDequeue(out var e) ?? false))
            {
                CreateConnection(e);
            }
        }

        private void HandleDisconnectionEvents()
        {
            var ctsToken = _runtimeCts!.Token;

            while (!ctsToken.IsCancellationRequested
                   && (_ignorance.DisconnectionEvents?.TryDequeue(out var e) ?? false))
            {
                if (_connections.TryGetValue(e.NativePeerId, out var connection))
                    connection.Disconnect(true);
            }
        }

        private void HandleIncomingEvents()
        {
            var ctsToken = _runtimeCts!.Token;

            while (!ctsToken.IsCancellationRequested
                   && (_ignorance.Incoming?.TryDequeue(out var e) ?? false))
            {
                if (!_connections.TryGetValue(e.NativePeerId, out var connection) || connection.State == ENetConnectionState.Disconnected)
                {
                    // Connection not established, ignore incoming
                    e.Payload.Dispose();
                    continue;
                }
                
                // Read incoming into buffer
                var receiveLength = e.Payload.Length;
                EnsureReceiveBufferSize(receiveLength);
                e.Payload.CopyTo(_receiveBuffer!, 0);
                e.Payload.Dispose();
                
                var bufferReader = new SpanBuffer(_receiveBuffer.AsSpan(..receiveLength));
                
                // If pending: first packet should be connection request only (IgnCon)
                if (connection.State == ENetConnectionState.Pending)
                {
                    if (!TryAcceptConnection(connection, ref bufferReader))
                        connection.Disconnect();
                    
                    return;
                }
                // Process actual payload
                var deliveryMethod = e.Channel == 0 ? IgnoranceChannelTypes.Reliable : IgnoranceChannelTypes.Unreliable;
                OnReceive(connection.EndPoint, ref bufferReader, deliveryMethod);
            }
        }

        #endregion

        #region Connections

        private ENetConnection CreateConnection(IgnoranceConnectionEvent e)
        {
            KickPeer(e.NativePeerId); // clean up any previous use of peer ID
            
            var connection = new ENetConnection(this, e.NativePeerId, e.IP, e.Port);
            _connections[e.NativePeerId] = connection;
            
            _logger.Verbose("Incoming ENet connection (PeerId={PeerId}, EndPoint={EndPoint})",
                connection.NativePeerId, connection.EndPoint);
            
            return connection;
        }

        private bool TryAcceptConnection(ENetConnection connection, ref SpanBuffer reader)
        {
            try
            {
                // First packet from pending connection: should be IgnCon request
                var requestPrefix = reader.ReadString();
                
                if (requestPrefix != "IgnCon")
                {
                    _logger.Warning("Invalid first ENet msg: bad prefix string");
                    return false;
                }

                // Continue with regular accept flow (remainder is regular GameLift connection request)
                var player = TryAcceptConnection(connection.EndPoint, ref reader);

                
                if (player != null)
                {
                    // Accept success
                    player.ENetPeerId = connection.NativePeerId;
                    connection.State = ENetConnectionState.Accepted;
                    OnConnect(connection.EndPoint);
                    return true;
                }
            }
            catch (EndOfBufferException)
            {
                // Invalid connection request - read/length error
                _logger.Warning("Invalid first ENet msg: length read error");
            }
            
            return false;
        }

        public void KickPeer(uint peerId, bool sendKick = true)
        {
            if (!_connections.TryGetValue(peerId, out var connection) || !_connections.Remove(peerId))
                // Connection not established
                return;
            
            _logger.Verbose("Closing ENet connection (PeerId={PeerId}, EndPoint={Port})",
                connection.NativePeerId, connection.EndPoint);

            connection.State = ENetConnectionState.Disconnected;
            OnDisconnect(connection.EndPoint);
            
            if (!IsAlive || !sendKick)
                // Can't or won't send kick
                return;
            
            _ignorance.Commands.Enqueue(new IgnoranceCommandPacket()
            {
                Type = IgnoranceCommandType.ServerKickPeer,
                PeerId = peerId
            });
        }

        public void KickAllPeers()
        {
            foreach (var connection in _connections.Values)
                KickPeer(connection.NativePeerId, true);
        }

        #endregion

        #region Send

        public void Send(IPlayer player, ReadOnlySpan<byte> message, IgnoranceChannelTypes deliveryMethod)
        {
            if (!player.ENetPeerId.HasValue)
                // Not an ENet peer
                return;
            
            Send(player.ENetPeerId.Value, message, deliveryMethod);
        }
        
        public void Send(uint peerId, ReadOnlySpan<byte> message, IgnoranceChannelTypes deliveryMethod)
        {
            if (!_connections.TryGetValue(peerId, out var connection))
                // Invalid peer
                return;
            
            Send(connection, message, deliveryMethod);
        }

        public void Send(ENetConnection connection, ReadOnlySpan<byte> message, IgnoranceChannelTypes deliveryMethod)
        {
            if (!IsAlive)
                return;
            
            if (connection.State != ENetConnectionState.Accepted)
                // Do not send if pending/disconnected
                return;

            var eNetPacket = default(Packet);
            eNetPacket.Create(message.ToArray(), message.Length/*, (PacketFlags)deliveryMethod*/);

            _ignorance.Outgoing.Enqueue(new IgnoranceOutgoingPacket()
            {
                Channel = (byte)(deliveryMethod == IgnoranceChannelTypes.Reliable ? 0 : 1), // 1 = Unreliable, 0 = reliable
                NativePeerId = connection.NativePeerId,
                Payload = eNetPacket
            });
        }

        #endregion

        #region Buffer

        private void EnsureReceiveBufferSize(int length)
        {
            if (length >= _ignorance.MaximumPacketSize)
                throw new ArgumentException("Receive buffer size should never exceed MaximumPacketSize");

            if (_receiveBuffer != null && _receiveBuffer.Length >= length)
                // Buffer does not need adjusting
                return;
                
            ReturnReceiveBuffer();
            _receiveBuffer = ArrayPool<byte>.Shared.Rent(length);
        }

        private void ReturnReceiveBuffer()
        {
            if (_receiveBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(_receiveBuffer);
                _receiveBuffer = null;
            }
        }

        #endregion



        #region Overrideables
        public virtual IPlayer? TryAcceptConnection(IPEndPoint endPoint, ref SpanBuffer reader)
        {
            return null;
        }

        public virtual void OnConnect(EndPoint endPoint)
        {
        }

        public virtual void OnDisconnect(EndPoint endPoint)
        {
        }

        public virtual void OnReceive(EndPoint remoteEndPoint, ref SpanBuffer reader, IgnoranceChannelTypes method)
        {
        }

        #endregion
    }
}