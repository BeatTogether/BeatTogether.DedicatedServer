using BeatTogether.DedicatedServer.Messaging.Abstractions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetIsInLobbyPacket : BaseRpcPacket
    {
        public bool IsInLobby { get; set; }

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            IsInLobby = reader.GetBool();
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.Put(IsInLobby);
        }
    }
}
