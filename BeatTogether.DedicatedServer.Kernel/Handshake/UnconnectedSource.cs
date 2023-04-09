using System;
using System.Net;
using System.Threading.Tasks;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Core.Messaging.Messages;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.LiteNetLib.Enums;
using BeatTogether.LiteNetLib.Sources;
using Krypton.Buffers;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.Handshake
{
    public class UnconnectedSource : UnconnectedMessageSource
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUnconnectedDispatcher _unconnectedDispatcher;
        private readonly IMessageReader _messageReader;
        private readonly IHandshakeSessionRegistry _handshakeSessionRegistry;

        private readonly ILogger _logger;

        public UnconnectedSource(IServiceProvider serviceProvider, IUnconnectedDispatcher unconnectedDispatcher,
            IMessageReader messageReader, IHandshakeSessionRegistry handshakeSessionRegistry)
        {
            _serviceProvider = serviceProvider;
            _unconnectedDispatcher = unconnectedDispatcher;
            _messageReader = messageReader;
            _handshakeSessionRegistry = handshakeSessionRegistry;

            _logger = Log.ForContext<UnconnectedSource>();
        }

        #region Receive

        public override void OnReceive(EndPoint remoteEndPoint, ref SpanBufferReader reader,
            UnconnectedMessageType type)
        {
            var session = _handshakeSessionRegistry.GetOrAdd(remoteEndPoint);
            var message = _messageReader.ReadFrom(ref reader);

            Task.Run(() => HandleMessage(session, message));
        }

        public async Task HandleMessage(HandshakeSession session, IMessage message)
        {
            var messageType = message.GetType();

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
    }
}