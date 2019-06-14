// Original Work Copyright (c) Ethan Moffat 2014-2019

namespace NetworkEngine.PacketCompiler.State
{
    public class PacketDataElement
    {
        public PacketDataType DataType { get; }
        public int Length { get; }
        public string Name { get; }
        public IMemberState MemberState { get; }

        public PacketDataElement(PacketDataType dataType, int length, string name, IMemberState memberState)
        {
            DataType = dataType;
            Length = length;
            Name = name;
            MemberState = memberState;
        }
    }
}