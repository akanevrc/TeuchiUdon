using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonIndirect : ITypedLabel, IEquatable<TeuchiUdonIndirect>
    {
        public int Index { get; }
        public ITeuchiUdonLabel Label { get; }
        public TeuchiUdonType Type { get; } = TeuchiUdonType.UInt;

        public TeuchiUdonIndirect(int index, ITeuchiUdonLabel label)
        {
            Index = index;
            Label = label;
        }

        public bool Equals(TeuchiUdonIndirect obj)
        {
            return !object.ReferenceEquals(obj, null) && Label == obj.Label;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonIndirect indirect ? Equals(indirect) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Label.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonIndirect obj1, TeuchiUdonIndirect obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonIndirect obj1, TeuchiUdonIndirect obj2)
        {
            return !(obj1 == obj2);
        }

        public string GetLabel()
        {
            return $"indirect[{Label.GetLabel()}]";
        }

        public string GetFullLabel()
        {
            return $"indirect[{Label.GetFullLabel()}]";
        }
    }
}
