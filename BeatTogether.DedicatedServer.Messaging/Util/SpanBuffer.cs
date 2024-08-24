using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Util
{
    public ref struct SpanBuffer
    {
        private readonly bool _resize;

        private Span<byte> _buffer;

        private int _offset;
        public int Offset => _offset;
        public ReadOnlySpan<byte> ReadOnlyData => _buffer.Slice(0, _offset);
        public Span<byte> Data => _buffer.Slice(0, _offset);
        public int Size => _offset;
        public readonly int RemainingSize => _buffer.Length - _offset;
        public ReadOnlySpan<byte> RemainingData => _buffer.Slice(_offset);

        public SpanBuffer(Span<byte> buffer, bool resize = true)
        {
            _resize = resize;
            _buffer = buffer;
            _offset = 0;
        }

        public SpanBuffer(int size, bool resize = true)
        {
            _resize = resize;
            _buffer = new byte[size].AsSpan();
            _offset = 0;
        }
        public void SetOffset(int NewOffset)
        {
            _offset = NewOffset;
        }

        #region Writing To buffer

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Reserve(int length)
        {
            int num = _offset + length;
            if (num > _buffer.Length)
            {
                ResizeBuffer(num);
            }
        }

        private void ResizeBuffer(int neededLength)
        {
            if (!_resize)
            {
                throw new OutOfSpaceException(_buffer.Length, _offset, neededLength);
            }

            byte[] array = new byte[neededLength * 2];
            _buffer.CopyTo(array.AsSpan());
            _buffer = array.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBool(bool x)
        {
            Reserve(1);
            _buffer[_offset++] = (byte)(x ? 1 : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt8(sbyte x)
        {
            Reserve(1);
            _buffer[_offset++] = (byte)x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt8(byte x)
        {
            Reserve(1);
            _buffer[_offset++] = x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt16(short x)
        {
            Reserve(2);
            BinaryPrimitives.WriteInt16LittleEndian(_buffer.Slice(_offset), x);
            _offset += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt16(ushort x)
        {
            Reserve(2);
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Slice(_offset), x);
            _offset += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt32(int x)
        {
            Reserve(4);
            BinaryPrimitives.WriteInt32LittleEndian(_buffer.Slice(_offset), x);
            _offset += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt32(uint x)
        {
            Reserve(4);
            BinaryPrimitives.WriteUInt32LittleEndian(_buffer.Slice(_offset), x);
            _offset += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt64(ulong x)
        {
            Reserve(8);
            BinaryPrimitives.WriteUInt64LittleEndian(_buffer.Slice(_offset), x);
            _offset += 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt64(long x)
        {
            Reserve(8);
            BinaryPrimitives.WriteInt64LittleEndian(_buffer.Slice(_offset), x);
            _offset += 8;
        }

        public void WriteFloat32(float x)
        {
            if (!BitConverter.IsLittleEndian)
            {
                throw new NotImplementedException();
            }

            Reserve(4);
            MemoryMarshal.Write(_buffer.Slice(_offset), ref x);
            _offset += 4;
        }

        public void WriteFloat64(double x)
        {
            if (!BitConverter.IsLittleEndian)
            {
                throw new NotImplementedException();
            }

            Reserve(8);
            MemoryMarshal.Write(_buffer.Slice(_offset), ref x);
            _offset += 8;
        }

        public void WriteGuid(Guid guid)
        {
            Reserve(16);
            guid.TryWriteBytes(_buffer.Slice(_offset));
            _offset += 16;
        }

        public void WriteString(string str, Encoding encoding)
        {
            int byteCount = encoding.GetByteCount(str);
            Reserve(byteCount + 2);
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Slice(_offset), (ushort)byteCount);
            _offset += 2;
            Span<byte> bytes = _buffer.Slice(_offset, byteCount);
            encoding.GetBytes(str.AsSpan(), bytes);
            _offset += byteCount;
        }

        public void WriteUTF8String(string str)
        {
            WriteString(str, Encoding.UTF8);
        }

        public void WriteUTF16String(string str)
        {
            WriteString(str, Encoding.Unicode);
        }

        public void WriteBytes(ReadOnlySpan<byte> x)
        {
            Reserve(x.Length);
            x.CopyTo(_buffer.Slice(_offset));
            _offset += x.Length;
        }
        public void PadBytes(int n)
        {
            Reserve(n);
            _offset += n;
        }

        public void Dispose()
        {
            _buffer = Span<byte>.Empty;
            _offset = 0;
        }

        public static implicit operator ReadOnlySpan<byte>(SpanBuffer buffer)
        {
            return buffer.ReadOnlyData;
        }
        #endregion

        #region ReadingFromBuffer

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfEndOfBuffer(int neededSize)
        {
            if (_offset + neededSize > _buffer.Length)
            {
                throw new EndOfBufferException(_buffer.Length, _offset, neededSize);
            }
        }

        public Span<byte> ReadBytes(int count)
        {
            ThrowIfEndOfBuffer(count);
            Span<byte> result = _buffer.Slice(_offset, count);
            _offset += count;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            ThrowIfEndOfBuffer(1);
            return _buffer[_offset++];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadUInt8()
        {
            return ReadByte();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadInt8()
        {
            return (sbyte)ReadByte();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBool()
        {
            if (ReadUInt8() != 1)
            {
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16()
        {
            ThrowIfEndOfBuffer(2);
            ushort result = BinaryPrimitives.ReadUInt16LittleEndian(_buffer.Slice(_offset));
            _offset += 2;
            return result;
        }

        public Span<ushort> ReadUInt16Slice(int count)
        {
            ThrowIfEndOfBuffer(2 * count);
            Span<ushort> readOnlySpan = MemoryMarshal.Cast<byte, ushort>(_buffer.Slice(_offset));
            _offset += 2 * count;
            if (BitConverter.IsLittleEndian)
            {
                return readOnlySpan.Slice(0, count);
            }

            ushort[] array = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = BinaryPrimitives.ReverseEndianness(readOnlySpan[i]);
            }

            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16()
        {
            ThrowIfEndOfBuffer(2);
            short result = BinaryPrimitives.ReadInt16LittleEndian(_buffer.Slice(_offset));
            _offset += 2;
            return result;
        }

        public ReadOnlySpan<short> ReadInt16Slice(int count)
        {
            ThrowIfEndOfBuffer(2 * count);
            ReadOnlySpan<short> readOnlySpan = MemoryMarshal.Cast<byte, short>(_buffer.Slice(_offset));
            _offset += 2 * count;
            if (BitConverter.IsLittleEndian)
            {
                return readOnlySpan.Slice(0, count);
            }

            short[] array = new short[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = BinaryPrimitives.ReverseEndianness(readOnlySpan[i]);
            }

            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32()
        {
            ThrowIfEndOfBuffer(4);
            uint result = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.Slice(_offset));
            _offset += 4;
            return result;
        }

        public ReadOnlySpan<uint> ReadUInt32Slice(int count)
        {
            ThrowIfEndOfBuffer(4 * count);
            ReadOnlySpan<uint> readOnlySpan = MemoryMarshal.Cast<byte, uint>(_buffer.Slice(_offset));
            _offset += 4 * count;
            if (BitConverter.IsLittleEndian)
            {
                return readOnlySpan.Slice(0, count);
            }

            uint[] array = new uint[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = BinaryPrimitives.ReverseEndianness(readOnlySpan[i]);
            }

            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32()
        {
            ThrowIfEndOfBuffer(4);
            int result = BinaryPrimitives.ReadInt32LittleEndian(_buffer.Slice(_offset));
            _offset += 4;
            return result;
        }

        public ReadOnlySpan<int> ReadInt32Slice(int count)
        {
            ThrowIfEndOfBuffer(4 * count);
            ReadOnlySpan<int> readOnlySpan = MemoryMarshal.Cast<byte, int>(_buffer.Slice(_offset));
            _offset += 4 * count;
            if (BitConverter.IsLittleEndian)
            {
                return readOnlySpan.Slice(0, count);
            }

            int[] array = new int[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = BinaryPrimitives.ReverseEndianness(readOnlySpan[i]);
            }

            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            ThrowIfEndOfBuffer(8);
            ulong result = BinaryPrimitives.ReadUInt64LittleEndian(_buffer.Slice(_offset));
            _offset += 8;
            return result;
        }

        public ReadOnlySpan<ulong> ReadUInt64Slice(int count)
        {
            ThrowIfEndOfBuffer(8 * count);
            ReadOnlySpan<ulong> readOnlySpan = MemoryMarshal.Cast<byte, ulong>(_buffer.Slice(_offset));
            _offset += 8 * count;
            if (BitConverter.IsLittleEndian)
            {
                return readOnlySpan.Slice(0, count);
            }

            ulong[] array = new ulong[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = BinaryPrimitives.ReverseEndianness(readOnlySpan[i]);
            }

            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64()
        {
            ThrowIfEndOfBuffer(8);
            long result = BinaryPrimitives.ReadInt64LittleEndian(_buffer.Slice(_offset));
            _offset += 8;
            return result;
        }

        public ReadOnlySpan<long> ReadInt64Slice(int count)
        {
            ThrowIfEndOfBuffer(8 * count);
            ReadOnlySpan<long> readOnlySpan = MemoryMarshal.Cast<byte, long>(_buffer.Slice(_offset));
            _offset += 8 * count;
            if (BitConverter.IsLittleEndian)
            {
                return readOnlySpan.Slice(0, count);
            }

            long[] array = new long[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = BinaryPrimitives.ReverseEndianness(readOnlySpan[i]);
            }

            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloat32()
        {
            if (!BitConverter.IsLittleEndian)
            {
                throw new NotImplementedException();
            }

            ThrowIfEndOfBuffer(4);
            float result = MemoryMarshal.Read<float>(_buffer.Slice(_offset));
            _offset += 4;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadFloat64()
        {
            if (!BitConverter.IsLittleEndian)
            {
                throw new NotImplementedException();
            }

            ThrowIfEndOfBuffer(8);
            double result = MemoryMarshal.Read<double>(_buffer.Slice(_offset));
            _offset += 8;
            return result;
        }

        public Guid ReadGuid()
        {
            ThrowIfEndOfBuffer(16);
            Guid result = new Guid(_buffer.Slice(_offset, 16));
            _offset += 16;
            return result;
        }

        public string ReadString(Encoding encoding)
        {
            ushort count = ReadUInt16();
            ReadOnlySpan<byte> bytes = ReadBytes(count);
            return encoding.GetString(bytes);
        }

        public string ReadUTF8String()
        {
            return ReadString(Encoding.UTF8);
        }

        public string ReadUTF16String()
        {
            return ReadString(Encoding.Unicode);
        }

        public void SkipBytes(int count)
        {
            ThrowIfEndOfBuffer(count);
            _offset += count;
        }
        #endregion
    }
}