using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonLet : ITeuchiUdonLabel, IEquatable<TeuchiUdonLet>
    {
        public int Index { get; }

        public TeuchiUdonLet(int index)
        {
            Index = index;
        }

        public bool Equals(TeuchiUdonLet obj)
        {
            return !object.ReferenceEquals(obj, null) && Index == obj.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonLet let ? Equals(let) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonLet obj1, TeuchiUdonLet obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonLet obj1, TeuchiUdonLet obj2)
        {
            return !(obj1 == obj2);
        }

        public string GetLabel()
        {
            return $"let[{Index}]";
        }
    }
}
