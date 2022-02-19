using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Abstractions
{
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

        public override void ReadFrom(ref SpanBufferReader reader)
        {
            base.ReadFrom(ref reader);
            HasValues = reader.ReadUInt8();
        }

        public override void WriteTo(ref SpanBufferWriter writer)
        {
            base.WriteTo(ref writer);
            writer.WriteUInt8(HasValues);
        }
    }
}
