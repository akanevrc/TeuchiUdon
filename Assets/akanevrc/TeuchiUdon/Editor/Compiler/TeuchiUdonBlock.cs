using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonBlock : ITeuchiUdonLabel, IEquatable<TeuchiUdonBlock>
    {
        public int Index { get; }
        public ITeuchiUdonLabel EndLabel { get; }

        public TeuchiUdonBlock(int index)
        {
            Index    = index;
            EndLabel = new TextLabel(GetLabel());
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

        public string GetLabel()
        {
            return $"block[{Index}]";
        }
    }
}
