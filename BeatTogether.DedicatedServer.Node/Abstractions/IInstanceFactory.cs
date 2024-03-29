﻿using BeatTogether.DedicatedServer.Interface.Models;
using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Node.Abstractions
{
    public interface IInstanceFactory
    {
        public IDedicatedInstance? CreateInstance(
            string secret,
            string managerId,
            GameplayServerConfiguration config,
            bool permanentManager = false,//If a user links there account to discord and uses a bot to make a lobby, then can enter there userId
            float instanceTimeout = 0f,
            string ServerName = "",
            float resultScreenTime = 20.0f,
            float BeatmapStartTime = 5.0f,
            float PlayersReadyCountdownTime = 0f,
            bool AllowPerPlayerModifiers = false,
            bool AllowPerPlayerDifficulties = false,
            bool AllowChroma = true,
            bool AllowME = true,
            bool AllowNE = true
            );
    }
}
