using LiteNetLib.Utils;

namespace BeatTogether.Extensions
{
    public static class NetDataWriterExtensions
    {
        public static void PutRoutingHeader(this NetDataWriter writer, byte senderId, byte receiverId)
        {
            writer.Put(senderId);
            writer.Put(receiverId);
        }
    }
}
