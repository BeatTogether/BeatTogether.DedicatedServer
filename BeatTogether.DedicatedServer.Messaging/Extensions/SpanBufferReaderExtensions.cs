using BeatTogether.DedicatedServer.Messaging.Models;
using Krypton.Buffers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;

namespace BeatTogether.DedicatedServer.Messaging.Extensions
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

        public static void ReadColor(this ref SpanBufferReader reader, ref Models.Color color)
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
        public static ulong ReadVarULong(this ref SpanBufferReader bufferReader)
        {
            var value = 0UL;
            var shift = 0;
            var b = bufferReader.ReadByte();
            while ((b & 128UL) != 0UL)
            {
                value |= (b & 127UL) << shift;
                shift += 7;
                b = bufferReader.ReadByte();
            }
            return value | (ulong)b << shift;
        }

        public static long ReadVarLong(this ref SpanBufferReader bufferReader)
        {
            var varULong = (long)bufferReader.ReadVarULong();
            if ((varULong & 1L) != 1L)
                return varULong >> 1;
            return -(varULong >> 1) + 1L;
        }

        public static uint ReadVarUInt(this ref SpanBufferReader bufferReader)
            => (uint)bufferReader.ReadVarULong();

        public static int ReadVarInt(this ref SpanBufferReader bufferReader)
            => (int)bufferReader.ReadVarLong();

        public static bool TryReadVarULong(this ref SpanBufferReader bufferReader, out ulong value)
        {
            value = 0UL;
            var shift = 0;
            while (shift <= 63 && bufferReader.RemainingSize >= 1)
            {
                var b = bufferReader.ReadByte();
                value |= (ulong)(b & 127) << shift;
                shift += 7;
                if ((b & 128) == 0)
                    return true;
            }

            value = 0UL;
            return false;
        }

        public static bool TryReadVarUInt(this ref SpanBufferReader bufferReader, out uint value)
        {
            ulong num;
            if (bufferReader.TryReadVarULong(out num) && (num >> 32) == 0UL)
            {
                value = (uint)num;
                return true;
            }

            value = 0U;
            return false;
        }

        public static ReadOnlySpan<byte> ReadVarBytes(this ref SpanBufferReader bufferReader)
        {
            var length = bufferReader.ReadVarUInt();
            return bufferReader.ReadBytes((int)length);
        }

        public static string ReadString(this ref SpanBufferReader bufferReader, int maxLength = 65535)
        {
            var length = bufferReader.ReadInt32();
            if (length <= 0 | length > maxLength)
                return string.Empty;
            var bytes = bufferReader.ReadBytes(length);
            return Encoding.UTF8.GetString(bytes);
        }

        public static IPEndPoint ReadIPEndPoint(this ref SpanBufferReader bufferReader)
        {
            if (!IPAddress.TryParse(bufferReader.ReadString(512), out var address))
                throw new ArgumentException("Failed to parse IP address");
            var port = bufferReader.ReadInt32();
            return new IPEndPoint(address, port);
        }

        public static System.Drawing.Color ReadColor(this ref SpanBufferReader reader)
        {
            var r = reader.ReadByte();
            var g = reader.ReadByte();
            var b = reader.ReadByte();
            var a = reader.ReadByte();
            return System.Drawing.Color.FromArgb(a, r, g, b);
        }
    }
}
