using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Structs;
using System.IO;

namespace BeatTogether.DedicatedServer.Messaging.Converter
{
    public static class AvatarDataMultiplayerAvatarsDataConverter
    {
        public static AvatarSystemIdentifier BaseGameAvatarSystemTypeIdentifier = new AvatarSystemIdentifier("BeatAvatarSystem");
        public static MultiplayerAvatarData CreateMultiplayerAvatarsData(this AvatarData avatarData)
        {
            MultiplayerAvatarData multiplayerAvatarData;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write(avatarData.HeadTopId);
                    binaryWriter.Write(avatarData.HeadTopPrimaryColor);
                    binaryWriter.Write(avatarData.HeadTopSecondaryColor);
                    binaryWriter.Write(avatarData.GlassesId);
                    binaryWriter.Write(avatarData.GlassesColor);
                    binaryWriter.Write(avatarData.FacialHairId);
                    binaryWriter.Write(avatarData.FacialHairColor);
                    binaryWriter.Write(avatarData.HandsId);
                    binaryWriter.Write(avatarData.HandsColor);
                    binaryWriter.Write(avatarData.ClothesId);
                    binaryWriter.Write(avatarData.ClothesPrimaryColor);
                    binaryWriter.Write(avatarData.ClothesSecondaryColor);
                    binaryWriter.Write(avatarData.ClothesDetailColor);
                    binaryWriter.Write(avatarData.SkinColorId);
                    binaryWriter.Write(avatarData.EyesId);
                    binaryWriter.Write(avatarData.MouthId);
                    byte[] array = memoryStream.ToArray();
                    multiplayerAvatarData = new MultiplayerAvatarData(BaseGameAvatarSystemTypeIdentifier.AvatarTypeIdentifierHash, array);
                }
            }
            return multiplayerAvatarData;
        }

        public static AvatarData CreateAvatarData(this MultiplayerAvatarData multiplayerAvatarsData)
        {
            AvatarData avatarData;
            using (MemoryStream memoryStream = new MemoryStream(multiplayerAvatarsData.Data!))
            {
                memoryStream.Position = 0L;
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    avatarData = new AvatarData(binaryReader.ReadString(), binaryReader.ReadColor(), binaryReader.ReadColor(), binaryReader.ReadString(), binaryReader.ReadColor(), binaryReader.ReadString(), binaryReader.ReadColor(), binaryReader.ReadString(), binaryReader.ReadColor(), binaryReader.ReadString(), binaryReader.ReadColor(), binaryReader.ReadColor(), binaryReader.ReadColor(), binaryReader.ReadString(), binaryReader.ReadString(), binaryReader.ReadString());
                }
            }
            return avatarData;
        }
    }
}
