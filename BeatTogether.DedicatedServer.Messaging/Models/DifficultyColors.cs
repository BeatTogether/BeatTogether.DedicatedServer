using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class DifficultyColours : INetSerializable
    {
        public ColorNoAlpha? SaberAColor { get; set; }
        public ColorNoAlpha? SaberBColor { get; set; }
        public ColorNoAlpha? EnvironmentColor0 { get; set; }
        public ColorNoAlpha? EnvironmentColor1 { get; set; }
        public ColorNoAlpha? EnvironmentColor0Boost { get; set; }
        public ColorNoAlpha? EnvironmentColor1Boost { get; set; }
        public ColorNoAlpha? ObstaclesColor { get; set; }

        public void ReadFrom(ref SpanBuffer reader)
        {
            var colors = reader.ReadByte();
            if ((colors & 0x1) != 0)
            {
                SaberAColor = new();
                SaberAColor.ReadFrom(ref reader);
            }
            if (((colors >> 1) & 0x1) != 0)
            {
                SaberBColor = new();
                SaberBColor.ReadFrom(ref reader);
            }
            if (((colors >> 2) & 0x1) != 0)
            {
                EnvironmentColor0 = new();
                EnvironmentColor0.ReadFrom(ref reader);
            }
            if (((colors >> 3) & 0x1) != 0)
            {
                EnvironmentColor1 = new();
                EnvironmentColor1.ReadFrom(ref reader);
            }
            if (((colors >> 4) & 0x1) != 0)
            {
                EnvironmentColor0Boost = new();
                EnvironmentColor0Boost.ReadFrom(ref reader);
            }
            if (((colors >> 5) & 0x1) != 0)
            {
                EnvironmentColor1Boost = new();
                EnvironmentColor1Boost.ReadFrom(ref reader);
            }
            if (((colors >> 6) & 0x1) != 0)
            {
                ObstaclesColor = new();
                ObstaclesColor.ReadFrom(ref reader);
            }
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            byte colors = (byte)(SaberAColor != null ? 1 : 0);
            colors |= (byte)((SaberBColor != null ? 1 : 0) << 1);
            colors |= (byte)((EnvironmentColor0 != null ? 1 : 0) << 2);
            colors |= (byte)((EnvironmentColor1 != null ? 1 : 0) << 3);
            colors |= (byte)((EnvironmentColor0Boost != null ? 1 : 0) << 4);
            colors |= (byte)((EnvironmentColor1Boost != null ? 1 : 0) << 5);
            colors |= (byte)((ObstaclesColor != null ? 1 : 0) << 6);
            writer.WriteUInt8(colors);

            if(SaberAColor != null)
                SaberAColor.WriteTo(ref writer);
            if (SaberBColor != null)
                SaberBColor.WriteTo(ref writer);
            if (EnvironmentColor0 != null)
                EnvironmentColor0.WriteTo(ref writer);
            if (EnvironmentColor1 != null)
                EnvironmentColor1.WriteTo(ref writer);
            if (EnvironmentColor0Boost != null)
                EnvironmentColor0Boost.WriteTo(ref writer);
            if (EnvironmentColor1Boost != null)
                EnvironmentColor1Boost.WriteTo(ref writer);
            if (ObstaclesColor != null)
                ObstaclesColor.WriteTo(ref writer);
        }
    }
}
