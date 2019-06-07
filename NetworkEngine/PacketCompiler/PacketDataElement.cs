// Original Work Copyright (c) Ethan Moffat 2014-2019

using System.Collections.Generic;

namespace NetworkEngine.PacketCompiler
{
    public class PacketDataElement
    {
        public PacketDataType DataType { get; }
        public int Length { get; }
        public string Name { get; }
        public IReadOnlyList<PacketDataElement> Members { get; }

        public PacketDataElement(PacketDataType dataType, int length, string name, IReadOnlyList<PacketDataElement> members)
        {
            DataType = dataType;
            Length = length;
            Name = name;
            Members = members;
        }
    }
}