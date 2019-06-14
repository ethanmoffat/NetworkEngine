using System.Collections.Generic;

namespace NetworkEngine.PacketCompiler.State
{
    public interface IMemberState
    {
        IReadOnlyList<PacketDataElement> Members { get; }

        object this[MemberProperty property] { get; }
    }

    public enum MemberProperty
    {
        Peek,
        Cases
    }
}
