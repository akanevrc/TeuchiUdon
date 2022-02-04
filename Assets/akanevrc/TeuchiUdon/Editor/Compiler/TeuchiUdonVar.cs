using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonVar : ITeuchiUdonLabel, IEquatable<TeuchiUdonVar>
    {
        public TeuchiUdonQualifier Qualifier { get; }
        public string Name { get; }
        public TeuchiUdonType Type { get; }

        public TeuchiUdonVar(TeuchiUdonQualifier qualifier, string name)
            : this(qualifier, name, null)
        {
        }

        public TeuchiUdonVar(TeuchiUdonQualifier qualifier, string name, TeuchiUdonType type)
        {
            Qualifier = qualifier;
            Name      = name;
            Type      = type;
        }

        public bool Equals(TeuchiUdonVar obj)
        {
            return !object.ReferenceEquals(obj, null) && Qualifier == obj.Qualifier && Name == obj.Name;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonVar v ? Equals(v) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonVar obj1, TeuchiUdonVar obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonVar obj1, TeuchiUdonVar obj2)
        {
            return !(obj1 == obj2);
        }

        public override string ToString()
        {
            return Qualifier.Qualify(".", Name);
        }

        public string GetLabel()
        {
            return Qualifier == TeuchiUdonQualifier.Top ? Name : $"var[{Name}]";
        }

        public string GetFullLabel()
        {
            return Qualifier == TeuchiUdonQualifier.Top ? Name : $"var[{Qualifier.Qualify(">", Name)}]";
        }
    }
}
