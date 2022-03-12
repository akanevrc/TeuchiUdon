using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonFunc : ITypedLabel, IEquatable<TeuchiUdonFunc>
    {
        public int Index { get; }
        public TeuchiUdonQualifier Qualifier { get; }
        public TeuchiUdonType Type { get; }
        public TeuchiUdonVar[] Vars { get; }
        public ExprResult Expr { get; }
        public TeuchiUdonReturn Return { get; }

        public TeuchiUdonFunc(int index, TeuchiUdonQualifier qualifier)
            : this(index, qualifier, null, null, null)
        {
        }

        public TeuchiUdonFunc(int index, TeuchiUdonQualifier qualifier, TeuchiUdonType type, IEnumerable<TeuchiUdonVar> vars, ExprResult expr)
        {
            Index     = index;
            Qualifier = qualifier;
            Type      = type;
            Vars      = vars?.ToArray();
            Expr      = expr;
            Return    = new TeuchiUdonReturn(index, this);
        }

        public bool Equals(TeuchiUdonFunc obj)
        {
            return !object.ReferenceEquals(obj, null) && Index == obj.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonFunc func ? Equals(func) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonFunc obj1, TeuchiUdonFunc obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonFunc obj1, TeuchiUdonFunc obj2)
        {
            return !(obj1 == obj2);
        }

        public override string ToString()
        {
            return Qualifier.Qualify(".", $"func[{Index}]({string.Join(", ", Vars.Select(x => x.Type))})");
        }

        public string GetLabel()
        {
            return $"func[{Index}]";
        }

        public string GetFullLabel()
        {
            return $"func[{Qualifier.Qualify(">", Index.ToString())}]";
        }
    }
}
