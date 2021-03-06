using System;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonVar : IIndexedLabel, IDataLabel, ITeuchiUdonLeftValue, IEquatable<TeuchiUdonVar>
    {
        public int Index { get; }
        public TeuchiUdonQualifier Qualifier { get; }
        public string Name { get; }
        public TeuchiUdonType Type { get; }
        public bool Mut { get; }
        public bool IsSystemVar { get; }

        public TeuchiUdonVar(TeuchiUdonQualifier qualifier, string name)
            : this(-1, qualifier, name, null, false, false)
        {
        }

        public TeuchiUdonVar
        (
            int index,
            TeuchiUdonQualifier qualifier,
            string name,
            TeuchiUdonType type,
            bool mut,
            bool isSystemVar
        )
        {
            Index       = index;
            Qualifier   = qualifier;
            Name        = name;
            Type        = type;
            Mut         = mut;
            IsSystemVar = isSystemVar;
        }

        public bool Equals(TeuchiUdonVar obj)
        {
            return
                !object.ReferenceEquals(obj, null) && Qualifier == obj.Qualifier && Name == obj.Name;
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
    }
}
