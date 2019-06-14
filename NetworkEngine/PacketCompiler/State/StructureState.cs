using System.Collections.Generic;

namespace NetworkEngine.PacketCompiler.State
{
    public class StructureState : BaseMemberState
    {
        public StructureState(IReadOnlyList<PacketDataElement> members)
        {
            Members = members;
        }
    }
}
