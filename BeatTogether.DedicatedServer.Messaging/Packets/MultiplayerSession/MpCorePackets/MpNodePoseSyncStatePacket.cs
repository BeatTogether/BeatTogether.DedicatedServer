﻿using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets
{
    public sealed class MpNodePoseSyncStatePacket : INetSerializable
    {
        public long deltaUpdateFrequency;
        public long fullStateUpdateFrequency;

        public void WriteTo(ref SpanBuffer bufferWriter)
        {
            bufferWriter.WriteVarLong(deltaUpdateFrequency);
            bufferWriter.WriteVarLong(fullStateUpdateFrequency);
        }
        public void ReadFrom(ref SpanBuffer bufferReader)
        {
            deltaUpdateFrequency = bufferReader.ReadVarLong();
            fullStateUpdateFrequency = bufferReader.ReadVarLong();
        }
    }

}
