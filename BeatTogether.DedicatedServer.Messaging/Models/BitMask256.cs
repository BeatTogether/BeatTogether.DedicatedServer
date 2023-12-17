using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public class BitMask256 : INetSerializable
    {
        public ulong D0 { get; set; }
        public ulong D1 { get; set; }
        public ulong D2 { get; set; }
        public ulong D3 { get; set; }

        public const int BitCount = 256;

        public static BitMask256 MaxValue => new(ulong.MaxValue, ulong.MaxValue, ulong.MaxValue, ulong.MaxValue);
        public static BitMask256 MinValue => new();

        public BitMask256(ulong d0 = 0U, ulong d1 = 0U, ulong d2 = 0U, ulong d3 = 0U)
        {
            D0 = d0;
            D1 = d1;
            D2 = d2;
            D3 = d3;
        }

        #region Bits

        public BitMask256 SetBits(int offset, ulong bits)
        {
            ulong d = D0;
            int num = offset - 192;
            ulong num2 = d | ShiftLeft(bits, num);
            ulong d2 = D1;
            int num3 = offset - 128;
            ulong num4 = d2 | ShiftLeft(bits, num3);
            ulong d3 = D2;
            int num5 = offset - 64;
            return new BitMask256(num2, num4, d3 | ShiftLeft(bits, num5), D3 | ShiftLeft(bits, offset));
        }

        public ulong GetBits(int offset, int count)
        {
            ulong num = (1UL << count) - 1UL;
            int num2 = offset - 192;
            ulong num3 = ShiftRight(D0, num2);
            int num4 = offset - 128;
            ulong num5 = num3 | ShiftRight(D1, num4);
            int num6 = offset - 64;
            return (num5 | ShiftRight(D2, num6) | ShiftRight(D3, offset)) & num;
        }

        #endregion

        #region Network

        public void WriteTo(ref SpanBuffer bufferWriter)
        {
            bufferWriter.WriteUInt64(D0);
            bufferWriter.WriteUInt64(D1);
            bufferWriter.WriteUInt64(D2);
            bufferWriter.WriteUInt64(D3);
        }

        public void ReadFrom(ref SpanBuffer bufferReader)
        {
            D0 = bufferReader.ReadUInt64();
            D1 = bufferReader.ReadUInt64();
            D2 = bufferReader.ReadUInt64();
            D3 = bufferReader.ReadUInt64();
        }

        #endregion

        private static uint MurmurHash2(string key)
        {
            uint num = (uint)key.Length;
            uint num2 = 33U ^ num;
            int num3 = 0;
            while (num >= 4U)
            {
                uint num4 = (uint)((uint)key[num3 + 3] << 24 | (uint)key[num3 + 2] << 16 | (uint)key[num3 + 1] << 8 | key[num3]);
                num4 *= 1540483477U;
                num4 ^= num4 >> 24;
                num4 *= 1540483477U;
                num2 *= 1540483477U;
                num2 ^= num4;
                num3 += 4;
                num -= 4U;
            }
            switch (num)
            {
                case 1U:
                    num2 ^= (uint)key[num3];
                    num2 *= 1540483477U;
                    break;
                case 2U:
                    num2 ^= (uint)((uint)key[num3 + 1] << 8);
                    num2 ^= (uint)key[num3];
                    num2 *= 1540483477U;
                    break;
                case 3U:
                    num2 ^= (uint)((uint)key[num3 + 2] << 16);
                    num2 ^= (uint)((uint)key[num3 + 1] << 8);
                    num2 ^= (uint)key[num3];
                    num2 *= 1540483477U;
                    break;
            }
            num2 ^= num2 >> 13;
            num2 *= 1540483477U;
            return num2 ^ num2 >> 15;
        }


        public static ulong ShiftLeft(ulong value, in int shift)
        {
            if (shift < 0)
            {
                int num = -shift;
                return ShiftRight(value, num);
            }
            if (shift < 64)
            {
                return value << shift;
            }
            return 0UL;
        }

        public static ulong ShiftRight(ulong value, in int shift)
        {
            if (shift < 0)
            {
                int num = -shift;
                return ShiftLeft(value, num);
            }
            if (shift < 64)
            {
                return value >> shift;
            }
            return 0UL;
        }
    }
}