// Original Work Copyright (c) Ethan Moffat 2014-2019

using System.Collections.Generic;

namespace NetworkEngine.PacketCompiler
{
    public class PacketState
    {
        public string PacketName { get; }

        public IReadOnlyList<PacketDataElement> Data { get; }

        private PacketState(string packetName, IReadOnlyList<PacketDataElement> data)
        {
            PacketName = packetName;
            Data = data;
        }

        public PacketState WithData(PacketDataElement dataElement)
        {
            var list = new List<PacketDataElement>(Data) { dataElement };
            return new PacketState(PacketName, list);
        }

        public static PacketState Create(string packetName)
        {
            return new PacketState(packetName, new List<PacketDataElement>());
        }
    }
}