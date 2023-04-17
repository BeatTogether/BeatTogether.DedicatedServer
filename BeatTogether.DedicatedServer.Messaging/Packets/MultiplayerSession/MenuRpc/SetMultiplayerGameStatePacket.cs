using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetMultiplayerGameStatePacket : BaseRpcWithValuesPacket
    {
        public MultiplayerGameState State { get; set; }

        public override void ReadFrom(ref SpanBuffer reader)
        {
            base.ReadFrom(ref reader);
            if (HasValue0)
                State = (MultiplayerGameState)reader.ReadVarInt();
        }

        public override void WriteTo(ref SpanBuffer writer)
        {
            base.WriteTo(ref writer);
            writer.WriteVarInt((int)State);
        }
        public override void ReadFrom(ref MemoryBuffer reader)
        {
            base.ReadFrom(ref reader);
            if (HasValue0)
                State = (MultiplayerGameState)reader.ReadVarInt();
        }

        public override void WriteTo(ref MemoryBuffer writer)
        {
            base.WriteTo(ref writer);
            writer.WriteVarInt((int)State);
        }
    }
}
