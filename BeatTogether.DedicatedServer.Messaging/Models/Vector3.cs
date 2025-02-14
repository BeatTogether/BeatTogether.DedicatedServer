﻿using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public struct Vector3 : INetSerializable
    {
        public int x;
        public int y;
        public int z;

        public void ReadFrom(ref SpanBuffer reader)
        {
            x = reader.ReadVarInt();
            y = reader.ReadVarInt();
            z = reader.ReadVarInt();
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteVarInt(x);
            writer.WriteVarInt(y);
            writer.WriteVarInt(z);
        }
    }
}
