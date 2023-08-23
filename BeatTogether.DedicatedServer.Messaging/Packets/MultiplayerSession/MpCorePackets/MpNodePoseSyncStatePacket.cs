using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets
{
    public sealed class MpNodePoseSyncStatePacket : INetSerializable
    {
        public float deltaUpdateFrequency;
        public float fullStateUpdateFrequency;

        public void WriteTo(ref SpanBuffer bufferWriter)
        {
            bufferWriter.WriteFloat32(deltaUpdateFrequency);
            bufferWriter.WriteFloat32(fullStateUpdateFrequency);
        }
        public void ReadFrom(ref SpanBuffer bufferReader)
        {
            deltaUpdateFrequency = bufferReader.ReadFloat32();
            fullStateUpdateFrequency = bufferReader.ReadFloat32();
        }
    }

}
