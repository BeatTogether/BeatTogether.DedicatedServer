﻿using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Commands
{
    public class AllowPerPlayerDifficulties : ITextCommand
    {
        public string CommandName => "allowperplayerdifficulties";
        public string ShortHandName => "ppd";
        public string Description => "if set to true, then players will use what ever difficulty they have selected, default false";

        public bool Enabled = false;

        public void ReadValues(string[] Values)
        {
            if (Values != null)
                Enabled = Values[0] == "true";
        }
    }
}
