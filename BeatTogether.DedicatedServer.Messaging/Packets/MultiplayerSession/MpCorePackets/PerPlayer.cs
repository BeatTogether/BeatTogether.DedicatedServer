using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets
{
    public sealed class PerPlayer : INetSerializable
    {
        public bool PPDEnabled;
        public bool PPMEnabled;

        public void ReadFrom(ref SpanBuffer bufferReader)
        {
            PPDEnabled = bufferReader.ReadBool();
            PPMEnabled = bufferReader.ReadBool();
        }

        public void WriteTo(ref SpanBuffer bufferWriter)
        {
            bufferWriter.WriteBool(PPDEnabled);
            bufferWriter.WriteBool(PPMEnabled);
        }
    }
}