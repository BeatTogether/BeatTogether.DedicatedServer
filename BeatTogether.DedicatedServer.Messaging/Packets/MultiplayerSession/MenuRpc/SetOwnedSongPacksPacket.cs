using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetOwnedSongPacksPacket : BaseRpcWithValuesPacket
    {
        public SongPackMask SongPackMask { get; set; } = new();

        public override void ReadFrom(ref SpanBuffer reader)
        {
            base.ReadFrom(ref reader);
            if (HasValue0)
                SongPackMask.ReadFrom(ref reader);
        }

        public override void WriteTo(ref SpanBuffer writer)
        {
            base.WriteTo(ref writer);
            SongPackMask?.WriteTo(ref writer);
        }
        public override void ReadFrom(ref MemoryBuffer reader)
        {
            base.ReadFrom(ref reader);
            if (HasValue0)
                SongPackMask.ReadFrom(ref reader);
        }

        public override void WriteTo(ref MemoryBuffer writer)
        {
            base.WriteTo(ref writer);
            SongPackMask?.WriteTo(ref writer);
        }
    }
}
