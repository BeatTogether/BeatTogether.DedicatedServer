using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using Krypton.Buffers;
using System.Drawing;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class AvatarData : INetSerializable
    {
        public string HeadTopId { get; set; } = null!;
        public Color HeadTopPrimaryColor { get; set; }
        public Color HeadTopSecondaryColor { get; set; }
        public string GlassesId { get; set; } = null!;
        public Color GlassesColor { get; set; }
        public string FacialHairId { get; set; } = null!;
        public Color FacialHairColor { get; set; }
        public string HandsId { get; set; } = null!;
        public Color HandsColor { get; set; }
        public string ClothesId { get; set; } = null!;
        public Color ClothesPrimaryColor { get; set; }
        public Color ClothesSecondaryColor { get; set; }
        public Color ClothesDetailColor { get; set; }
        public string SkinColorId { get; set; } = null!;
        public string EyesId { get; set; } = null!;
        public string MouthId { get; set; } = null!;

        public void ReadFrom(ref SpanBufferReader reader)
        {
            HeadTopId = reader.ReadString();
            HeadTopPrimaryColor = reader.ReadColor();
            HandsColor = reader.ReadColor();
            ClothesId = reader.ReadString();
            ClothesPrimaryColor = reader.ReadColor();
            ClothesSecondaryColor = reader.ReadColor();
            ClothesDetailColor = reader.ReadColor();
            reader.SkipBytes(8);
            EyesId = reader.ReadString();
            MouthId = reader.ReadString();
            GlassesColor = reader.ReadColor();
            FacialHairColor = reader.ReadColor();
            HeadTopSecondaryColor = reader.ReadColor();
            GlassesId = reader.ReadString();
            FacialHairId = reader.ReadString();
            HandsId = reader.ReadString();
            SkinColorId = GlassesId;  // Don't ask
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {                
            switch (HeadTopId)
            {
                case null:
                    writer.WriteString("BedHead");
                    break;
                default:
                    writer.WriteString(HeadTopId);
                    break;
            }
            writer.WriteColor(HeadTopPrimaryColor);
            writer.WriteColor(HandsColor);
            switch (ClothesId)
            {
                case null:
                    writer.WriteString("Hoodie");
                    break;
                default:
                    writer.WriteString(ClothesId);
                    break;
            }
            writer.WriteColor(ClothesPrimaryColor);
            writer.WriteColor(ClothesSecondaryColor);
            writer.WriteColor(ClothesDetailColor);
            writer.WriteColor(new Color());
            writer.WriteColor(new Color());
            switch (EyesId)
            {
                case null:
                    writer.WriteString("Eyes1");
                    break;
                default:
                    writer.WriteString(EyesId);
                    break;
            }
            switch (MouthId)
            {
                case null:
                    writer.WriteString("Mouth8");
                    break;
                default:
                    writer.WriteString(MouthId);
                    break;
            }
            writer.WriteColor(GlassesColor);
            writer.WriteColor(FacialHairColor);
            writer.WriteColor(HeadTopSecondaryColor);
            switch (GlassesId)
            {
                case null:
                    writer.WriteString("Default");
                    SkinColorId = "Default";
                    break;
                default:
                    writer.WriteString(GlassesId);
                    break;
            }
            switch (FacialHairId)
            {
                case null:
                    writer.WriteString("None");
                    break;
                default:
                    writer.WriteString(FacialHairId);
                    break;
            }
            switch (HandsId)
            {
                case null:
                    writer.WriteString("BareHands");
                    break;
                default:
                    writer.WriteString(HandsId);
                    break;
            }
        }
    }
}
