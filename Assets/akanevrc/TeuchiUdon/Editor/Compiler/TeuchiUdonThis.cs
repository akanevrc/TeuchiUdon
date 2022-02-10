using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonThis : ITeuchiUdonLabel, IEquatable<TeuchiUdonThis>
    {
        public TeuchiUdonType Type { get; }

        public TeuchiUdonThis()
        {
            Type = TeuchiUdonType.GameObject;
        }

        public bool Equals(TeuchiUdonThis obj)
        {
            return !object.ReferenceEquals(obj, null) && Type == obj.Type;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonThis ths ? Equals(ths) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonThis obj1, TeuchiUdonThis obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonThis obj1, TeuchiUdonThis obj2)
        {
            return !(obj1 == obj2);
        }

        public override string ToString()
        {
            return "this";
        }

        public string GetLabel()
        {
            return "literal[this]";
        }

        public string GetFullLabel()
        {
            return "literal[this]";
        }
    }
}
