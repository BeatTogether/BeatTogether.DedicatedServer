using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MPChatPackets
{
    public class MpcCapabilitiesPacket : MpcBasePacket
    {
        public bool CanTextChat;
        public bool CanReceiveVoiceChat;
        public bool CanTransmitVoiceChat;

        public override void WriteTo(ref SpanBuffer writer)
        {
            base.WriteTo(ref writer);
            writer.WriteBool(CanTextChat);
            writer.WriteBool(CanReceiveVoiceChat);
            writer.WriteBool(CanTransmitVoiceChat);
        }

        public override void ReadFrom(ref SpanBuffer reader)
        {
            base.ReadFrom(ref reader);

            CanTextChat = reader.ReadBool();
            CanReceiveVoiceChat = reader.ReadBool();
            CanTransmitVoiceChat = reader.ReadBool();
        }
    }
}


