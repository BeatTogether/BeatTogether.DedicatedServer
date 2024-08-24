
namespace BeatTogether.DedicatedServer.Messaging.Structs
{
    public readonly struct MultiplayerAvatarData
    {
        public byte[]? Data { get; }
        public uint AvatarTypeIdentifierHash { get; }

        public MultiplayerAvatarData(uint avatarTypeIdentifierHash, byte[]? data)
        {
            AvatarTypeIdentifierHash = avatarTypeIdentifierHash;
            Data = data;
        }

    }
}
