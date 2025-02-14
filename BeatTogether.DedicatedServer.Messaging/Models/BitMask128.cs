﻿using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public class BitMask128 : INetSerializable
    {
		public int BitCount { get => 128; }

        public ulong Top { get; set; }
        public ulong Bottom { get; set; }

        public BitMask128(ulong top, ulong bottom)
        {
            Top = top;
            Bottom = bottom;
        }

        public BitMask128() { }

        public bool Contains(string value, int hashCount = 3, int hashBits = 8)
        {
			uint hash = MurmurHash2(value);
			for (int i = 0; i < hashCount; i++)
			{
				if (GetBits((int)(hash % (ulong)((long)BitCount)), 1) == 0UL)
				{
					return false;
				}
                hash >>= hashBits;
			}
			return true;
		}

		public void WriteToBitMask(string value, int hashCount = 3, int hashBits = 8)
        {
			ulong hash = MurmurHash2(value);
			for(int i = 0; i < hashCount; i++)
            {
				SetBits((int)(hash % (ulong)((long)BitCount)), 1UL);
				hash >>= hashBits;
			}
        }

		public ulong GetBits(int offset, int count)
		{
			ulong num = (1UL << count) - 1UL;
			int num2 = offset - 64;
			return (ShiftRight(Top, num2) | ShiftRight(Bottom, offset)) & num;
		}

        public BitMask128 SetBits(int offset, ulong bits)
        {
            ulong d = Top;
            int num = offset - 64;
            return new BitMask128(d | ShiftLeft(bits, num), Bottom | ShiftLeft(bits, offset));
        }

        public void ReadFrom(ref SpanBuffer reader)
        {
            Top = reader.ReadUInt64();
            Bottom = reader.ReadUInt64();
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteUInt64(Top);
            writer.WriteUInt64(Bottom);
        }

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
