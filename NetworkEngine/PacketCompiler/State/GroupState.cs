// Original Work Copyright (c) Ethan Moffat 2014-2019

using System.Collections.Generic;

namespace NetworkEngine.PacketCompiler.State
{
    public class GroupState : BaseMemberState
    {
        public PacketDataType CountType
        {
            get => GetMemberAs<PacketDataType>(MemberProperty.CountType);
            internal set => SetMemberAs(MemberProperty.CountType, value);
        }

        public int BreakOn
        {
            get => GetMemberAs<int>(MemberProperty.BreakOn);
            internal set => SetMemberAs(MemberProperty.BreakOn, value);
        }

        public PacketDataType BreakType
        {
            get => GetMemberAs<PacketDataType>(MemberProperty.BreakType);
            internal set => SetMemberAs(MemberProperty.BreakType, value);
        }

        public bool Peek
        {
            get => GetMemberAs<bool>(MemberProperty.Peek);
            internal set => SetMemberAs(MemberProperty.Peek, value);
        }

        public PacketDataElement PreLoop => Members[0];

        public PacketDataElement PostLoop => Members[1];

        public PacketDataElement Structure => Members[2];

        public GroupState(PacketDataElement preLoopNode,
                          PacketDataElement postLoopNode,
                          PacketDataElement structureNode)
        {
            Members = new List<PacketDataElement>
            {
                preLoopNode,
                postLoopNode,
                structureNode
            };
        }
    }
}
