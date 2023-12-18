using BeatTogether.DedicatedServer.Messaging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
