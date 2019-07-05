// Original Work Copyright (c) Ethan Moffat 2014-2019

namespace NetworkEngine.PacketCompiler.Parser
{
    public enum ValidationResult
    {
        Ok,
        SchemaError,
        InvalidRootElement,
        ElementRedefinition
    }
}