﻿using System;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class ConnectionRequestData : INetSerializable
    {
        public const string SessionIdPrefix = "ps:bt$";
        
        // Beat Saber
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public bool IsConnectionOwner { get; set; }
        
        // BasicConnectionRequestHandler
        public string? Secret { get; set; } = null!;
        
        // GameLiftClientConnectionRequestHandler
        public string? PlayerSessionId { get; set; } = null!;

        public void ReadFrom(ref SpanBufferReader reader)
        {
            Secret = null;
            PlayerSessionId = null;
            
            var initialOffset = reader.Offset;

            // Try to read as a GameLift connection request
            try
            {
                UserId = reader.ReadString();
                UserName = reader.ReadString();
                IsConnectionOwner = reader.ReadBool();
                PlayerSessionId = reader.ReadString();
            }
            catch (Exception ex) { }

            if (PlayerSessionId != null && PlayerSessionId.StartsWith(SessionIdPrefix))
                // Read OK, valid session identifier
                return;

            // Rewind, try to read as basic request
            reader.SkipBytes(initialOffset - reader.Offset);
            
            Secret = reader.ReadString();
            UserId = reader.ReadString();
            UserName = reader.ReadString();
            IsConnectionOwner = reader.ReadBool();
            PlayerSessionId = null;
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            if (!string.IsNullOrEmpty(PlayerSessionId))
            {
                // GameLift
                writer.WriteString(UserId);
                writer.WriteString(UserName);
                writer.WriteBool(IsConnectionOwner);
                writer.WriteString(PlayerSessionId);
            }
            else
            {
                // Basic
                writer.WriteString(Secret ?? "");
                writer.WriteString(UserId);
                writer.WriteString(UserName);
                writer.WriteBool(IsConnectionOwner);
            }
        }
    }
}
