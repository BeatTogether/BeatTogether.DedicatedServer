using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class AvatarData : INetSerializable
    {
        public string HeadTopId { get; set; } = "BedHead";
        public Color HeadTopPrimaryColor { get; set; } = new Color();
        public Color HeadTopSecondaryColor { get; set; } = new Color();
        public string GlassesId { get; set; } = "Default";
        public Color GlassesColor { get; set; } = new Color();
        public string FacialHairId { get; set; } = "None";
        public Color FacialHairColor { get; set; } = new Color();
        public string HandsId { get; set; } = "BareHands";
        public Color HandsColor { get; set; } = new Color();
        public string ClothesId { get; set; } = "Hoodie";
        public Color ClothesPrimaryColor { get; set; } = new Color();
        public Color ClothesSecondaryColor { get; set; } = new Color();
        public Color ClothesDetailColor { get; set; } = new Color();
        public string SkinColorId { get; set; } = "Default";
        public string EyesId { get; set; } = "Eyes1";
        public string MouthId { get; set; } = "Mouth8";

        public AvatarData() { }

        public AvatarData(string headTopId, Color headTopPrimaryColor, Color headTopSecondaryColor, string glassesId, Color glassesColor, string facialHairId, Color facialHairColor, string handsId, Color handsColor, string clothesId, Color clothesPrimaryColor, Color clothesSecondaryColor, Color clothesDetailColor, string skinColorId, string eyesId, string mouthId)
        {
            HeadTopId = headTopId;
            HeadTopPrimaryColor = headTopPrimaryColor;
            HeadTopSecondaryColor = headTopSecondaryColor;
            GlassesId = glassesId;
            GlassesColor = glassesColor;
            FacialHairId = facialHairId;
            FacialHairColor = facialHairColor;
            HandsId = handsId;
            HandsColor = handsColor;
            ClothesId = clothesId;
            ClothesPrimaryColor = clothesPrimaryColor;
            ClothesSecondaryColor = clothesSecondaryColor;
            ClothesDetailColor = clothesDetailColor;
            SkinColorId = skinColorId;
            EyesId = eyesId;
            MouthId = mouthId;
        }

        public void ReadFrom(ref SpanBuffer reader)
        {
            HeadTopId = reader.ReadString();
            HeadTopPrimaryColor.ReadFrom(ref reader);
            HandsColor.ReadFrom(ref reader);
            ClothesId = reader.ReadString();
            ClothesPrimaryColor.ReadFrom(ref reader);
            ClothesSecondaryColor.ReadFrom(ref reader);
            ClothesDetailColor.ReadFrom(ref reader);
            reader.SkipBytes(8);
            EyesId = reader.ReadString();
            MouthId = reader.ReadString();
            GlassesColor.ReadFrom(ref reader);
            FacialHairColor.ReadFrom(ref reader);
            HeadTopSecondaryColor.ReadFrom(ref reader);
            GlassesId = reader.ReadString();
            FacialHairId = reader.ReadString();
            HandsId = reader.ReadString();
            SkinColorId = GlassesId;  // Don't ask
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteString(HeadTopId);
            HeadTopPrimaryColor.WriteTo(ref writer);
            HandsColor.WriteTo(ref writer);
            writer.WriteString(ClothesId);
            ClothesPrimaryColor.WriteTo(ref writer);
            ClothesSecondaryColor.WriteTo(ref writer);
            ClothesDetailColor.WriteTo(ref writer);
            writer.WriteColor(new Color());
            writer.WriteColor(new Color());
            writer.WriteString(EyesId);
            writer.WriteString(MouthId);
            GlassesColor.WriteTo(ref writer);
            FacialHairColor.WriteTo(ref writer);
            HeadTopSecondaryColor.WriteTo(ref writer);
            writer.WriteString(GlassesId);
            writer.WriteString(FacialHairId);
            writer.WriteString(HandsId);
        }
    }
}
