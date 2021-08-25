using System.Diagnostics.CodeAnalysis;
using LiteNetLib.Utils;

namespace BeatTogether.Extensions
{
    public static class NetDataReaderExtensions
    {
        public static (byte SenderId, byte ReceiverId) GetRoutingHeader(this NetDataReader reader) =>
            (reader.GetByte(), reader.GetByte());

        public static bool TryGetRoutingHeader(this NetDataReader reader, [MaybeNullWhen(false)] out (byte SenderId, byte ReceiverId) routingHeader)
        {
            if (!reader.TryGetByte(out var senderId) ||
                !reader.TryGetByte(out var receiverId))
            {
                routingHeader = default;
                return false;
            }

            routingHeader = (senderId, receiverId);
            return true;
        }
    }
}
