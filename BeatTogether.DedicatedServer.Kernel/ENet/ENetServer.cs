using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Ignorance.Util;
using BeatTogether.LiteNetLib.Extensions;
using Krypton.Buffers;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.ENet
{
    /// <summary>
    /// Temporary secondary server/socket for ENet (1.31+) connectivity.
    /// </summary>
    public class ENetServer : IDisposable
    {
        public readonly DedicatedInstance DedicatedInstance;
        public readonly int Port;

        private readonly IgnoranceServer _ignorance;
        private readonly ILogger _logger;
        private CancellationTokenSource _runtimeCts;
        private readonly Thread _workerThread;
        private readonly Dictionary<uint, ENetConnection> _connections;
        private readonly byte[] _receiveBufferMemory;

        public IPEndPoint EndPoint => new(IPAddress.Any, Port);
        public bool IsAlive => _ignorance.IsAlive;

        public ENetServer(DedicatedInstance dedicatedInstance, int port)
        {
            DedicatedInstance = dedicatedInstance;
            Port = port;

            _ignorance = new IgnoranceServer();
            _logger = Log.ForContext<ENetServer>();
            _runtimeCts = new();
            _workerThread = new(ThreadWorker);
            _connections = new();
            _receiveBufferMemory = GC.AllocateArray<byte>(length: _ignorance.IncomingOutgoingBufferSize, pinned: true);

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
                e.Payload.CopyTo(_receiveBufferMemory, 0);
                e.Payload.Dispose();
                
                var bufferReader = new SpanBufferReader(_receiveBufferMemory.AsSpan(..receiveLength));
                
                // If pending, try to accept connection (IgnCon request)
                if (connection.State == ENetConnectionState.Pending)
                {
                    if (!TryAcceptConnection(connection, ref bufferReader))
                    {
                        connection.Disconnect();
                        continue;
                    }
                }
                
                // Read actual payload...
                // TODO
            }
        }

        #endregion

        #region Connections

        private ENetConnection CreateConnection(IgnoranceConnectionEvent e)
        {
            KickPeer(e.NativePeerId); // clean up any previous use of peer ID
            
            var connection = new ENetConnection(this, e.NativePeerId, e.IP, e.Port);
            _connections[e.NativePeerId] = connection;
            
            _logger.Information("Incoming ENet connection (PeerId={PeerId}, EndPoint={EndPoint})",
                connection.NativePeerId, connection.EndPoint);
            
            return connection;
        }

        private bool TryAcceptConnection(ENetConnection connection, ref SpanBufferReader reader)
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
                if (DedicatedInstance.ShouldAcceptConnection(connection.EndPoint, ref reader))
                {
                    connection.State = ENetConnectionState.Accepted;
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
            
            _logger.Information("Closing ENet connection (PeerId={PeerId}, EndPoint={Port})",
                connection.NativePeerId, connection.EndPoint);

            connection.State = ENetConnectionState.Disconnected;
            
            if (!IsAlive || !sendKick)
                // Can't or won't send kick
                return;
            
            _ignorance.Commands.Enqueue(new IgnoranceCommandPacket()
            {
                Type = IgnoranceCommandType.ServerKickPeer,
                PeerId = peerId
            });
        }

        #endregion
    }
}