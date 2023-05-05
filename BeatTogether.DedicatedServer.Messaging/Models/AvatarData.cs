using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;
using Krypton.Buffers;
using System.Drawing;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class AvatarData : INetSerializable
    {
        public string HeadTopId { get; set; } = "BedHead";
        public Color HeadTopPrimaryColor { get; set; }
        public Color HeadTopSecondaryColor { get; set; }
        public string GlassesId { get; set; } = "Default";
        public Color GlassesColor { get; set; }
        public string FacialHairId { get; set; } = "None";
        public Color FacialHairColor { get; set; }
        public string HandsId { get; set; } = "BareHands";
        public Color HandsColor { get; set; }
        public string ClothesId { get; set; } = "Hoodie";
        public Color ClothesPrimaryColor { get; set; }
        public Color ClothesSecondaryColor { get; set; }
        public Color ClothesDetailColor { get; set; }
        public string SkinColorId { get; set; } = "Default";
        public string EyesId { get; set; } = "Eyes1";
        public string MouthId { get; set; } = "Mouth8";

        public void ReadFrom(ref SpanBuffer reader)
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

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteString(HeadTopId);
            writer.WriteColor(HeadTopPrimaryColor);
            writer.WriteColor(HandsColor);
            writer.WriteString(ClothesId);
            writer.WriteColor(ClothesPrimaryColor);
            writer.WriteColor(ClothesSecondaryColor);
            writer.WriteColor(ClothesDetailColor);
            writer.WriteColor(new Color());
            writer.WriteColor(new Color());
            writer.WriteString(EyesId);
            writer.WriteString(MouthId);
            writer.WriteColor(GlassesColor);
            writer.WriteColor(FacialHairColor);
            writer.WriteColor(HeadTopSecondaryColor);
            writer.WriteString(GlassesId);
            writer.WriteString(FacialHairId);
            writer.WriteString(HandsId);
        }
    }
}
