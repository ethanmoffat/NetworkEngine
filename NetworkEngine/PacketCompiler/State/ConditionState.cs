using System.Collections.Generic;

namespace NetworkEngine.PacketCompiler.State
{
    public class ConditionState : BaseMemberState
    {
        public bool Peek => GetMemberAs<bool>(MemberProperty.Peek);

        public IReadOnlyList<CaseState> Cases => GetMemberAs<IReadOnlyList<CaseState>>(MemberProperty.Cases);

        public ConditionState(bool peek, PacketDataElement testMember, IReadOnlyList<CaseState> cases)
        {
            Members = new List<PacketDataElement> { testMember };
            SetMemberAs(MemberProperty.Peek, peek);
            SetMemberAs(MemberProperty.Cases, cases);
        }

        public class CaseState : BaseMemberState
        {
            public string TestValue { get; }

            public CaseState(string testValue, IReadOnlyList<PacketDataElement> members)
            {
                TestValue = testValue;
                Members = members;
            }
        }
    }
}
