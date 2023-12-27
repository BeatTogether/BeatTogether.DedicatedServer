using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;
using Serilog;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public class Vector3 : INetSerializable
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

        public override string ToString()
        {
            return $"(x: {x}, y: {y}, z: {z})";
        }
    }
}
