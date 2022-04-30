using System;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonIndirect : IIndexedLabel, IDataLabel, IEquatable<TeuchiUdonIndirect>
    {
        public int Index { get; }
        public ICodeLabel Label { get; }
        public TeuchiUdonType Type { get; }

        public TeuchiUdonIndirect(int index, ICodeLabel label, TeuchiUdonType type)
        {
            Index = index;
            Label = label;
            Type  = type;
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
    }
}
