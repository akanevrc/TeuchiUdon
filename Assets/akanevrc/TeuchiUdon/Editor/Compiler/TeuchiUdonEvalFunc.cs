using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonEvalFunc : ITeuchiUdonLabel, IEquatable<TeuchiUdonEvalFunc>
    {
        public int Index { get; }
        public TeuchiUdonQualifier Qualifier { get; }
        public ITeuchiUdonLabel EndLabel { get; }

        public TeuchiUdonEvalFunc(int index, TeuchiUdonQualifier qualifier)
        {
            Index     = index;
            Qualifier = qualifier;
            EndLabel  = new TextLabel(qualifier, GetLabel());
        }

        public bool Equals(TeuchiUdonEvalFunc obj)
        {
            return !object.ReferenceEquals(obj, null) && Index == obj.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonEvalFunc block ? Equals(block) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonEvalFunc obj1, TeuchiUdonEvalFunc obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonEvalFunc obj1, TeuchiUdonEvalFunc obj2)
        {
            return !(obj1 == obj2);
        }

        public string GetLabel()
        {
            return $"evalfunc[{Index}]";
        }

        public string GetFullLabel()
        {
            return $"evalfunc[{Qualifier.Qualify(">", Index.ToString())}]";
        }
    }
}
