using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class SetGameplaySceneSyncFinishedPacket : BaseRpcPacket
    {
        public PlayerSpecificSettingsAtStart PlayersAtStart { get; set; } = new();
        public string SessionGameId { get; set; } = null!;

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            PlayersAtStart.Deserialize(reader);
            SessionGameId = reader.GetString();
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            PlayersAtStart.Serialize(writer);
            writer.Put(SessionGameId);
        }
    }
}
