using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class ObstacleSpawnPacket : BaseRpcWithValuesPacket
    {
        public override void ReadFrom(ref SpanBuffer reader)
        {
            base.ReadFrom(ref reader);
        }
        public override void WriteTo(ref SpanBuffer writer)
        {
            base.WriteTo(ref writer);
        }
    }
}
