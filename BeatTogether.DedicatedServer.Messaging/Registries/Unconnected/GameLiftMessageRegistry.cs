using BeatTogether.Core.Messaging.Implementations.Registries;
using BeatTogether.Core.Messaging.Messages;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Messages.GameLift;

namespace BeatTogether.DedicatedServer.Messaging.Registries.Unconnected
{
    public class GameLiftMessageRegistry : BaseMessageRegistry
    {
        public override uint MessageGroup => 1U; // MessageGroup.User?

        public GameLiftMessageRegistry()
        {
            Register<AuthenticateGameLiftUserRequest>(GameLiftMessageType.AuthenticateUserRequest);
            Register<AuthenticateGameLiftUserResponse>(GameLiftMessageType.AuthenticateUserResponse);
            Register<AcknowledgeMessage>(GameLiftMessageType.MessageReceivedAcknowledge);
            Register<MultipartMessage>(GameLiftMessageType.MultipartMessage);
        }
    }
}
