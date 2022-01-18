using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonFunc : ITeuchiUdonLabel, IEquatable<TeuchiUdonFunc>
    {
        public int Index { get; }
        public TeuchiUdonQualifier Qualifier { get; }
        public TextLabel Name { get; }
        public TeuchiUdonVar[] Vars { get; }
        public ExprResult Expr { get; }
        public TextLabel ReturnAddress { get; }

        public TeuchiUdonFunc(int index)
            : this(index, null, null, null, null)
        {
        }

        public TeuchiUdonFunc(TeuchiUdonQualifier qualifier, string name)
            : this(-1, qualifier, name, null, null)
        {
        }

        public TeuchiUdonFunc(int index, TeuchiUdonQualifier qualifier, string name, IEnumerable<TeuchiUdonVar> vars, ExprResult expr)
        {
            Index         = index;
            Qualifier     = qualifier;
            Name          = new TextLabel(name);
            Vars          = vars?.ToArray();
            Expr          = expr;
            ReturnAddress = new TextLabel($"{GetLabel()}>return");
        }

        public bool Equals(TeuchiUdonFunc obj)
        {
            return !object.ReferenceEquals(obj, null) && Qualifier == obj.Qualifier && Name == obj.Name;
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
            return $"{Qualifier.QualifyText(".", Name.ToString())}({string.Join(", ", Vars.Select(x => x.Type))})";
        }

        public string GetLabel()
        {
            return $"func[{Index}]";
        }
    }
}
