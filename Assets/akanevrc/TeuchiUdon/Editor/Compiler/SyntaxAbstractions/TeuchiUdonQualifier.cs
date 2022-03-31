using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonQualifier : ITeuchiUdonTypeArg, IEquatable<TeuchiUdonQualifier>
    {
        public static TeuchiUdonQualifier Top { get; } = new TeuchiUdonQualifier(Array.Empty<TeuchiUdonScope>());

        public TeuchiUdonScope[] Logical { get; }

        public TeuchiUdonQualifier(IEnumerable<TeuchiUdonScope> logical)
        {
            Logical = logical?.ToArray();
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
            return new TeuchiUdonQualifier(l);
        }

        public TeuchiUdonScope LastScope(IEnumerable<TeuchiUdonScopeMode> modes)
        {
            foreach (var scope in Logical.Reverse())
            {
                if (modes.Any(x => x == scope.Mode)) return scope;
            }
            return null;
        }

        public TeuchiUdonQualifier GetFuncQualifier()
        {
            var func = (TeuchiUdonFunc)LastScope(new TeuchiUdonScopeMode[] { TeuchiUdonScopeMode.Func })?.Label;
            return func == null ? TeuchiUdonQualifier.Top : func.Qualifier;
        }

        public TeuchiUdonBlock GetFuncBlock()
        {
            var scope = LastScope(new TeuchiUdonScopeMode[] { TeuchiUdonScopeMode.Func, TeuchiUdonScopeMode.FuncBlock })?.Label;
            return scope != null && scope is TeuchiUdonBlock block ? block : null;
        }

        public TeuchiUdonBlock GetLoopBlock()
        {
            var scope = LastScope(new TeuchiUdonScopeMode[] { TeuchiUdonScopeMode.Func, TeuchiUdonScopeMode.LoopBlock })?.Label;
            return scope != null && scope is TeuchiUdonBlock block ? block : null;
        }

        public T GetLast<T>() where T : class, ITeuchiUdonLabel
        {
            return Logical.LastOrDefault()?.Label as T;
        }
    }
}
