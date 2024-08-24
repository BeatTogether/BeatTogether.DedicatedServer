using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets
{
    public sealed class GetMpPerPlayerPacket : INetSerializable
    {
        public void ReadFrom(ref SpanBuffer bufferReader)
        {
        }

        public void WriteTo(ref SpanBuffer bufferWriter)
        {
        }
    }
}