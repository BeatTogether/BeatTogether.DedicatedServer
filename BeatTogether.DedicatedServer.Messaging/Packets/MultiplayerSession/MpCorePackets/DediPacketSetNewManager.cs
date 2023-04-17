using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;

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
        public void WriteTo(ref MemoryBuffer bufferWriter)
        {
            bufferWriter.WriteString(NewManagerID);
        }

        public void ReadFrom(ref MemoryBuffer bufferReader)
        {
            NewManagerID = bufferReader.ReadString();
        }
    }
}
