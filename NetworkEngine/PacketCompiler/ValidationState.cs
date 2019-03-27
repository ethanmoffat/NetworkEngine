// Original Work Copyright (c) Ethan Moffat 2014-2019

namespace NetworkEngine.PacketCompiler
{
    internal class ValidationState
    {
        public ValidationResult Status { get; }

        public ValidationState(ValidationResult status)
        {
            Status = status;
        }
    }
}