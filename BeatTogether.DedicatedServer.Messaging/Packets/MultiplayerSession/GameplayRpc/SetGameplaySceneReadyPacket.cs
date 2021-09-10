using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class SetGameplaySceneReadyPacket : BaseRpcPacket
    {
        public PlayerSpecificSettings PlayerSpecificSettings { get; set; } = new();

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            PlayerSpecificSettings.Deserialize(reader);
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            PlayerSpecificSettings.Serialize(writer);
        }
    }
}
