using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.Extensions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetMultiplayerGameStatePacket : BaseRpcPacket
    {
        public MultiplayerGameState State { get; set; }

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            State = (MultiplayerGameState)reader.GetVarInt();
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.PutVarInt((int)State);
        }
    }
}
