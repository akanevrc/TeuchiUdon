using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonBranch : IIndexedLabel, ICodeLabel, IEquatable<TeuchiUdonBranch>
    {
        public int Index { get; }

        public TeuchiUdonBranch(int index)
        {
            Index = index;
        }

        public bool Equals(TeuchiUdonBranch obj)
        {
            return !object.ReferenceEquals(obj, null) && Index == obj.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonBranch branch ? Equals(branch) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonBranch obj1, TeuchiUdonBranch obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonBranch obj1, TeuchiUdonBranch obj2)
        {
            return !(obj1 == obj2);
        }

        public string GetLabel()
        {
            return $"branch[{Index}]";
        }

        public string GetFullLabel()
        {
            return $"branch[{Index}]";
        }
    }
}
