using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonRawAddress : ITeuchiUdonLabel, IEquatable<TeuchiUdonRawAddress>
    {
        public ITeuchiUdonLabel Label { get; }

        public TeuchiUdonRawAddress(ITeuchiUdonLabel label)
        {
            Label = label;
        }

        public bool Equals(TeuchiUdonRawAddress obj)
        {
            return !object.ReferenceEquals(obj, null) && Label == obj.Label;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonRawAddress address ? Equals(address) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Label.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonRawAddress obj1, TeuchiUdonRawAddress obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonRawAddress obj1, TeuchiUdonRawAddress obj2)
        {
            return !(obj1 == obj2);
        }

        public string GetLabel()
        {
            return $"address[{Label.GetLabel()}]";
        }

        public string GetFullLabel()
        {
            return $"address[{Label.GetFullLabel()}]";
        }
    }
}
