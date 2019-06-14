// Original Work Copyright (c) Ethan Moffat 2014-2019

using System.Collections.Generic;

namespace NetworkEngine.PacketCompiler.State
{
    public class PacketState
    {
        public string PacketName { get; }

        public string BasePacketName { get; }

        public IReadOnlyList<PacketDataElement> Data { get; }

        private PacketState(string packetName,
                            string basePacketName,
                            IReadOnlyList<PacketDataElement> data)
        {
            PacketName = packetName;
            BasePacketName = basePacketName;
            Data = data;
        }

        public PacketState WithData(PacketDataElement dataElement)
        {
            var list = new List<PacketDataElement>(Data) { dataElement };
            return new PacketState(PacketName, BasePacketName, list);
        }

        public PacketState WithBasePacket(string basePacketName)
        {
            return new PacketState(PacketName, basePacketName, Data);
        }

        public static PacketState Create(string packetName)
        {
            return new PacketState(packetName, string.Empty, new List<PacketDataElement>());
        }
    }
}