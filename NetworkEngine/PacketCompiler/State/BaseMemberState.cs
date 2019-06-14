using System.Collections.Generic;

namespace NetworkEngine.PacketCompiler.State
{
    public class BaseMemberState : IMemberState
    {
        private readonly Dictionary<MemberProperty, object> _memberData = new Dictionary<MemberProperty, object>();

        public IReadOnlyList<PacketDataElement> Members { get; protected set; }

        public object this[MemberProperty property]
        {
            get
            {
                _memberData.TryGetValue(property, out var value);
                return value;
            }
            private set => _memberData[property] = value;
        }

        protected T GetMemberAs<T>(MemberProperty member) => (T) this[member];

        protected void SetMemberAs<T>(MemberProperty member, T value) => this[member] = value;
    }
}
