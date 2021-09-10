using BeatTogether.DedicatedServer.Messaging.Abstractions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class SetSongStartTimePacket : BaseRpcPacket
    {
        public float StartTime { get; set; }

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            StartTime = reader.GetFloat();
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.Put(StartTime);
        }
    }
}
