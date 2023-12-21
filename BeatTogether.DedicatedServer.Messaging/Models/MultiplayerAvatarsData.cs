using BeatTogether.DedicatedServer.Messaging.Converter;
using BeatTogether.DedicatedServer.Messaging.Structs;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class MultiplayerAvatarsData : INetSerializable
    {
        public List<MultiplayerAvatarData>? AvatarsData { get; set; }
        public BitMask128 SupportedAvatarTypeIdHashesBloomFilter { get; set; } = AddBloomFilterEntryHash(new BitMask128(), AvatarDataMultiplayerAvatarsDataConverter.BaseGameAvatarSystemTypeIdentifier.AvatarTypeIdentifierHash, 3, 8);
        
        public MultiplayerAvatarsData()
        {
            AvatarData defaultAvatar = new AvatarData();
            AvatarsData = new List<MultiplayerAvatarData>
            {
                defaultAvatar.CreateMultiplayerAvatarsData()
            };
        }

        public MultiplayerAvatarsData(List<MultiplayerAvatarData> multiplayerAvatarsData, IEnumerable<uint> supportedAvatarTypeIdHashes)
        {
            AvatarsData = multiplayerAvatarsData;
            SupportedAvatarTypeIdHashesBloomFilter = ToBloomFilter(supportedAvatarTypeIdHashes, 3, 8);
        }

        // Token: 0x060000A2 RID: 162 RVA: 0x00003D91 File Offset: 0x00001F91
        public MultiplayerAvatarsData(List<MultiplayerAvatarData> multiplayerAvatarsData, BitMask128 supportedAvatarTypeIdHashesBloomFilter)
        {
            AvatarsData = multiplayerAvatarsData;
            SupportedAvatarTypeIdHashesBloomFilter = supportedAvatarTypeIdHashesBloomFilter;
        }
        
        public void WriteTo(ref SpanBuffer writer)
        {
            WriteToAvatarsData(ref writer);
            SupportedAvatarTypeIdHashesBloomFilter.WriteTo(ref writer);
        }
        public void WriteToAvatarsData(ref SpanBuffer writer)
        {
            if (AvatarsData == null)
            {
                writer.WriteInt32(0);
                return;
            }
            writer.WriteInt32(AvatarsData.Count);
            foreach (MultiplayerAvatarData multiplayerAvatarData in AvatarsData)
            {
                writer.WriteUInt32(multiplayerAvatarData.AvatarTypeIdentifierHash);
                writer.WriteByteArray(multiplayerAvatarData.Data);
            }
        }

        public void ReadFrom(ref SpanBuffer reader)
        {
            AvatarsData = ReadFromAvatarsData(ref reader);
            SupportedAvatarTypeIdHashesBloomFilter.ReadFrom(ref reader);
        }
        public static List<MultiplayerAvatarData> ReadFromAvatarsData(ref SpanBuffer reader)
        {
            int @int = reader.ReadInt32();
            List<MultiplayerAvatarData> list = new List<MultiplayerAvatarData>(@int);
            for (int i = 0; i < @int; i++)
            {
                uint @uint = reader.ReadUInt32();
                ReadOnlySpan<byte> byteArray = reader.ReadByteArray();
                list.Add(new MultiplayerAvatarData(@uint, byteArray.ToArray()));
            }
            return list;
        }
        public static BitMask128 ToBloomFilter(IEnumerable<uint> hashes, int hashCount = 3, int hashBits = 8)
        {
            return hashes.Aggregate(new BitMask128(), (BitMask128 bloomFilter, uint hash) => AddBloomFilterEntryHash(bloomFilter, hash, hashCount, hashBits));
        }

        public static BitMask128 AddBloomFilterEntryHash(BitMask128 bitMask, uint hash, int hashCount = 3, int hashBits = 8)
        {
            for (int i = 0; i < hashCount; i++)
            {
                bitMask = bitMask.SetBits((int)((ulong)hash % (ulong)((long)bitMask.BitCount)), 1UL);
                hash >>= hashBits;
            }
            return bitMask;
        }
    }
}
