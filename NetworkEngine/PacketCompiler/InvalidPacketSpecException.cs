// Original Work Copyright (c) Ethan Moffat 2014-2019

using System;

namespace NetworkEngine.PacketCompiler
{
    public class InvalidPacketSpecException : Exception
    {
        internal InvalidPacketSpecException(ValidationState validationResult)
        {
        }
    }
}