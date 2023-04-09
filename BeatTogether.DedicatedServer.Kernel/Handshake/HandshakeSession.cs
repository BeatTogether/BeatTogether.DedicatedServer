using System;
using System.Collections.Concurrent;
using System.Net;
using BeatTogether.Core.Messaging.Implementations;
using Org.BouncyCastle.Crypto.Parameters;

namespace BeatTogether.DedicatedServer.Kernel.Handshake
{
    public enum HandshakeSessionState
    {
        None = 0,
        New = 1,
        Established = 2,
        Authenticated = 3
    }
    
    public class HandshakeSession : BaseSession
    {
        
        public byte[] Cookie { get; set; }
        public byte[] ClientRandom { get; set; }
        public byte[] ServerRandom { get; set; }
        public byte[] ClientPublicKey { get; set; }
        public ECPublicKeyParameters ClientPublicKeyParameters { get; set; }
        public ECPrivateKeyParameters ServerPrivateKeyParameters { get; set; }
        public byte[] PreMasterSecret { get; set; }
        public DateTimeOffset LastKeepAlive { get; set; }

        public string? UserId { get; set; } = null;
        public string? UserName { get; set; } = null;
        public string? PlayerSessionId { get; set; } = null;

        public ConcurrentDictionary<uint, HandshakePendingRequest> PendingRequests;
        public ConcurrentDictionary<uint, HandshakePendingMultipart> PendingMultiparts;
        
        public HandshakeSession(EndPoint endPoint) : base(endPoint)
        {
            PendingRequests = new();
            PendingMultiparts = new();
        }
    }
}