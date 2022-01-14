using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonMethod : IEquatable<TeuchiUdonMethod>
    {
        public TeuchiUdonType Type { get; }
        public string Name { get; }
        public TeuchiUdonType[] InTypes { get; }
        public TeuchiUdonType[] OutTypes { get; }
        public TeuchiUdonType[] AllArgTypes { get; }
        public string UdonName { get; }

        public TeuchiUdonMethod
        (
            TeuchiUdonType type,
            string name,
            IEnumerable<TeuchiUdonType> inTypes
        )
        {
            Type         = type;
            Name         = name;
            InTypes      = inTypes.ToArray();
            OutTypes     = null;
            AllArgTypes  = null;
            UdonName     = null;
        }

        public TeuchiUdonMethod
        (
            TeuchiUdonType type,
            string name,
            IEnumerable<TeuchiUdonType> inTypes,
            IEnumerable<TeuchiUdonType> outTypes,
            IEnumerable<TeuchiUdonType> allArgTypes,
            string udonName
        )
        {
            Type         = type;
            Name         = name;
            InTypes      = inTypes    .ToArray();
            OutTypes     = outTypes   .ToArray();
            AllArgTypes  = allArgTypes.ToArray();
            UdonName     = udonName;
        }

        public bool Equals(TeuchiUdonMethod obj)
        {
            return Type == obj.Type && Name == obj.Name && InTypes.SequenceEqual(obj.InTypes);
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonMethod method ? Equals(method) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonMethod obj1, TeuchiUdonMethod obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonMethod obj1, TeuchiUdonMethod obj2)
        {
            return !obj1.Equals(obj2);
        }

        public override string ToString()
        {
            return $"{Type}.{Name}({string.Join<TeuchiUdonType>(", ", InTypes)})";
        }
    }
}
