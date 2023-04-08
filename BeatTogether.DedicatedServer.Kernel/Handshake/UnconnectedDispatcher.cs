using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.LiteNetLib;
using BeatTogether.LiteNetLib.Dispatchers;
using BeatTogether.LiteNetLib.Enums;
using Krypton.Buffers;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.Handshake
{
    public class UnconnectedDispatcher : UnconnectedMessageDispatcher, IUnconnectedDispatcher
    {
        private readonly IMessageWriter _messageWriter;
        private readonly ILogger _logger;
        
        public UnconnectedDispatcher(LiteNetServer server, IMessageWriter messageWriter) : base(server)
        {
            _messageWriter = messageWriter;
            _logger = Log.ForContext<UnconnectedDispatcher>();
        }
        
        public void Send(HandshakeSession session, IMessage message)
        {
            _logger.Information("Sending handshake message of type {MessageType} (EndPoint={EndPoint})",
                message.GetType().Name, session.EndPoint.ToString());

            if (message is IRequest requestMessage)
                requestMessage.RequestId = session.GetNextRequestId();
            
            // TODO Reliable retries

            var bufferWriter = new SpanBufferWriter(stackalloc byte[412]);
            _messageWriter.WriteTo(ref bufferWriter, message);
            Send(session.EndPoint, bufferWriter, UnconnectedMessageType.BasicMessage);
        }
    }
}