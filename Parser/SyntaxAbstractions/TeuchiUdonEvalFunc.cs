using System;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonEvalFunc : IIndexedLabel, ICodeLabel, IEquatable<TeuchiUdonEvalFunc>
    {
        public int Index { get; }
        public TeuchiUdonQualifier Qualifier { get; }

        public TeuchiUdonEvalFunc(int index, TeuchiUdonQualifier qualifier)
        {
            Index     = index;
            Qualifier = qualifier;
        }

        public bool Equals(TeuchiUdonEvalFunc obj)
        {
            return !object.ReferenceEquals(obj, null) && Index == obj.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonEvalFunc evalFunc ? Equals(evalFunc) : base.Equals(obj);
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
    }
}
