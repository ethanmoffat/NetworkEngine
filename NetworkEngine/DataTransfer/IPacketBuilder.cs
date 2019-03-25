// Original Work Copyright (c) Ethan Moffat 2014-2019
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;

namespace NetworkEngine.DataTransfer
{
    public interface IPacketBuilder
    {
        int Length { get; }

        IPacketBuilder AddBreak();

        IPacketBuilder AddByte(byte b);

        IPacketBuilder AddChar(byte b);

        IPacketBuilder AddShort(short s);

        IPacketBuilder AddThree(int t);

        IPacketBuilder AddInt(int i);

        IPacketBuilder AddString(string s);

        IPacketBuilder AddBreakString(string s);

        IPacketBuilder AddBytes(IEnumerable<byte> bytes);

        IPacket Build();
    }
}
