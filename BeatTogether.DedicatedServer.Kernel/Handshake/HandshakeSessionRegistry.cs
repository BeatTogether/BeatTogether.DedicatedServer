﻿using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.Handshake
{
    public class HandshakeSessionRegistry : IHandshakeSessionRegistry
    {
        private readonly ConcurrentDictionary<EndPoint, HandshakeSession> _sessions;
        private readonly ConcurrentDictionary<string, bool> _pendingPlayerSessionIds;

        private readonly ILogger _logger;

        public HandshakeSessionRegistry()
        {
            _sessions = new();
            _pendingPlayerSessionIds = new();
            
            _logger = Log.ForContext<HandshakeSessionRegistry>();
        }

        public HandshakeSession GetOrAdd(EndPoint endPoint)
        {
            return _sessions.GetOrAdd(endPoint, (ep) => new HandshakeSession(ep));
        }

        public HandshakeSession? TryGetByPlayerSessionId(string playerSessionId)
        {
            return (from s in _sessions
                where s.Value.PlayerSessionId == playerSessionId
                select s.Value).FirstOrDefault();
        }

        public void AddPendingPlayerSessionId(string playerSessionId)
        {
            _pendingPlayerSessionIds[playerSessionId] = true;
        }

        public bool TryRemovePendingPlayerSessionId(string playerSessionId)
        {
            return _pendingPlayerSessionIds.TryRemove(playerSessionId, out _);
        }
    }
}