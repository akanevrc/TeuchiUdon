using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonQualifier : IEquatable<TeuchiUdonQualifier>
    {
        public static TeuchiUdonQualifier Top { get; } = new TeuchiUdonQualifier(new string[0], new string[0]);

        public string[] Logical { get; }
        public string[] Real { get; }

        public TeuchiUdonQualifier(IEnumerable<string> logical)
        {
            Logical = logical.ToArray();
            Real    = null;
        }

        public TeuchiUdonQualifier(IEnumerable<string> logical, IEnumerable<string> real)
        {
            Logical = logical.ToArray();
            Real    = real   .ToArray();
        }

        public bool Equals(TeuchiUdonQualifier obj)
        {
            return Logical.SequenceEqual(obj.Logical);
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonQualifier qual ? Equals(qual) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Logical.Length == 0 ? 0 : Logical[Logical.Length - 1].GetHashCode();
        }

        public static bool operator ==(TeuchiUdonQualifier obj1, TeuchiUdonQualifier obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonQualifier obj1, TeuchiUdonQualifier obj2)
        {
            return !obj1.Equals(obj2);
        }

        public override string ToString()
        {
            return string.Join(".", Logical);
        }

        public string QualifiedName(string name)
        {
            return string.Join(".", Logical.Concat(new string[] { name }));
        }

        public TeuchiUdonQualifier Append(string logical)
        {
            var l = logical == null ? Logical : Logical.Concat(new string[] { logical });
            return new TeuchiUdonQualifier(l);
        }

        public TeuchiUdonQualifier Append(string logical, string real)
        {
            var l = logical == null ? Logical : Logical.Concat(new string[] { logical });
            var r = real    == null ? Real    : Real   .Concat(new string[] { real    });
            return new TeuchiUdonQualifier(l, r);
        }
    }
}
