using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonOutValue : ITeuchiUdonLabel, IEquatable<TeuchiUdonOutValue>
    {
        public TeuchiUdonType Type { get; }
        public int Index { get; }

        public TeuchiUdonOutValue(TeuchiUdonType type, int index)
        {
            Type  = type;
            Index = index;
        }

        public bool Equals(TeuchiUdonOutValue obj)
        {
            return !object.ReferenceEquals(obj, null) && Type == obj.Type && Index == obj.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonOutValue o ? Equals(o) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonOutValue obj1, TeuchiUdonOutValue obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonOutValue obj1, TeuchiUdonOutValue obj2)
        {
            return !(obj1 == obj2);
        }

        public string GetLabel()
        {
            return $"out[{Type.LogicalName}>{Index}]";
        }

        public string GetFullLabel()
        {
            return $"out[{Type.LogicalName}>{Index}]";
        }
    }
}
