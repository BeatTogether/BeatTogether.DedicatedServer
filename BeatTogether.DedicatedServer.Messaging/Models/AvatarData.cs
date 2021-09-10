using System.Drawing;
using BeatTogether.Extensions;
using LiteNetLib.Utils;

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

        public void Deserialize(NetDataReader reader)
        {
            HeadTopId = reader.GetString();
            HeadTopPrimaryColor = reader.GetColor();
            HandsColor = reader.GetColor();
            ClothesId = reader.GetString();
            ClothesPrimaryColor = reader.GetColor();
            ClothesSecondaryColor = reader.GetColor();
            ClothesDetailColor = reader.GetColor();
            reader.SkipBytes(8);
            EyesId = reader.GetString();
            MouthId = reader.GetString();
            GlassesColor = reader.GetColor();
            FacialHairColor = reader.GetColor();
            HeadTopSecondaryColor = reader.GetColor();
            GlassesId = reader.GetString();
            FacialHairId = reader.GetString();
            HandsId = reader.GetString();
            SkinColorId = GlassesId;  // Don't ask
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(HeadTopId);
            writer.Put(HeadTopPrimaryColor);
            writer.Put(HandsColor);
            writer.Put(ClothesId);
            writer.Put(ClothesPrimaryColor);
            writer.Put(ClothesSecondaryColor);
            writer.Put(ClothesDetailColor);
            writer.Put(new Color());
            writer.Put(new Color());
            writer.Put(EyesId);
            writer.Put(MouthId);
            writer.Put(GlassesColor);
            writer.Put(FacialHairColor);
            writer.Put(HeadTopSecondaryColor);
            writer.Put(GlassesId);
            writer.Put(FacialHairId);
            writer.Put(HandsId);
        }
    }
}
