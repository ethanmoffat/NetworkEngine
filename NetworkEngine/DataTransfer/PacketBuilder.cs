// Original Work Copyright (c) Ethan Moffat 2014-2019

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkEngine.DataTransfer
{
    public class PacketBuilder : IPacketBuilder
    {
        private readonly IReadOnlyList<byte> _data;
        private readonly INumberEncoder _numberEncoder;

        public PacketBuilder()
            : this(new NumberEncoder()) { }

        public PacketBuilder(INumberEncoder numberEncoder)
            : this(new List<byte>(), numberEncoder) { }

        private PacketBuilder(IReadOnlyList<byte> data, INumberEncoder numberEncoder)
        {
            _data = data;
            _numberEncoder = numberEncoder;
        }

        public int Length => _data.Count;

        public IPacketBuilder AddBreak()
        {
            return AddByte(byte.MaxValue);
        }

        public IPacketBuilder AddByte(byte b)
        {
            return AddBytes(new[] { b });
        }

        public IPacketBuilder AddChar(byte b)
        {
            return AddBytes(_numberEncoder.EncodeNumber(b, 1));
        }

        public IPacketBuilder AddShort(short s)
        {
            return AddBytes(_numberEncoder.EncodeNumber(s, 2));
        }

        public IPacketBuilder AddThree(int t)
        {
            return AddBytes(_numberEncoder.EncodeNumber(t, 3));
        }

        public IPacketBuilder AddInt(int i)
        {
            return AddBytes(_numberEncoder.EncodeNumber(i, 4));
        }

        public IPacketBuilder AddString(string s)
        {
            return AddBytes(Encoding.ASCII.GetBytes(s));
        }

        public IPacketBuilder AddBreakString(string s)
        {
            var sList = Encoding.ASCII.GetBytes(s).Concat(new[] { byte.MaxValue }).ToList();
            return AddBytes(sList);
        }

        public IPacketBuilder AddBytes(IEnumerable<byte> bytes)
        {
            var list = new List<byte>(_data);
            list.AddRange(bytes);
            return new PacketBuilder(list, _numberEncoder);
        }

        public IPacket Build()
        {
            return new Packet(_data.ToList());
        }
    }
}
