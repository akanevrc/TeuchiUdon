using System;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonFor : IIndexedLabel, IEquatable<TeuchiUdonFor>
    {
        public int Index { get; }

        public TeuchiUdonFor(int index)
        {
            Index = index;
        }

        public bool Equals(TeuchiUdonFor obj)
        {
            return !object.ReferenceEquals(obj, null) && Index == obj.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonFor f ? Equals(f) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonFor obj1, TeuchiUdonFor obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonFor obj1, TeuchiUdonFor obj2)
        {
            return !(obj1 == obj2);
        }

        public string GetLabel()
        {
            return $"for[{Index}]";
        }

        public string GetFullLabel()
        {
            return $"for[{Index}]";
        }
    }
}
