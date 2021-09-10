using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetPlayersPermissionConfigurationPacket : BaseRpcPacket
    {
        public PlayersPermissionConfiguration PermissionConfiguration { get; set; } = new();

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            PermissionConfiguration.Deserialize(reader);
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            PermissionConfiguration.Serialize(writer);
        }
    }
}
