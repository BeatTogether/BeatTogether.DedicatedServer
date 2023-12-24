using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Messaging.Packets.Legacy
{
    public interface IVersionedNetSerializable : INetSerializable
    {
        void ReadFrom(ref SpanBuffer reader, Version version);
        void WriteTo(ref SpanBuffer writer, Version version);
    }
}
