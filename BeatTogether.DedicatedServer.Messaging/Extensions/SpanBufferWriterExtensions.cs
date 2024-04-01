using Krypton.Buffers;
using System;
using System.Drawing;
using System.Net;
using System.Text;

namespace BeatTogether.DedicatedServer.Messaging.Extensions
{
    public static class SpanBufferWriterExtensions
    {
        public static void WriteVarULong(this ref SpanBufferWriter bufferWriter, ulong value)
        {
            do
            {
                var b = (byte)(value & 127UL);
                value >>= 7;
                if (value != 0UL)
                    b |= 128;
                bufferWriter.WriteUInt8(b);
            } while (value != 0UL);
        }

        public static void WriteVarLong(this ref SpanBufferWriter bufferWriter, long value)
            => bufferWriter.WriteVarULong((value < 0L ? (ulong)((-value << 1) - 1L) : (ulong)(value << 1)));

        public static void WriteVarUInt(this ref SpanBufferWriter buffer, uint value)
            => buffer.WriteVarULong(value);

        public static void WriteVarInt(this ref SpanBufferWriter bufferWriter, int value)
            => bufferWriter.WriteVarLong(value);

        public static void WriteVarBytes(this ref SpanBufferWriter bufferWriter, ReadOnlySpan<byte> bytes)
        {
            bufferWriter.WriteVarUInt((uint)bytes.Length);
            bufferWriter.WriteBytes(bytes);
        }

        public static void WriteString(this ref SpanBufferWriter bufferWriter, string value)
        {
            bufferWriter.WriteInt32(Encoding.UTF8.GetByteCount(value));
            bufferWriter.WriteBytes(Encoding.UTF8.GetBytes(value));
        }

        public static void WriteIPEndPoint(this ref SpanBufferWriter bufferWriter, IPEndPoint ipEndPoint)
        {
            bufferWriter.WriteString(ipEndPoint.Address.ToString());
            bufferWriter.WriteInt32(ipEndPoint.Port);
        }

        public static void WriteColor(this ref SpanBufferWriter writer, Color value)
        {
            writer.WriteUInt8(value.R);
            writer.WriteUInt8(value.G);
            writer.WriteUInt8(value.B);
            writer.WriteUInt8(value.A);
        }
    }
}
