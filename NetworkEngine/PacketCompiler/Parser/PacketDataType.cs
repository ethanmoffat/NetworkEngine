// Original Work Copyright (c) Ethan Moffat 2014-2019

namespace NetworkEngine.PacketCompiler.Parser
{
    public enum PacketDataType
    {
        Byte,
        Char,
        Short,
        Three,
        Int,
        String,
        EndString,
        BreakString,
        Structure,
        Condition,
        Group
    }
}