using System.Threading.Tasks;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Handshake;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IHandshakeMessageHandler
    {
        Task<IMessage?> Handle(HandshakeSession session, IMessage message);
    }

    public interface IHandshakeMessageHandler<TMessage> : IHandshakeMessageHandler
        where TMessage : class, IMessage
    {
        Task<IMessage?> Handle(HandshakeSession session, TMessage message);
    }
}
