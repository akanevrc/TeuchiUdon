using System;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonVar : IEquatable<TeuchiUdonVar>
    {
        public TeuchiUdonQualifier Qualifier { get; }
        public string Name { get; }
        public TeuchiUdonType Type { get; private set; }
        public ExprResult Expr { get; private set; }

        public TeuchiUdonVar(TeuchiUdonQualifier qualifier, string name)
        {
            Qualifier = qualifier;
            Name      = name;
            Type      = null;
            Expr      = null;
        }

        public TeuchiUdonVar(TeuchiUdonQualifier qualifier, string name, TeuchiUdonType type)
        {
            Qualifier = qualifier;
            Name      = name;
            Type      = type;
            Expr      = null;
        }

        public TeuchiUdonVar(TeuchiUdonQualifier qualifier, string name, TeuchiUdonType type, ExprResult expr)
        {
            Qualifier = qualifier;
            Name      = name;
            Type      = type;
            Expr      = expr;
        }

        public void SetExpr(ExprResult expr)
        {
            Type = expr.Inner.Type;
            Expr = expr;
        }

        public bool Equals(TeuchiUdonVar obj)
        {
            return Qualifier == obj.Qualifier && Name == obj.Name;
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
            return obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonVar obj1, TeuchiUdonVar obj2)
        {
            return !obj1.Equals(obj2);
        }

        public override string ToString()
        {
            return $"{Qualifier.QualifiedName(Name)}";
        }

        public string GetUdonName()
        {
            return $"var[{string.Join(">", Qualifier.Logical.Concat(new string[] { Name }))}]";
        }
    }
}
