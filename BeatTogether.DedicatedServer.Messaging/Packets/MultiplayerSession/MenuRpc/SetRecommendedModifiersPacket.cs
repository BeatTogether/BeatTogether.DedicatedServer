using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetRecommendedModifiersPacket : BaseRpcPacket
    {
        public GameplayModifiers Modifiers { get; set; } = new();

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            Modifiers.Deserialize(reader);
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            Modifiers.Serialize(writer);
        }
    }
}
