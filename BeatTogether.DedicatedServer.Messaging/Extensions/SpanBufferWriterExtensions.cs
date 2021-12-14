using Krypton.Buffers;

namespace BeatTogether.Extensions
{
    public static class SpanBufferWriterExtensions
    {
        public static void WriteRoutingHeader(this ref SpanBufferWriter writer, byte senderId, byte receiverId)
        {
            writer.WriteUInt8(senderId);
            writer.WriteUInt8(receiverId);
        }
    }
}
