using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets
{
    public enum Platform
    {
        Unknown = 0,
        Steam = 1,
        OculusPC = 2,
        OculusQuest = 3,
        PS4 = 4,
        PS5 = 5,
        Pico = 6,
    }

    public sealed class MpPlayerData : INetSerializable
    {
        public string PlatformID = string.Empty;
        public Platform Platform;
        public string ClientVersion = string.Empty;
    
        public void WriteTo(ref SpanBuffer bufferWriter)
        {           
            bufferWriter.WriteString(PlatformID);
            bufferWriter.WriteInt32((int)Platform);
            bufferWriter.WriteString(ClientVersion.Replace("_","-"));
        }
        public void ReadFrom(ref SpanBuffer bufferReader)
        {
            PlatformID = bufferReader.ReadString();
            Platform = (Platform)bufferReader.ReadInt32();
            ClientVersion = bufferReader.ReadString();
        }
    }
}
