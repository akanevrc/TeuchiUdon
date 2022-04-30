using System;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonLoop : IIndexedLabel, ICodeLabel, IEquatable<TeuchiUdonLoop>
    {
        public int Index { get; }

        public TeuchiUdonLoop(int index)
        {
            Index = index;
        }

        public bool Equals(TeuchiUdonLoop obj)
        {
            return !object.ReferenceEquals(obj, null) && Index == obj.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonLoop loop ? Equals(loop) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonLoop obj1, TeuchiUdonLoop obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonLoop obj1, TeuchiUdonLoop obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
