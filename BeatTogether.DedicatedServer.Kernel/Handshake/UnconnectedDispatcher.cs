using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Encryption;
using BeatTogether.DedicatedServer.Messaging.Messages.Handshake;
using BeatTogether.LiteNetLib;
using BeatTogether.LiteNetLib.Dispatchers;
using BeatTogether.LiteNetLib.Enums;
using Krypton.Buffers;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.Handshake
{
    public class UnconnectedDispatcher : UnconnectedMessageDispatcher, IUnconnectedDispatcher, IDisposable
    {
        private readonly IDedicatedInstance _instance;
        private readonly IMessageWriter _messageWriter;
        private readonly PacketEncryptionLayer _packetEncryptionLayer;

        private readonly ConcurrentDictionary<EndPoint, HandshakeSession> _activeSessions;
        private readonly CancellationTokenSource _stopCts;
        private readonly ILogger _logger;

        public UnconnectedDispatcher(LiteNetServer server, IDedicatedInstance instance, IMessageWriter messageWriter,
            PacketEncryptionLayer packetEncryptionLayer) : base(server)
        {
            _instance = instance;
            _messageWriter = messageWriter;
            _packetEncryptionLayer = packetEncryptionLayer;

            _activeSessions = new();
            _stopCts = new();
            _logger = Log.ForContext<UnconnectedDispatcher>();

            Task.Run(() => UpdateLoop(_stopCts.Token));
            _instance.StopEvent += HandleInstanceStop;
        }

        public void Dispose()
        {
            _instance.StopEvent -= HandleInstanceStop;
        }

        private void HandleInstanceStop(IDedicatedInstance inst) => _stopCts.Cancel();

        #region API

        public void Send(HandshakeSession session, IMessage message, bool retry = false)
        {
            _logger.Verbose("Sending handshake message of type {MessageType} (EndPoint={EndPoint})",
                message.GetType().Name, session.EndPoint.ToString());

            // Assign request ID to outgoing requests
            if (message is IRequest requestMessage && !retry) 
            {
                requestMessage.RequestId = session.GetNextRequestId();
            }

            // Track reliable requests for retry
            if (message is IReliableRequest reliableRequest)
            {
                if (!retry)
                {
                    session.PendingRequests[reliableRequest.RequestId] =
                        new HandshakePendingRequest(this, session, reliableRequest);
                    _activeSessions.TryAdd(session.EndPoint, session);
                }
            }

            var bufferWriter = new SpanBufferWriter(stackalloc byte[412]);
            _messageWriter.WriteTo(ref bufferWriter, message);
            Send(session.EndPoint, bufferWriter, UnconnectedMessageType.BasicMessage);
        }

        public bool Acknowledge(HandshakeSession session, uint responseId, bool handled = true)
        {
            var ackOk = session.PendingRequests.TryRemove(responseId, out var ackedRequest);

            if (ackOk && handled)
            {
                if (ackedRequest!.Request is ChangeCipherSpecRequest)
                {
                    // The client acknowledged & handled the change cipher request
                    // We can now turn on the encryption layer
                    _packetEncryptionLayer.AddEncryptedEndPoint((IPEndPoint)session.EndPoint, 
                        session.EncryptionParameters!);
                }
            }

            return ackOk;
        }

        #endregion

        #region Update / Retry

        private async Task UpdateLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var session in _activeSessions.Values)
                {
                    var pendingInSession = session.PendingRequests.Values;

                    if (pendingInSession.Count == 0)
                    {
                        // Nothing left to retry in session, remove from tracked list
                        _activeSessions.TryRemove(session.EndPoint, out _);
                        continue;
                    }

                    foreach (var pendingRequest in pendingInSession)
                    {
                        if (pendingRequest.HasExpired)
                        {
                            // Max retries exceeded
                            session.PendingRequests.TryRemove(pendingRequest.RequestId, out _);
                            break;
                        }

                        if (!pendingRequest.ShouldRetry)
                        {
                            // Waiting for retry interval
                            continue;
                        }

                        pendingRequest.Retry();
                    }
                }

                await Task.Delay(100, cancellationToken);
            }
        }

        #endregion
    }
}