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
            if (SkinColorId == null || SkinColorId == string.Empty)
                SkinColorId = "Default";
            if (HeadTopId == null || HeadTopId == string.Empty)
                HeadTopId = "BedHead";
            writer.WriteString(HeadTopId);
            writer.WriteColor(HeadTopPrimaryColor);
            writer.WriteColor(HandsColor);
            if (ClothesId == null || ClothesId == string.Empty)
                ClothesId = "Hoodie";
            writer.WriteString(ClothesId);
            writer.WriteColor(ClothesPrimaryColor);
            writer.WriteColor(ClothesSecondaryColor);
            writer.WriteColor(ClothesDetailColor);
            writer.WriteColor(new Color());
            writer.WriteColor(new Color());
            if (EyesId == null || EyesId == string.Empty)
                EyesId = "Eyes1";
            writer.WriteString(EyesId);
            if (MouthId == null || MouthId == string.Empty)
                MouthId = "Mouth8";
            writer.WriteString(MouthId);
            writer.WriteColor(GlassesColor);
            writer.WriteColor(FacialHairColor);
            writer.WriteColor(HeadTopSecondaryColor);
            if (GlassesId == null || GlassesId == string.Empty)
                GlassesId = "Default";
            writer.WriteString(GlassesId);
            if (FacialHairId == null || FacialHairId == string.Empty)
                FacialHairId = "None";
            writer.WriteString(FacialHairId);
            if (HandsId == null || HandsId == string.Empty)
                HandsId = "BareHands";
            writer.WriteString(HandsId);
        }
    }
}
