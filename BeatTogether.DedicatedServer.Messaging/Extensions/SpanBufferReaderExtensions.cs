using Krypton.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace BeatTogether.Extensions
{
    public static class SpanBufferReaderExtensions
    {
        public static (byte SenderId, byte ReceiverId) ReadRoutingHeader(this ref SpanBufferReader reader) =>
            (reader.ReadUInt8(), reader.ReadUInt8());

        public static bool TryReadRoutingHeader(this ref SpanBufferReader reader, [MaybeNullWhen(false)] out (byte SenderId, byte ReceiverId) routingHeader)
        {
            if (reader.RemainingSize < 2)
            {
                routingHeader = default;
                return false;
            }

            routingHeader = (reader.ReadUInt8(), reader.ReadUInt8());
            return true;
        }
    }
}
