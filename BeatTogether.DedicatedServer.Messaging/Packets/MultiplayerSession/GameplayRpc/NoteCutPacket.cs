using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class NoteCutPacket : BaseRpcPacket
    {
        public float SongTime { get; set; }
        public NoteCutInfo Info { get; set; } = new();

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            SongTime = reader.GetFloat();
            Info.Deserialize(reader);
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.Put(SongTime);
            Info.Serialize(writer);
        }
    }
}
