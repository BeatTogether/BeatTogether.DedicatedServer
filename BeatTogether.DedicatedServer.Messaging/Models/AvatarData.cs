using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using Krypton.Buffers;
using System.Drawing;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class AvatarData : INetSerializable
    {
        public string? HeadTopId { get; set; }
        public Color HeadTopPrimaryColor { get; set; }
        public Color HeadTopSecondaryColor { get; set; }
        public string? GlassesId { get; set; }
        public Color GlassesColor { get; set; }
        public string? FacialHairId { get; set; }
        public Color FacialHairColor { get; set; }
        public string? HandsId { get; set; }
        public Color HandsColor { get; set; }
        public string? ClothesId { get; set; }
        public Color ClothesPrimaryColor { get; set; }
        public Color ClothesSecondaryColor { get; set; }
        public Color ClothesDetailColor { get; set; }
        public string? SkinColorId { get; set; }
        public string? EyesId { get; set; }
        public string? MouthId { get; set; }

        public void ReadFrom(ref SpanBufferReader reader)
        {
            HeadTopId = reader.ReadUTF8String();
            HeadTopPrimaryColor = reader.ReadColor();
            HandsColor = reader.ReadColor();
            ClothesId = reader.ReadUTF8String();
            ClothesPrimaryColor = reader.ReadColor();
            ClothesSecondaryColor = reader.ReadColor();
            ClothesDetailColor = reader.ReadColor();
            reader.SkipBytes(8);
            EyesId = reader.ReadUTF8String();
            MouthId = reader.ReadUTF8String();
            GlassesColor = reader.ReadColor();
            FacialHairColor = reader.ReadColor();
            HeadTopSecondaryColor = reader.ReadColor();
            GlassesId = reader.ReadUTF8String();
            FacialHairId = reader.ReadUTF8String();
            HandsId = reader.ReadUTF8String();
            SkinColorId = GlassesId;  // Don't ask
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteUTF8String(HeadTopId);
            writer.WriteColor(HeadTopPrimaryColor);
            writer.WriteColor(HandsColor);
            writer.WriteUTF8String(ClothesId);
            writer.WriteColor(ClothesPrimaryColor);
            writer.WriteColor(ClothesSecondaryColor);
            writer.WriteColor(ClothesDetailColor);
            writer.WriteColor(new Color());
            writer.WriteColor(new Color());
            writer.WriteUTF8String(EyesId);
            writer.WriteUTF8String(MouthId);
            writer.WriteColor(GlassesColor);
            writer.WriteColor(FacialHairColor);
            writer.WriteColor(HeadTopSecondaryColor);
            writer.WriteUTF8String(GlassesId);
            writer.WriteUTF8String(FacialHairId);
            writer.WriteUTF8String(HandsId);
        }
    }
}
