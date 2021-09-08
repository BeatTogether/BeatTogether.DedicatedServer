using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class StartLevelPacket : BaseRpcPacket
    {
        public BeatmapIdentifierNetSerializable Beatmap { get; set; } = new();
        public GameplayModifiers Modifiers { get; set; } = new();
        public float StartTime { get; set; }

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            Beatmap.Deserialize(reader);
            Modifiers.Deserialize(reader);
            StartTime = reader.GetFloat();
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            Beatmap.Serialize(writer);
            Modifiers.Serialize(writer);
            writer.Put(StartTime);
        }
    }
}
