using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets
{
    public sealed class MCSetNewManagerPacket : INetSerializable
    {
        public string NewManagerID { get; set; } = null!;

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteString(NewManagerID);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            NewManagerID = bufferReader.ReadString();
        }
    }
}
