﻿using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Commands
{
    public class SetPerPlayerModifiers : ITextCommand
    {
        public string CommandName => "setperplayermodifiers";
        public string ShortHandName => "ppm";
        public string Description => "if set to true, then players will use what ever modifiers they have selected, default false";

        public bool Enabled = false;

        public void ReadValues(string[] Values)
        {
            if (Values != null)
                Enabled = Values[0] == "true" || Values[0] == "t";
        }
    }
}
