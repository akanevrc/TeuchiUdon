using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public enum TeuchiUdonScopeMode
    {
        None,
        Block,
        FuncBlock,
        LoopBlock,
        Func,
        LetIn,
        Type,
        VarBind
    }

    public class TeuchiUdonScope : IEquatable<TeuchiUdonScope>
    {
        public ITeuchiUdonLabel Label { get; }
        public TeuchiUdonScopeMode Mode { get; }

        public TeuchiUdonScope(ITeuchiUdonLabel label)
            : this(label, TeuchiUdonScopeMode.None)
        {
        }

        public TeuchiUdonScope(ITeuchiUdonLabel label, TeuchiUdonScopeMode mode)
        {
            Label = label;
            Mode  = mode;
        }

        public TeuchiUdonScope(string label, TeuchiUdonScopeMode mode)
            : this(new TextLabel(label), mode)
        {
        }

        public bool Equals(TeuchiUdonScope obj)
        {
            return !object.ReferenceEquals(obj, null) && Label.Equals(obj.Label);
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonScope scope ? Equals(scope) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Label.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonScope obj1, TeuchiUdonScope obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonScope obj1, TeuchiUdonScope obj2)
        {
            return !(obj1 == obj2);
        }

        public override string ToString()
        {
            return Label.GetLabel();
        }
    }
}
