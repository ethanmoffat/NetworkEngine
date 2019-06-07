// Original Work Copyright (c) Ethan Moffat 2014-2019

using System;

namespace NetworkEngine.PacketCompiler
{
    [Flags]
    public enum ParseOptions
    {
        None = 0x00,
        SkipSchemaValidation = 0x01
    }
}
