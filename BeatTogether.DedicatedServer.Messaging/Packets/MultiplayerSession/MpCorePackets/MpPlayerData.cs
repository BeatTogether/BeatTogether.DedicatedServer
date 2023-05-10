using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets
{
    public sealed class MpPlayerData : INetSerializable
    {
        public string PlatformID = string.Empty;
        public byte Platform;
        public string ClientVersion = string.Empty;
    
        public void WriteTo(ref SpanBuffer bufferWriter)
        {           
            bufferWriter.WriteString(PlatformID);
            bufferWriter.WriteUInt8(Platform);
            bufferWriter.WriteString(ClientVersion);
        }
        public void ReadFrom(ref SpanBuffer bufferReader)
        {
            PlatformID = bufferReader.ReadString();
            Platform = bufferReader.ReadUInt8();
            ClientVersion = bufferReader.ReadString();
        }
    }
}
