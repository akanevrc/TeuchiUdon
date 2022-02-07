using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonLetIn : ITeuchiUdonLabel, IEquatable<TeuchiUdonLetIn>
    {
        public int Index { get; }
        public TeuchiUdonQualifier Qualifier { get; }

        public TeuchiUdonLetIn(int index, TeuchiUdonQualifier qualifier)
        {
            Index     = index;
            Qualifier = qualifier;
        }

        public bool Equals(TeuchiUdonLetIn obj)
        {
            return !object.ReferenceEquals(obj, null) && Index == obj.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonLetIn letin ? Equals(letin) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonLetIn obj1, TeuchiUdonLetIn obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonLetIn obj1, TeuchiUdonLetIn obj2)
        {
            return !(obj1 == obj2);
        }

        public string GetLabel()
        {
            return $"let[{Index}]";
        }

        public string GetFullLabel()
        {
            return $"let[{Qualifier.Qualify(">", Index.ToString())}]";
        }
    }
}
