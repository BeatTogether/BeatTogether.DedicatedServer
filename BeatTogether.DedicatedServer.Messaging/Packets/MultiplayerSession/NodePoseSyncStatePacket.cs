using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession
{
    public sealed class NodePoseSyncStatePacket : INetSerializable
    {
        public byte SyncStateId { get; set; }
        public float Time { get; set; }
        public NodePoseSyncState State { get; set; } = new();

        public void ReadFrom(ref SpanBuffer reader)
        {
            SyncStateId = reader.ReadUInt8();
            Time = reader.ReadFloat32();
            State.ReadFrom(ref reader);
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteUInt8(SyncStateId);
            writer.WriteFloat32(Time);
            State.WriteTo(ref writer);
        }
    }
}
