// Original Work Copyright (c) Ethan Moffat 2014-2019

namespace NetworkEngine.PacketCompiler
{
    public class PacketDataElement
    {
        public PacketDataType DataType { get; }
        public int Length { get; }
        public string Name { get; }

        public PacketDataElement(PacketDataType dataType, int length, string name)
        {
            DataType = dataType;
            Length = length;
            Name = name;
        }
    }
}