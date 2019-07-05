// Original Work Copyright (c) Ethan Moffat 2014-2019

using System;

using NetworkEngine.PacketCompiler.Parser;

namespace NetworkEngine.PacketCompiler
{
    public class InvalidPacketSpecException : Exception
    {
        private readonly ValidationState _validationResult;

        public ValidationResult Result => _validationResult.Status;

        internal InvalidPacketSpecException(ValidationState validationResult)
        {
            _validationResult = validationResult;
        }
    }
}