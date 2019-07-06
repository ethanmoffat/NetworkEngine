// Original Work Copyright (c) Ethan Moffat 2014-2019

using NetworkEngine.PacketCompiler.State;

namespace NetworkEngine.PacketCompiler.Parser
{
    public interface IPacketSpecParser
    {
        PacketState Parse(ParseOptions options = ParseOptions.None);
    }
}