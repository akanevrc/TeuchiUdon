using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonBlock : IEquatable<TeuchiUdonBlock>
    {
        public int Index { get; }

        public TeuchiUdonBlock(int index)
        {
            Index = index;
        }

        public bool Equals(TeuchiUdonBlock obj)
        {
            return Index == obj.Index;
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
            return obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonBlock obj1, TeuchiUdonBlock obj2)
        {
            return !obj1.Equals(obj2);
        }

        public override string ToString()
        {
            return $"block[{Index}]";
        }

        public string GetUdonName()
        {
            return $"block[{Index}]";
        }
    }
}
