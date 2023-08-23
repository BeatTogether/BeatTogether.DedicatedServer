using BeatTogether.LiteNetLib.Util;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;

namespace BeatTogether.Extensions
{
    public static class MemoryBufferExtensions
    {
        public static (byte SenderId, byte ReceiverId) ReadRoutingHeader(this MemoryBuffer reader) =>
    (reader.ReadUInt8(), reader.ReadUInt8());

        public static bool TryReadRoutingHeader(this MemoryBuffer reader, [MaybeNullWhen(false)] out (byte SenderId, byte ReceiverId) routingHeader)
        {
            if (reader.RemainingSize < 2)
            {
                routingHeader = default;
                return false;
            }

            routingHeader = (reader.ReadUInt8(), reader.ReadUInt8());
            return true;
        }

        public static void WriteRoutingHeader(this MemoryBuffer writer, byte senderId, byte receiverId)
        {
            writer.WriteUInt8(senderId);
            writer.WriteUInt8(receiverId);
        }

        public static ulong ReadVarULong(this MemoryBuffer bufferReader)
        {
            ulong num = 0uL;
            int num2 = 0;
            byte b = bufferReader.ReadByte();
            while (((ulong)b & 0x80uL) != 0L)
            {
                num |= ((ulong)b & 0x7FuL) << num2;
                num2 += 7;
                b = bufferReader.ReadByte();
            }

            return num | ((ulong)b << num2);
        }

        public static long ReadVarLong(this MemoryBuffer bufferReader)
        {
            long num = (long)bufferReader.ReadVarULong();
            if ((num & 1) != 1)
            {
                return num >> 1;
            }

            return -(num >> 1) + 1;
        }

        public static uint ReadVarUInt(this MemoryBuffer bufferReader)
        {
            return (uint)bufferReader.ReadVarULong();
        }

        public static int ReadVarInt(this MemoryBuffer bufferReader)
        {
            return (int)bufferReader.ReadVarLong();
        }

        public static bool TryReadVarULong(this MemoryBuffer bufferReader, out ulong value)
        {
            value = 0uL;
            int num = 0;
            while (num <= 63 && bufferReader.RemainingSize >= 1)
            {
                byte b = bufferReader.ReadByte();
                value |= (ulong)((long)(b & 0x7F) << num);
                num += 7;
                if ((b & 0x80) == 0)
                {
                    return true;
                }
            }

            value = 0uL;
            return false;
        }

        public static bool TryReadVarUInt(this MemoryBuffer bufferReader, out uint value)
        {
            if (bufferReader.TryReadVarULong(out var value2) && value2 >> 32 == 0L)
            {
                value = (uint)value2;
                return true;
            }

            value = 0u;
            return false;
        }

        public static ReadOnlySpan<byte> ReadVarBytes(this MemoryBuffer bufferReader)
        {
            uint count = bufferReader.ReadVarUInt();
            return bufferReader.ReadBytes((int)count).Span;
        }

        public static string ReadString(this MemoryBuffer bufferReader, int maxLength = 65535)
        {
            int num = bufferReader.ReadInt32();
            if (num <= 0 || num > maxLength)
            {
                return string.Empty;
            }

            ReadOnlySpan<byte> bytes = bufferReader.ReadBytes(num).Span;
            return Encoding.UTF8.GetString(bytes);
        }

        public static IPEndPoint ReadIPEndPoint(this MemoryBuffer bufferReader)
        {
            if (!IPAddress.TryParse(bufferReader.ReadString(512), out var address))
            {
                throw new ArgumentException("Failed to parse IP address");
            }

            int port = bufferReader.ReadInt32();
            return new IPEndPoint(address, port);
        }
        public static void WriteVarULong(this MemoryBuffer bufferWriter, ulong value)
        {
            do
            {
                byte b = (byte)(value & 0x7F);
                value >>= 7;
                if (value != 0L)
                {
                    b = (byte)(b | 0x80u);
                }

                bufferWriter.WriteUInt8(b);
            }
            while (value != 0L);
        }

        public static void WriteVarLong(this MemoryBuffer bufferWriter, long value)
        {
            bufferWriter.WriteVarULong((ulong)((value < 0) ? ((-value << 1) - 1) : (value << 1)));
        }

        public static void WriteVarUInt(this MemoryBuffer buffer, uint value)
        {
            buffer.WriteVarULong(value);
        }

        public static void WriteVarInt(this MemoryBuffer bufferWriter, int value)
        {
            bufferWriter.WriteVarLong(value);
        }

        public static void WriteVarBytes(this MemoryBuffer bufferWriter, ReadOnlySpan<byte> bytes)
        {
            bufferWriter.WriteVarUInt((uint)bytes.Length);
            bufferWriter.WriteBytes(bytes);
        }

        public static void WriteString(this MemoryBuffer bufferWriter, string value)
        {
            bufferWriter.WriteInt32(Encoding.UTF8.GetByteCount(value));
            bufferWriter.WriteBytes(Encoding.UTF8.GetBytes(value));
        }

        public static void WriteIPEndPoint(this MemoryBuffer bufferWriter, IPEndPoint ipEndPoint)
        {
            bufferWriter.WriteString(ipEndPoint.Address.ToString());
            bufferWriter.WriteInt32(ipEndPoint.Port);
        }
    }
}
