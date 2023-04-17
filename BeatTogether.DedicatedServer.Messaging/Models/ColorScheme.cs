using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class ColorScheme : INetSerializable
    {
        public ColorNoAlpha SaberAColor { get; set; } = new();
        public ColorNoAlpha SaberBColor { get; set; } = new();
        public ColorNoAlpha ObstaclesColor { get; set; } = new();
        public ColorNoAlpha EnvironmentColor0 { get; set; } = new();
        public ColorNoAlpha EnvironmentColor1 { get; set; } = new();
        public ColorNoAlpha EnvironmentColor0Boost { get; set; } = new();
        public ColorNoAlpha EnvironmentColor1Boost { get; set; } = new();

        public void ReadFrom(ref SpanBuffer reader)
        {
            SaberAColor.ReadFrom(ref reader);
            SaberBColor.ReadFrom(ref reader);
            ObstaclesColor.ReadFrom(ref reader);
            EnvironmentColor0.ReadFrom(ref reader);
            EnvironmentColor1.ReadFrom(ref reader);
            EnvironmentColor0Boost.ReadFrom(ref reader);
            EnvironmentColor1Boost.ReadFrom(ref reader);
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            SaberAColor.WriteTo(ref writer);
            SaberBColor.WriteTo(ref writer);
            ObstaclesColor.WriteTo(ref writer);
            EnvironmentColor0.WriteTo(ref writer);
            EnvironmentColor1.WriteTo(ref writer);
            EnvironmentColor0Boost.WriteTo(ref writer);
            EnvironmentColor1Boost.WriteTo(ref writer);
        }
        public void ReadFrom(ref MemoryBuffer reader)
        {
            SaberAColor.ReadFrom(ref reader);
            SaberBColor.ReadFrom(ref reader);
            ObstaclesColor.ReadFrom(ref reader);
            EnvironmentColor0.ReadFrom(ref reader);
            EnvironmentColor1.ReadFrom(ref reader);
            EnvironmentColor0Boost.ReadFrom(ref reader);
            EnvironmentColor1Boost.ReadFrom(ref reader);
        }

        public void WriteTo(ref MemoryBuffer writer)
        {
            SaberAColor.WriteTo(ref writer);
            SaberBColor.WriteTo(ref writer);
            ObstaclesColor.WriteTo(ref writer);
            EnvironmentColor0.WriteTo(ref writer);
            EnvironmentColor1.WriteTo(ref writer);
            EnvironmentColor0Boost.WriteTo(ref writer);
            EnvironmentColor1Boost.WriteTo(ref writer);
        }
    }
}
