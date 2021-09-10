using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.Extensions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerDisconnectedPacket : INetSerializable
    {
        public DisconnectedReason DisconnectedReason { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            DisconnectedReason = (DisconnectedReason)reader.GetVarInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutVarInt((int)DisconnectedReason);
        }
    }
}
