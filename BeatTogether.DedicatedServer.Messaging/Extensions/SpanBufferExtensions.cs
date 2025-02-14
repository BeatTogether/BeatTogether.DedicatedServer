﻿using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Util;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Net;
using System.Text;

namespace BeatTogether.Extensions
{
    public static class SpanBufferExtensions
    {
        public static (byte SenderId, byte ReceiverId, PacketOption packetOptions) ReadRoutingHeader(this ref SpanBuffer reader) =>
            (reader.ReadUInt8(), reader.ReadUInt8(), (PacketOption)reader.ReadUInt8());

        public static bool TryReadRoutingHeader(this ref SpanBuffer reader, [MaybeNullWhen(false)] out (byte SenderId, byte ReceiverId, PacketOption packetOptions) routingHeader)
        {
            if (reader.RemainingSize < 2)
            {
                routingHeader = default;
                return false;
            }

            routingHeader = (reader.ReadUInt8(), reader.ReadUInt8(), (PacketOption)reader.ReadUInt8());
            return true;
        }

        public static void WriteRoutingHeader(this ref SpanBuffer writer, byte senderId, byte receiverId, PacketOption packetOptions = PacketOption.None) =>
            writer.WriteRoutingHeader(senderId, receiverId, (byte)packetOptions);



        public static void WriteRoutingHeader(this ref SpanBuffer writer, byte senderId, byte receiverId, byte packetOptions)
        {
            writer.WriteUInt8(senderId);
            writer.WriteUInt8(receiverId);
            writer.WriteUInt8(packetOptions);
        }

        public static ulong ReadVarULong(this ref SpanBuffer bufferReader)
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

        public static long ReadVarLong(this ref SpanBuffer bufferReader)
        {
            long num = (long)bufferReader.ReadVarULong();
            if ((num & 1) != 1)
            {
                return num >> 1;
            }

            return -(num >> 1) + 1;
        }

        public static uint ReadVarUInt(this ref SpanBuffer bufferReader)
        {
            return (uint)bufferReader.ReadVarULong();
        }

        public static int ReadVarInt(this ref SpanBuffer bufferReader)
        {
            return (int)bufferReader.ReadVarLong();
        }

        public static bool TryReadVarULong(this ref SpanBuffer bufferReader, out ulong value)
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

        public static bool TryReadVarUInt(this ref SpanBuffer bufferReader, out uint value)
        {
            if (bufferReader.TryReadVarULong(out var value2) && value2 >> 32 == 0L)
            {
                value = (uint)value2;
                return true;
            }

            value = 0u;
            return false;
        }

        public static ReadOnlySpan<byte> ReadVarBytes(this ref SpanBuffer bufferReader)
        {
            uint count = bufferReader.ReadVarUInt();
            return bufferReader.ReadBytes((int)count);
        }

        public static ReadOnlySpan<byte> ReadByteArray(this ref SpanBuffer bufferReader)
        {
            ushort num = bufferReader.ReadUInt16();
            return bufferReader.ReadBytes(num);
        }

        public static string ReadString(this ref SpanBuffer bufferReader, int maxLength = 65535)
        {
            int num = bufferReader.ReadInt32();
            if (num <= 0 || num > maxLength)
            {
                return string.Empty;
            }

            ReadOnlySpan<byte> bytes = bufferReader.ReadBytes(num);
            return Encoding.UTF8.GetString(bytes);
        }

        public static IPEndPoint ReadIPEndPoint(this ref SpanBuffer bufferReader)
        {
            if (!IPAddress.TryParse(bufferReader.ReadString(512), out var address))
            {
                throw new ArgumentException("Failed to parse IP address");
            }

            int port = bufferReader.ReadInt32();
            return new IPEndPoint(address, port);
        }

        public static Color ReadColor(this ref SpanBuffer reader)
        {
            var r = reader.ReadByte();
            var g = reader.ReadByte();
            var b = reader.ReadByte();
            var a = reader.ReadByte();
            return Color.FromArgb(a, r, g, b);
        }

        public static void WriteVarULong(this ref SpanBuffer bufferWriter, ulong value)
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

        public static void WriteVarLong(this ref SpanBuffer bufferWriter, long value)
        {
            bufferWriter.WriteVarULong((ulong)((value < 0) ? ((-value << 1) - 1) : (value << 1)));
        }

        public static void WriteVarUInt(this ref SpanBuffer buffer, uint value)
        {
            buffer.WriteVarULong(value);
        }

        public static void WriteVarInt(this ref SpanBuffer bufferWriter, int value)
        {
            bufferWriter.WriteVarLong(value);
        }

        public static void WriteVarBytes(this ref SpanBuffer bufferWriter, ReadOnlySpan<byte> bytes)
        {
            bufferWriter.WriteVarUInt((uint)bytes.Length);
            bufferWriter.WriteBytes(bytes);
        }

        public static void WriteByteArray(this ref SpanBuffer bufferWriter, byte[]? arr = null)
        {
            ushort num = (arr == null) ? (ushort)0 : ((ushort)arr.Length);
            bufferWriter.WriteUInt16(num);
            if (arr != null)
            {
                bufferWriter.WriteBytes(arr);
            }
        }

        public static void WriteString(this ref SpanBuffer bufferWriter, string value)
        {
            bufferWriter.WriteInt32(Encoding.UTF8.GetByteCount(value));
            bufferWriter.WriteBytes(Encoding.UTF8.GetBytes(value));
        }

        public static void WriteIPEndPoint(this ref SpanBuffer bufferWriter, IPEndPoint ipEndPoint)
        {
            bufferWriter.WriteString(ipEndPoint.Address.ToString());
            bufferWriter.WriteInt32(ipEndPoint.Port);
        }

        public static void WriteColor(this ref SpanBuffer writer, Color value)
        {
            writer.WriteUInt8(value.R);
            writer.WriteUInt8(value.G);
            writer.WriteUInt8(value.B);
            writer.WriteUInt8(value.A);
        }
    }
}
