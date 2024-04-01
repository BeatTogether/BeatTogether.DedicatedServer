using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Util;
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

        //TODO: Move class as it's no longer a INetSerializable
        [Obsolete("This method is obsolete and will be removed soon. Use MultiplayerAvatarsData.ReadFrom instead.")]
        public void ReadFrom(ref SpanBuffer reader)
        {
            //HeadTopId = reader.ReadString();
            //HeadTopPrimaryColor = reader.ReadColor();
            //HandsColor = reader.ReadColor();
            //ClothesId = reader.ReadString();
            //ClothesPrimaryColor = reader.ReadColor();
            //ClothesSecondaryColor = reader.ReadColor();
            //ClothesDetailColor = reader.ReadColor();
            //reader.SkipBytes(8);
            //EyesId = reader.ReadString();
            //MouthId = reader.ReadString();
            //GlassesColor = reader.ReadColor();
            //FacialHairColor = reader.ReadColor();
            //HeadTopSecondaryColor = reader.ReadColor();
            //GlassesId = reader.ReadString();
            //FacialHairId = reader.ReadString();
            //HandsId = reader.ReadString();
            //SkinColorId = GlassesId;  // Don't ask
        }

        [Obsolete("This method is obsolete and will be removed soon. Use MultiplayerAvatarsData.WriteTo instead.")]
        public void WriteTo(ref SpanBuffer writer)
        {
            //writer.WriteString(HeadTopId);
            //writer.WriteColor(HeadTopPrimaryColor);
            //writer.WriteColor(HandsColor);
            //writer.WriteString(ClothesId);
            //writer.WriteColor(ClothesPrimaryColor);
            //writer.WriteColor(ClothesSecondaryColor);
            //writer.WriteColor(ClothesDetailColor);
            //writer.WriteColor(new Color());
            //writer.WriteColor(new Color());
            //writer.WriteString(EyesId);
            //writer.WriteString(MouthId);
            //writer.WriteColor(GlassesColor);
            //writer.WriteColor(FacialHairColor);
            //writer.WriteColor(HeadTopSecondaryColor);
            //writer.WriteString(GlassesId);
            //writer.WriteString(FacialHairId);
            //writer.WriteString(HandsId);
        }
    }
}
