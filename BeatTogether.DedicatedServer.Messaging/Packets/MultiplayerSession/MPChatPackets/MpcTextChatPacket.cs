using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MPChatPackets
{
    public class MpcTextChatPacket : MpcBasePacket
    {
        public string Text = string.Empty;

        public override void WriteTo(ref SpanBuffer writer)
        {
            base.WriteTo(ref writer);
            writer.WriteString(Text);
        }

        public override void ReadFrom(ref SpanBuffer reader)
        {
            base.ReadFrom(ref reader);

            Text = reader.ReadString();
        }
    }
}


