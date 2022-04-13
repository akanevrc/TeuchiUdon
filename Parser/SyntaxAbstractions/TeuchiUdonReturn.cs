using System;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonReturn : IIndexedLabel, IDataLabel, IEquatable<TeuchiUdonReturn>
    {
        public int Index { get; }
        public TeuchiUdonFunc Func { get; }
        public TeuchiUdonType Type { get; } = PrimitiveTypes.Instance.UInt;

        public TeuchiUdonReturn(int index, TeuchiUdonFunc func)
        {
            Index = index;
            Func  = func;
        }

        public bool Equals(TeuchiUdonReturn obj)
        {
            return !object.ReferenceEquals(obj, null) && Index == obj.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonReturn ret ? Equals(ret) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonReturn obj1, TeuchiUdonReturn obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonReturn obj1, TeuchiUdonReturn obj2)
        {
            return !(obj1 == obj2);
        }

        public string GetLabel()
        {
            return $"return[{Func.GetLabel()}]";
        }

        public string GetFullLabel()
        {
            return $"return[{Func.GetFullLabel()}]";
        }
    }
}
