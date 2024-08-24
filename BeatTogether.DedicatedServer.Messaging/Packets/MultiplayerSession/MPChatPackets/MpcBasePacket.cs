using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MPChatPackets
{
    public class MpcBasePacket : INetSerializable
    {

        public uint ProtocolVersion;

        public virtual void WriteTo(ref SpanBuffer writer)
        {
            ProtocolVersion = 1;

            writer.WriteVarUInt(ProtocolVersion);
        }

        public virtual void ReadFrom(ref SpanBuffer reader)
        {
            ProtocolVersion = reader.ReadVarUInt();
        }
    }
}


