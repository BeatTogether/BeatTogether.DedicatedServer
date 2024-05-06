using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Messaging.Util;
using System.Collections.Generic;
using BeatTogether.DedicatedServer.Messaging.Models;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets
{
    public record Contributor(string role, string name, string iconPath);

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

        public Contributor[] contributors = null!;

        public Dictionary<uint, DifficultyColours> diffColors = new();

        public void WriteTo(ref SpanBuffer bufferWriter)
        {
            bufferWriter.WriteString(levelHash);
            bufferWriter.WriteString(songName);
            bufferWriter.WriteString(songSubName);
            bufferWriter.WriteString(songAuthorName);
            bufferWriter.WriteString(levelAuthorName);

            bufferWriter.WriteFloat32(beatsPerMinute);
            bufferWriter.WriteFloat32(songDuration);

            bufferWriter.WriteString(characteristic);
            bufferWriter.WriteUInt32(difficulty);

            bufferWriter.WriteUInt8((byte)requirements.Count);

            //requirements
            foreach (var requirement in requirements)
            {
                bufferWriter.WriteUInt8((byte)requirement.Key);
                bufferWriter.WriteUInt8((byte)requirement.Value.Length);
                for (int i = 0; i < requirement.Value.Length; i++)
                {
                    bufferWriter.WriteString(requirement.Value[i]);
                }
            }
            
            //contributors
            bufferWriter.WriteUInt8((byte)contributors.Length);
            foreach (var contributor in contributors)
            {
                bufferWriter.WriteString(contributor.role);
                bufferWriter.WriteString(contributor.name);
                bufferWriter.WriteString(contributor.iconPath);
            }

            //difficulty colors
            bufferWriter.WriteUInt8((byte)diffColors.Count);
            foreach (var diffColor in diffColors)
            {
                bufferWriter.WriteUInt8((byte)diffColor.Key);
                diffColor.Value.WriteTo(ref bufferWriter);
            }
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

            //contributors
            byte count = bufferReader.ReadByte();
            contributors = new Contributor[count];
            for (int i = 0; i < count; i++)
            {
                contributors[i] = new(bufferReader.ReadString(), bufferReader.ReadString(), bufferReader.ReadString());
            }

            //difficulty colors
            byte count2 = bufferReader.ReadByte();
            for (int i = 0; i < count2; i++)
            {
                var diff = bufferReader.ReadByte();
                DifficultyColours difficultyColours = new();
                difficultyColours.ReadFrom(ref bufferReader);
                diffColors[diff] = difficultyColours;
            }
        }
    }
}
