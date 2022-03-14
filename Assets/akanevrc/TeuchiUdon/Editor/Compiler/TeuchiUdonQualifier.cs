using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonQualifier : ITeuchiUdonTypeArg, IEquatable<TeuchiUdonQualifier>
    {
        public static TeuchiUdonQualifier Top { get; } = new TeuchiUdonQualifier(Array.Empty<TeuchiUdonScope>(), Array.Empty<TeuchiUdonScope>());

        public TeuchiUdonScope[] Logical { get; }
        public TeuchiUdonScope[] Real { get; }

        public TeuchiUdonQualifier(IEnumerable<TeuchiUdonScope> logical)
            : this(logical, null)
        {
        }

        public TeuchiUdonQualifier(IEnumerable<TeuchiUdonScope> logical, IEnumerable<TeuchiUdonScope> real)
        {
            Logical = logical?.ToArray();
            Real    = real   ?.ToArray();
        }

        public bool Equals(TeuchiUdonQualifier obj)
        {
            return !object.ReferenceEquals(obj, null) && Logical.SequenceEqual(obj.Logical);
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
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonQualifier obj1, TeuchiUdonQualifier obj2)
        {
            return !(obj1 == obj2);
        }

        public override string ToString()
        {
            return string.Join<TeuchiUdonScope>(".", Logical);
        }

        public string GetLogicalName()
        {
            return string.Join<TeuchiUdonScope>("", Logical);
        }

        public string Qualify(string separator, string text)
        {
            return string.Join(separator, Logical.Select(x => x.ToString()).Concat(new string[] { text }));
        }

        public TeuchiUdonQualifier Append(TeuchiUdonScope logical)
        {
            var l = logical == null ? Logical : Logical.Concat(new TeuchiUdonScope[] { logical });
            return new TeuchiUdonQualifier(l);
        }

        public TeuchiUdonQualifier Append(TeuchiUdonScope logical, TeuchiUdonScope real)
        {
            var l = logical == null ? Logical : Logical.Concat(new TeuchiUdonScope[] { logical });
            var r = real    == null ? Real    : Real   .Concat(new TeuchiUdonScope[] { real    });
            return new TeuchiUdonQualifier(l, r);
        }

        public TeuchiUdonScope LastScope(TeuchiUdonScopeMode mode)
        {
            foreach (var scope in Logical.Reverse())
            {
                if (scope.Mode == mode) return scope;
            }
            return null;
        }

        public TeuchiUdonQualifier GetFuncQualifier()
        {
            var func = (TeuchiUdonFunc)LastScope(TeuchiUdonScopeMode.Func)?.Label;
            return func == null ? TeuchiUdonQualifier.Top : func.Qualifier;
        }

        public T GetLast<T>() where T : class, ITeuchiUdonLabel
        {
            return Logical.LastOrDefault()?.Label as T;
        }
    }
}
