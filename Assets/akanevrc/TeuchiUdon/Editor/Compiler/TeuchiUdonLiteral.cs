using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonLiteral : ITeuchiUdonLabel, IEquatable<TeuchiUdonLiteral>
    {
        public int Index { get; }
        public string Text { get; }
        public TeuchiUdonType Type { get; }
        public object Value { get; }

        public TeuchiUdonLiteral(int index)
            : this(index, null, null, null)
        {
        }

        public TeuchiUdonLiteral(string text)
            : this(-1, text, null, null)
        {
        }

        public TeuchiUdonLiteral(int index, string text, TeuchiUdonType type, object value)
        {
            Index = index;
            Text  = text;
            Type  = type;
            Value = value;
        }

        public bool Equals(TeuchiUdonLiteral obj)
        {
            return !object.ReferenceEquals(obj, null) && Text == obj.Text;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonLiteral literal ? Equals(literal) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonLiteral obj1, TeuchiUdonLiteral obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonLiteral obj1, TeuchiUdonLiteral obj2)
        {
            return !(obj1 == obj2);
        }

        public override string ToString()
        {
            return Text;
        }

        public string GetLabel()
        {
            return $"literal[{Index}]";
        }
    }
}
