﻿// Original Work Copyright (c) Ethan Moffat 2014-2019

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetworkEngine.DataTransfer
{
    public class Packet : IPacket
    {
        private readonly INumberEncoder _numberEncoder;

        public Packet(IList<byte> data)
            : this(data.ToArray()) { }

        public Packet(IList<byte> data, INumberEncoder numberEncoder)
            : this(data.ToArray(), numberEncoder) { }

        public Packet(byte[] data)
            : this(data, new NumberEncoder()) { }

        public Packet(byte[] data, INumberEncoder numberEncoder)
        {
            ReadPosition = 0;
            RawData = new List<byte>(data);
            _numberEncoder = numberEncoder;
        }

        public int Length => RawData.Count;

        public int ReadPosition { get; private set; }

        public IReadOnlyList<byte> RawData { get; }

        public void Seek(int position, SeekOrigin origin)
        {
            int newPosition;
            switch (origin)
            {
                case SeekOrigin.Begin: newPosition = position; break;
                case SeekOrigin.Current: newPosition = ReadPosition + position; break;
                case SeekOrigin.End: newPosition = Length - 1 + position; break;
                default: throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            if (newPosition > Length)
                throw new ArgumentOutOfRangeException(nameof(position), "Position is out of bounds of the packet!");

            ReadPosition = newPosition;
        }

        public byte PeekByte()
        {
            ThrowIfOutOfBounds(0);
            return RawData[ReadPosition];
        }

        public byte PeekChar()
        {
            ThrowIfOutOfBounds(0);
            return (byte) _numberEncoder.DecodeNumber(RawData[ReadPosition]);
        }

        public short PeekShort()
        {
            ThrowIfOutOfBounds(1);

            var bytes = new[] {RawData[ReadPosition], RawData[ReadPosition + 1]};
            return (short) _numberEncoder.DecodeNumber(bytes);
        }

        public int PeekThree()
        {
            ThrowIfOutOfBounds(2);

            var bytes = new[]
            {
                RawData[ReadPosition],
                RawData[ReadPosition + 1],
                RawData[ReadPosition + 2]
            };
            return _numberEncoder.DecodeNumber(bytes);
        }

        public int PeekInt()
        {
            ThrowIfOutOfBounds(3);

            var bytes = new[]
            {
                RawData[ReadPosition],
                RawData[ReadPosition + 1],
                RawData[ReadPosition + 2],
                RawData[ReadPosition + 3]
            };
            return _numberEncoder.DecodeNumber(bytes);
        }

        public string PeekString(int length)
        {
            return Encoding.ASCII.GetString(PeekBytes(length).ToArray());
        }

        public string PeekEndString()
        {
            return PeekString(Length - ReadPosition);
        }

        public string PeekBreakString()
        {
            var ndx = ReadPosition;
            while (ndx < Length && RawData[ndx] != byte.MaxValue)
                ndx++;
            return PeekString(ndx - ReadPosition);
        }

        public IEnumerable<byte> PeekBytes(int length)
        {
            ThrowIfOutOfBounds(length);

            var bytes = new List<byte>(length);
            for (var i = ReadPosition; i < ReadPosition + length; ++i)
                bytes.Add(RawData[i]);
            return bytes;
        }

        public byte ReadByte()
        {
            var ret = PeekByte();
            ReadPosition += 1;
            return ret;
        }

        public byte ReadChar()
        {
            var ret = PeekChar();
            ReadPosition += 1;
            return ret;
        }

        public short ReadShort()
        {
            var ret = PeekShort();
            ReadPosition += 2;
            return ret;
        }

        public int ReadThree()
        {
            var ret = PeekThree();
            ReadPosition += 3;
            return ret;
        }

        public int ReadInt()
        {
            var ret = PeekInt();
            ReadPosition += 4;
            return ret;
        }

        public string ReadString(int length)
        {
            var ret = PeekString(length);
            ReadPosition += length;
            return ret;
        }

        public string ReadEndString()
        {
            return ReadString(Length - ReadPosition);
        }

        public string ReadBreakString()
        {
            var ret = PeekBreakString();
            ReadPosition = Math.Min(ReadPosition + ret.Length + 1, Length);
            return ret;
        }

        public IEnumerable<byte> ReadBytes(int length)
        {
            var ret = PeekBytes(length);
            ReadPosition += length;
            return ret;
        }

        private void ThrowIfOutOfBounds(int extraBytes)
        {
            if (ReadPosition + extraBytes > Length)
                throw new InvalidOperationException("Operation is out of bounds of the packet");
        }
    }
}
