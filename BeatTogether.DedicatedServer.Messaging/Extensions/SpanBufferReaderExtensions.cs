using BeatTogether.DedicatedServer.Messaging.Models;
using Krypton.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace BeatTogether.Extensions
{
    public static class SpanBufferReaderExtensions
    {
        public static (byte SenderId, byte ReceiverId, byte PacketOption) ReadRoutingHeader(this ref SpanBufferReader reader) =>
            (reader.ReadUInt8(), reader.ReadUInt8(), reader.ReadUInt8());

        public static bool TryReadRoutingHeader(this ref SpanBufferReader reader, [MaybeNullWhen(false)] out (byte SenderId, byte ReceiverId, byte PacketOption) routingHeader)
        {
            if (reader.RemainingSize < 2)
            {
                routingHeader = default;
                return false;
            }

            routingHeader = (reader.ReadUInt8(), reader.ReadUInt8(), reader.ReadUInt8());
            return true;
        }

        public static void ReadBytesArray(this ref SpanBufferReader reader, ref byte[]? array)
        {
            int length = (int)reader.ReadVarUInt();
            array = reader.ReadBytes(length).ToArray();
        }

        public static void ReadColor(this ref SpanBufferReader reader, ref Color color)
        {
            color.r = reader.ReadFloat32();
            color.g = reader.ReadFloat32();
            color.b = reader.ReadFloat32();
            color.a = reader.ReadFloat32();
        }

        public static void ReadColorNoAlpha(this ref SpanBufferReader reader, ref ColorNoAlpha color)
        {
            color.r = reader.ReadFloat32();
            color.g = reader.ReadFloat32();
            color.b = reader.ReadFloat32();
        }

        public static void ReadBeatmapIdentifier(this ref SpanBufferReader reader, ref BeatmapIdentifier beatmapIdentifier)
        {
            beatmapIdentifier.LevelId = reader.ReadString();
            beatmapIdentifier.Characteristic = reader.ReadString();
            beatmapIdentifier.Difficulty = (BeatmapDifficulty)reader.ReadVarUInt();
        }
    }
}
