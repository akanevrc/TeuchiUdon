using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonLetInBind : ITeuchiUdonLabel, IEquatable<TeuchiUdonLetInBind>
    {
        public int Index { get; }

        public TeuchiUdonLetInBind(int index)
        {
            Index = index;
        }

        public bool Equals(TeuchiUdonLetInBind obj)
        {
            return !object.ReferenceEquals(obj, null) && Index == obj.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonLetInBind letinbind ? Equals(letinbind) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonLetInBind obj1, TeuchiUdonLetInBind obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonLetInBind obj1, TeuchiUdonLetInBind obj2)
        {
            return !(obj1 == obj2);
        }

        public string GetLabel()
        {
            return $"letinbind[{Index}]";
        }
    }
}
