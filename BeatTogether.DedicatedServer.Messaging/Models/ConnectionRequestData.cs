using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class ConnectionRequestData : INetSerializable
    {
        //public const string SessionIdPrefix = "ps:bt$";
        
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public bool IsConnectionOwner { get; set; }
        public string PlayerSessionId { get; set; } = null!;

        public void ReadFrom(ref SpanBuffer reader)
        {
            //read as a GameLift connection request
            UserId = reader.ReadString();
            UserName = reader.ReadString();
            IsConnectionOwner = reader.ReadBool();
            PlayerSessionId = reader.ReadString();
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            // GameLift
            writer.WriteString(UserId);
            writer.WriteString(UserName);
            writer.WriteBool(IsConnectionOwner);
            writer.WriteString(PlayerSessionId);
        }
    }
}
