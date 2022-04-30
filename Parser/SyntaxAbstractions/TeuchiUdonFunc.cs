using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonFunc : IIndexedLabel, ICodeLabel, IEquatable<TeuchiUdonFunc>
    {
        public int Index { get; }
        public TeuchiUdonQualifier Qualifier { get; }
        public TeuchiUdonType Type { get; }
        public TeuchiUdonVar[] Vars { get; }
        public ExprResult Expr { get; }
        public TeuchiUdonReturn Return { get; }
        public bool Deterministic { get; }

        public TeuchiUdonFunc(int index, TeuchiUdonQualifier qualifier)
            : this(index, qualifier, null, null, null, false, null)
        {
        }

        public TeuchiUdonFunc
        (
            int index,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonType type,
            IEnumerable<TeuchiUdonVar> vars,
            ExprResult expr,
            bool deterministic,
            TeuchiUdonType addressType
        )
        {
            Index         = index;
            Qualifier     = qualifier;
            Type          = type;
            Vars          = vars?.ToArray();
            Expr          = expr;
            Return        = new TeuchiUdonReturn(index, this, addressType);
            Deterministic = deterministic;
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
    }
}
