using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets
{
    public sealed class GetMpPlayerData : INetSerializable
    {
        public void ReadFrom(ref SpanBuffer bufferReader)
        {
        }

        public void WriteTo(ref SpanBuffer bufferWriter)
        {
        }
    }
}
