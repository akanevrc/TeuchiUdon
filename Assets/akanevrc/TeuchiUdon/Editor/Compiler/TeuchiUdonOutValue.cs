using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonOutValue : IIndexedLabel, IEquatable<TeuchiUdonOutValue>
    {
        public TeuchiUdonQualifier Qualifier { get; }
        public int Index { get; }
        public TeuchiUdonType Type { get; }

        public TeuchiUdonOutValue(TeuchiUdonQualifier qualifier, int index)
        {
            Qualifier = qualifier;
            Index     = index;
            Type      = TeuchiUdonType.Any;
        }

        public bool Equals(TeuchiUdonOutValue obj)
        {
            return !object.ReferenceEquals(obj, null) && Qualifier == obj.Qualifier && Index == obj.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonOutValue o ? Equals(o) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonOutValue obj1, TeuchiUdonOutValue obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonOutValue obj1, TeuchiUdonOutValue obj2)
        {
            return !(obj1 == obj2);
        }

        public string GetLabel()
        {
            return $"out[{Index}]";
        }

        public string GetFullLabel()
        {
            return $"out[{Qualifier.Qualify(">", Index.ToString())}]";
        }
    }
}
