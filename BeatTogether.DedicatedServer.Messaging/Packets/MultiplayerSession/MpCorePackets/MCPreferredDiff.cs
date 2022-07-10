using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets
{
    public sealed class MCPreferredDiff : INetSerializable
    {
        public uint PreferredDifficulty { get; set; }

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteUInt32(PreferredDifficulty);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            PreferredDifficulty = bufferReader.ReadUInt32();
        }

    }
}
