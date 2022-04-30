using System;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonThis : IIndexedLabel, IDataLabel, IEquatable<TeuchiUdonThis>
    {
        public int Index { get; } = 0;
        public TeuchiUdonType Type { get; }

        public TeuchiUdonThis(TeuchiUdonType type)
        {
            Type = type;
        }

        public bool Equals(TeuchiUdonThis obj)
        {
            return !object.ReferenceEquals(obj, null) && Type.LogicalTypeEquals(obj.Type);
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonThis ths ? Equals(ths) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonThis obj1, TeuchiUdonThis obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonThis obj1, TeuchiUdonThis obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
