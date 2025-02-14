﻿using System;

namespace BeatTogether.DedicatedServer.Instancing.Configuration
{
    public sealed class InstancingConfiguration
    {
        public string HostEndpoint { get; set; } = "127.0.0.1";
        public int BasePort { get; set; } = 30000;
        public int MaximumSlots { get; set; } = 10000;
    }
}
