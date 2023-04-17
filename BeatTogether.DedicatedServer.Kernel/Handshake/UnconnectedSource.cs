using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Core.Messaging.Messages;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Encryption;
using BeatTogether.DedicatedServer.Messaging.Messages.Handshake;
using BeatTogether.LiteNetLib.Enums;
using BeatTogether.LiteNetLib.Sources;
using BeatTogether.LiteNetLib.Util;
using Krypton.Buffers;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.Handshake
{
    public class UnconnectedSource : UnconnectedMessageSource, IDisposable
    {
        private readonly IDedicatedInstance _instance;
        private readonly IServiceProvider _serviceProvider;
        private readonly IUnconnectedDispatcher _unconnectedDispatcher;
        private readonly IMessageReader _messageReader;
        private readonly IHandshakeSessionRegistry _handshakeSessionRegistry;
        private readonly PacketEncryptionLayer _packetEncryptionLayer;

        private readonly ConcurrentDictionary<EndPoint, HandshakeSession> _activeSessions;
        private readonly CancellationTokenSource _stopCts;
        private readonly ILogger _logger;

        public UnconnectedSource(
            IDedicatedInstance instance, 
            IServiceProvider serviceProvider,
            IUnconnectedDispatcher unconnectedDispatcher,
            IMessageReader messageReader,
            IHandshakeSessionRegistry handshakeSessionRegistry,
            PacketEncryptionLayer packetEncryptionLayer)
        {
            _instance = instance;
            _serviceProvider = serviceProvider;
            _unconnectedDispatcher = unconnectedDispatcher;
            _messageReader = messageReader;
            _handshakeSessionRegistry = handshakeSessionRegistry;
            _packetEncryptionLayer = packetEncryptionLayer;
            
            _activeSessions = new();
            _stopCts = new();
            _logger = Log.ForContext<UnconnectedSource>();
            
            Task.Run(() => UpdateLoop(_stopCts.Token));
            _instance.StopEvent += HandleInstanceStop;
        }

        public void Dispose()
        {
            _instance.StopEvent -= HandleInstanceStop;
        }

        private void HandleInstanceStop(IDedicatedInstance inst) => _stopCts.Cancel();

        public IMessageReader GetMessageReader() => _messageReader;

        #region Receive

        public override void OnReceive(EndPoint remoteEndPoint, ref MemoryBuffer reader,
            UnconnectedMessageType type)
        {

            SpanBufferReader spanBufferReader = new(reader.RemainingData.Span.ToArray());
            var session = _handshakeSessionRegistry.GetOrAdd(remoteEndPoint);
            var message = _messageReader.ReadFrom(ref spanBufferReader);

            Task.Run(() => HandleMessage(session, message));

        }

        public async Task HandleMessage(HandshakeSession session, IMessage message)
        {
            var messageType = message.GetType();

            if (message is ClientHelloRequest)
            {
                // Received client hello, first handshake message - ensure encryption is OFF for this endpoint
                // This prevents outbound encryption with stale parameters from previous/incomplete handshake
                _packetEncryptionLayer.RemoveEncryptedEndPoint((IPEndPoint)session.EndPoint);
            }

            // Skip previously handled messages
            if (message is IRequest request && !session.ShouldHandleRequest(request.RequestId))
            {
                _logger.Warning("Skipping duplicate request (MessageType={Type}, RequestId={RequestId})",
                    messageType.Name, request.RequestId
                );
                return;
            }

            // Acknowledge reliable messages
            uint requestId = 0;
            
            if (message is IReliableRequest reliableRequest)
            {
                requestId = reliableRequest.RequestId;
                
                _unconnectedDispatcher.Send(session, new AcknowledgeMessage()
                {
                    ResponseId = requestId,
                    MessageHandled = true
                });
            }

            // Dispatch to handler
            _logger.Verbose("Handling handshake message of type {MessageType} (EndPoint={EndPoint})",
                messageType.Name, session.EndPoint.ToString());

            if (message is MultipartMessage multipartMessage)
            {
                if (!session.PendingMultiparts.ContainsKey(multipartMessage.MultipartMessageId))
                {
                    session.PendingMultiparts.TryAdd(multipartMessage.MultipartMessageId,
                        new HandshakePendingMultipart(this, session, multipartMessage.MultipartMessageId,
                            multipartMessage.TotalLength));
                }
                
                session.PendingMultiparts[multipartMessage.MultipartMessageId].AddMessage(multipartMessage);
                return;
            }
            
            var targetHandlerType = typeof(IHandshakeMessageHandler<>).MakeGenericType(messageType);
            var messageHandler = _serviceProvider.GetService(targetHandlerType);

            if (messageHandler is null)
            {
                _logger.Warning("No handler exists for handshake message {MessageType}",
                    messageType.Name);
                return;
            }

            try
            {
                var replyMessage = await ((IHandshakeMessageHandler) messageHandler).Handle(session, message);

                // Send response, if any
                if (replyMessage == null)
                    return;

                if (replyMessage is IResponse responseMessage)
                    responseMessage.ResponseId = requestId;

                _unconnectedDispatcher.Send(session, replyMessage);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception handling message {MessageType} (EndPoint={EndPoint})",
                    messageType.Name, session.EndPoint.ToString());
            }
        }

        #endregion
        
        #region Update / Retry

        private async Task UpdateLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var session in _activeSessions.Values)
                {
                    var pendingInSession = session.PendingMultiparts.Values;

                    if (pendingInSession.Count == 0)
                    {
                        // Nothing left pending in session, remove from tracked list
                        _activeSessions.TryRemove(session.EndPoint, out _);
                        continue;
                    }

                    foreach (var pendingRequest in pendingInSession)
                    {
                        if (pendingRequest.HasExpired || pendingRequest.IsComplete)
                        {
                            // Clean up completed / expired
                            session.PendingMultiparts.TryRemove(pendingRequest.MultipartMessageId, out _);
                            break;
                        }
                    }
                }

                await Task.Delay(1000, cancellationToken);
            }
        }

        #endregion
    }
}