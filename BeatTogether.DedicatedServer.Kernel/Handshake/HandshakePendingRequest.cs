using System;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Handshake
{
    public class HandshakePendingRequest
    {
        public IUnconnectedDispatcher UnconnectedDispatcher { get; private set; }
        public HandshakeSession Session { get; private set; }
        public IReliableRequest Request { get; private set; }
        public DateTime LastSend { get; private set; }
        public int RetryCount { get; private set; }

        public uint RequestId => Request.RequestId;

        public HandshakePendingRequest(IUnconnectedDispatcher unconnectedDispatcher, HandshakeSession session,
            IReliableRequest request)
        {
            UnconnectedDispatcher = unconnectedDispatcher;
            Session = session;
            Request = request;
            LastSend = DateTime.Now;
            RetryCount = 0;
        }


        public void Retry()
        {
            UnconnectedDispatcher.Send(Session, Request, true);
            LastSend = DateTime.Now;
            RetryCount++;
        }

        public int? GetRetryInterval()
        {
            switch (RetryCount)
            {
                case 0:
                    return 200;
                case 1:
                    return 300;
                case 2:
                    return 450;
                case 3:
                    return 600;
                case 4:
                    return 1000;
                default:
                    return null;
            }
        }

        public double MsSinceLastSend => DateTime.Now.Subtract(LastSend).TotalMilliseconds;
        
        public bool HasExpired => GetRetryInterval() == null;
        
        public bool ShouldRetry
        {
            get
            {
                var interval = GetRetryInterval();
                return interval.HasValue && MsSinceLastSend >= interval;
            }
        }
    }
}