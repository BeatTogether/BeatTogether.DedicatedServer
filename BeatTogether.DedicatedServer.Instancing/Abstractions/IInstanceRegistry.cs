using BeatTogether.Core.Enums;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace BeatTogether.DedicatedServer.Instancing.Abstractions
{
    public interface IInstanceRegistry
    {
        public bool AddInstance(IDedicatedInstance instance);
        public bool RemoveInstance(IDedicatedInstance instance);
        public bool TryGetInstance(string secret, [MaybeNullWhen(false)] out IDedicatedInstance instance);
        public bool TryGetInstanceByCode(string code, [MaybeNullWhen(false)] out IDedicatedInstance instance);
        public bool TryGetAvailablePublicServer(InvitePolicy invitePolicy, GameplayServerMode serverMode, SongSelectionMode songMode, GameplayServerControlSettings serverControlSettings, BeatmapDifficultyMask difficultyMask, GameplayModifiersMask modifiersMask, string songPackMasks, [MaybeNullWhen(false)] out IDedicatedInstance instance);
    }
}
