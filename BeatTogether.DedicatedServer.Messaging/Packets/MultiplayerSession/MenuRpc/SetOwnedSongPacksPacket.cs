using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetOwnedSongPacksPacket : BaseRpcPacket
    {
        public SongPackMask? SongPackMask { get; set; }

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            SongPackMask = new SongPackMask();
            SongPackMask.Deserialize(reader);
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            SongPackMask?.Serialize(writer);
        }
    }
}
