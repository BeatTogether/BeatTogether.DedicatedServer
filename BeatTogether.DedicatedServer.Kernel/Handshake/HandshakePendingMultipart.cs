using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.Core.Messaging.Messages;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Kernel.Handshake
{
    public class HandshakePendingMultipart
    {
        public UnconnectedSource UnconnectedSource { get; private set; }
        public HandshakeSession Session { get; private set; }
        public uint MultipartMessageId { get; private set; }
        public uint TotalLength { get; private set; }
        public DateTime LastReceive { get; private set; }
        
        private readonly ConcurrentDictionary<uint, MultipartMessage> _messages;
        private uint _receivedLength;

        public bool IsComplete { get; private set; }

        public HandshakePendingMultipart(UnconnectedSource unconnectedSource, HandshakeSession session,
            uint multipartMessageId, uint totalLength)
        {
            UnconnectedSource = unconnectedSource;
            Session = session;
            MultipartMessageId = multipartMessageId;
            TotalLength = totalLength;
            LastReceive = DateTime.Now;

            _messages = new();
            _receivedLength = 0;
        }
        
        public void AddMessage(MultipartMessage message)
        {
            if (message.MultipartMessageId != MultipartMessageId)
                // Invalid id
                return;
            
            if (_receivedLength >= TotalLength)
                // Already received all messages
                return;
            
            if (!_messages.TryAdd(message.Offset, message))
                // Already received this message
                return;
            
            // Receive success
            LastReceive = DateTime.Now;

            if (Interlocked.Add(ref _receivedLength, message.Length) < TotalLength)
                // Not yet complete, wait for all messages to come in
                return;

            FinalizeMessage();
        }

        private void FinalizeMessage()
        {
            if (IsComplete)
                return;

            IsComplete = true;
            
            var bufferWriter = new SpanBufferWriter(stackalloc byte[(int)TotalLength]);
            
            foreach (var kvp in _messages.OrderBy(kvp => kvp.Key))
                bufferWriter.WriteBytes(kvp.Value.Data);
            
            var bufferReader = new SpanBufferReader(bufferWriter.Data);
            var fullMessage = UnconnectedSource.GetMessageReader().ReadFrom(ref bufferReader);

            Task.Run(() => UnconnectedSource.HandleMessage(Session, fullMessage));
        }
        
        public double MsSinceLastReceive => DateTime.Now.Subtract(LastReceive).TotalMilliseconds;

        public bool HasExpired => MsSinceLastReceive > 10000;
    }
}