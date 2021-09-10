using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class SetPlayerDidConnectLatePacket : BaseRpcPacket
    {
        public string UserId { get; set; } = null!;
        public PlayerSpecificSettingsAtStart PlayersAtStart { get; set; } = new();
        public string SessionGameId { get; set; } = null!;

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            UserId = reader.GetString();
            PlayersAtStart.Deserialize(reader);
            SessionGameId = reader.GetString();
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.Put(UserId);
            PlayersAtStart.Serialize(writer);
            writer.Put(SessionGameId);
        }
    }
}
