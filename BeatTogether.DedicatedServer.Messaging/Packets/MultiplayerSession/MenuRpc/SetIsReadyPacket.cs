using BeatTogether.DedicatedServer.Messaging.Abstractions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetIsReadyPacket : BaseRpcPacket
    {
        public bool IsReady { get; set; }

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            IsReady = reader.GetBool();
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.Put(IsReady);
        }
    }
}
