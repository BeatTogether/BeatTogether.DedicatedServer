using BeatTogether.DedicatedServer.Interface.Enums;
using System;
using System.Drawing;

namespace BeatTogether.DedicatedServer.Interface.Models
{
    public record AvatarData(
        string HeadTopId,
        Color HeadTopPrimaryColor,
        Color HeadTopSecondaryColor,
        string GlassesId,
        Color GlassesColor,
        string FacialHairId,
        Color FacialHairColor,
        string HandsId,
        Color HandsColor,
        string ClothesId,
        Color ClothesPrimaryColor,
        Color ClothesSecondaryColor,
        Color ClothesDetailColor,
        string SkinColorId,
        string EyesId,
        string MouthId
        );
}
