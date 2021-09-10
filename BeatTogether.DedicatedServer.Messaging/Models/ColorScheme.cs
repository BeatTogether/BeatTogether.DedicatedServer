using LiteNetLib.Utils;

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

        public void Deserialize(NetDataReader reader)
        {
            SaberAColor.Deserialize(reader);
            SaberBColor.Deserialize(reader);
            ObstaclesColor.Deserialize(reader);
            EnvironmentColor0.Deserialize(reader);
            EnvironmentColor1.Deserialize(reader);
            EnvironmentColor0Boost.Deserialize(reader);
            EnvironmentColor1Boost.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            SaberAColor.Serialize(writer);
            SaberBColor.Serialize(writer);
            ObstaclesColor.Serialize(writer);
            EnvironmentColor0.Serialize(writer);
            EnvironmentColor1.Serialize(writer);
            EnvironmentColor0Boost.Serialize(writer);
            EnvironmentColor1Boost.Serialize(writer);
        }
    }
}
