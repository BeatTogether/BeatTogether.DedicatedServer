using BeatTogether.DedicatedServer.Messaging.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class ClearSelectedBeatmap : BaseRpcWithValuesPacket
    {
        public override void WriteTo(ref SpanBufferWriter writer)
        {
            base.WriteTo(ref writer);
            writer.WriteInt8(0); //Idk why but seems to be an issue with this packet's size being off by 1 too small or 3 too big. im going with 1 too small for now
        }
    }
}
