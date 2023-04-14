using System.Threading.Tasks;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Handshake;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public abstract class BaseHandshakeMessageHandler<TMessage> : IHandshakeMessageHandler<TMessage>
        where TMessage : class, IMessage
    {
        public abstract Task<IMessage?> Handle(HandshakeSession session, TMessage message);

        public Task<IMessage?> Handle(HandshakeSession session, IMessage message) =>
            Handle(session, (TMessage) message);
    }
}