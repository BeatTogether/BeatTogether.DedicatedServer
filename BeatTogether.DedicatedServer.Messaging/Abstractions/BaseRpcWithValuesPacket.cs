﻿using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Abstractions
{
    // WARNING do not cast to ulong for any packets that inherit this class
    public abstract class BaseRpcWithValuesPacket : BaseRpcPacket
    {
        public byte HasValues { get; set; } = (1 | 2 | 4 | 8);

        public bool HasValue0
        {
            get => (HasValues & 1) != 0;
            set => HasValues |= (byte) (value ? 1 : 0);
        }

        public bool HasValue1
        {
            get => (HasValues & 2) != 0;
            set => HasValues |= (byte) (value ? 2 : 0);
        }

        public bool HasValue2
        {
            get => (HasValues & 4) != 0;
            set => HasValues |= (byte) (value ? 4 : 0);
        }

        public bool HasValue3
        {
            get => (HasValues & 8) != 0;
            set => HasValues |= (byte) (value ? 8 : 0);
        }

        public override void ReadFrom(ref SpanBuffer reader, Version version)
        {
            base.ReadFrom(ref reader, version);
            HasValues = reader.ReadUInt8();
        }

        public override void WriteTo(ref SpanBuffer writer, Version version)
        {
            base.WriteTo(ref writer, version);
            writer.WriteUInt8(HasValues);
        }
    }
}
