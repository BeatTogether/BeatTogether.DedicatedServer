using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Extensions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Messages.GameLift
{
    public sealed class AuthenticateGameLiftUserRequest : IEncryptedMessage, IReliableRequest, IReliableResponse
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
        
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string PlayerSessionId { get; set; }

        public void ReadFrom(ref SpanBufferReader reader)
        {
            UserId = reader.ReadString();
            UserName = reader.ReadString();
            PlayerSessionId = reader.ReadString();
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteString(UserId);
            writer.WriteString(UserName);
            writer.WriteString(PlayerSessionId);
        }
    }
}
