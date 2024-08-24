using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets
{
    public sealed class DediPacketSetNewManagerPacket : INetSerializable
    {
        public string NewManagerID { get; set; } = null!;

        public void WriteTo(ref SpanBuffer bufferWriter)
        {
            bufferWriter.WriteString(NewManagerID);
        }

        public void ReadFrom(ref SpanBuffer bufferReader)
        {
            NewManagerID = bufferReader.ReadString();
        }
    }
}
