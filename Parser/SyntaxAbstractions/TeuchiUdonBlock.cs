using System;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonBlock : IIndexedLabel, IEquatable<TeuchiUdonBlock>
    {
        public int Index { get; }
        public TeuchiUdonQualifier Qualifier { get; }
        public TeuchiUdonType Type { get; }
        public ITeuchiUdonLabel Return { get; set; } = null;
        public ITeuchiUdonLabel Continue { get; set; } = null;
        public ITeuchiUdonLabel Break { get; set; } = null;

        public TeuchiUdonBlock(int index, TeuchiUdonQualifier qualifier)
            : this(index, qualifier, null)
        {
        }

        public TeuchiUdonBlock(int index, TeuchiUdonQualifier qualifier, TeuchiUdonType type)
        {
            Index     = index;
            Qualifier = qualifier;
            Type      = type;
        }

        public bool Equals(TeuchiUdonBlock obj)
        {
            return !object.ReferenceEquals(obj, null) && Index == obj.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonBlock block ? Equals(block) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonBlock obj1, TeuchiUdonBlock obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonBlock obj1, TeuchiUdonBlock obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
