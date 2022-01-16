using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonFunc : IEquatable<TeuchiUdonFunc>
    {
        public int Index { get; }
        public TeuchiUdonQualifier Qualifier { get; }
        public string Name { get; }
        public TeuchiUdonVar[] Vars { get; }
        public ExprResult Expr { get; }

        public TeuchiUdonFunc(int index)
        {
            Index     = index;
            Qualifier = null;
            Name      = null;
            Vars      = null;
            Expr      = null;
        }

        public TeuchiUdonFunc(TeuchiUdonQualifier qualifier, string name)
        {
            Index     = -1;
            Qualifier = qualifier;
            Name      = name;
            Vars      = null;
            Expr      = null;
        }

        public TeuchiUdonFunc(int index, TeuchiUdonQualifier qualifier, string name, IEnumerable<TeuchiUdonVar> vars, ExprResult expr)
        {
            Index     = index;
            Qualifier = qualifier;
            Name      = name;
            Vars      = vars.ToArray();
            Expr      = expr;
        }

        public bool Equals(TeuchiUdonFunc obj)
        {
            return Qualifier == obj.Qualifier && Name == obj.Name;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonFunc func ? Equals(func) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonFunc obj1, TeuchiUdonFunc obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonFunc obj1, TeuchiUdonFunc obj2)
        {
            return !obj1.Equals(obj2);
        }

        public override string ToString()
        {
            return $"{Qualifier.QualifiedName(Name)}({string.Join(", ", Vars.Select(x => x.Type))})";
        }

        public string GetUdonName()
        {
            return $"func[{Index}]";
        }
    }
}
