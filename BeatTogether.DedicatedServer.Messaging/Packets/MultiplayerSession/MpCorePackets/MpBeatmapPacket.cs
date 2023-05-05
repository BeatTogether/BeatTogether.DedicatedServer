using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;
using System.Collections.Generic;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets
{
    public sealed class MpBeatmapPacket : INetSerializable
    {
        public string levelHash = null!;
        public string songName = null!;
        public string songSubName = null!;
        public string songAuthorName = null!;
        public string levelAuthorName = null!;
        public float beatsPerMinute;
        public float songDuration;

        public string characteristic = null!;
        public uint difficulty;

        public Dictionary<uint, string[]> requirements = new();

        public void WriteTo(ref SpanBuffer bufferWriter)
        {
            throw new System.NotImplementedException();
        }
        public void ReadFrom(ref SpanBuffer bufferReader)
        {
            levelHash = bufferReader.ReadString();
            songName = bufferReader.ReadString();
            songSubName = bufferReader.ReadString();
            songAuthorName = bufferReader.ReadString();
            levelAuthorName = bufferReader.ReadString();
            beatsPerMinute = bufferReader.ReadFloat32();
            songDuration = bufferReader.ReadFloat32();

            characteristic = bufferReader.ReadString();
            difficulty = bufferReader.ReadUInt32();

            var difficultyCount = bufferReader.ReadByte();
            for (int i = 0; i < difficultyCount; i++)
            {
                byte difficulty = bufferReader.ReadByte();
                var requirementCount = bufferReader.ReadByte();
                string[] reqsForDifficulty = new string[requirementCount];
                for (int j = 0; j < requirementCount; j++)
                    reqsForDifficulty[j] = bufferReader.ReadString();
                requirements[difficulty] = reqsForDifficulty;
            }

            byte count = bufferReader.ReadByte();
            for (int i = 0; i < count; i++)
            {
                bufferReader.ReadString();
                bufferReader.ReadString();
                bufferReader.ReadString();
            }

            byte count2 = bufferReader.ReadByte();
            for (int i = 0; i < count2; i++)
            {
                bufferReader.ReadByte();
                bufferReader.ReadColor();
            }
        }
    }
}
