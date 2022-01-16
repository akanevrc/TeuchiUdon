using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonLiteral : IEquatable<TeuchiUdonLiteral>
    {
        public int Index { get; }
        public string Text { get; }
        public TeuchiUdonType Type { get; }
        public object Value { get; }

        public TeuchiUdonLiteral(int index)
        {
            Index = index;
            Text  = null;
            Type  = null;
            Value = null;
        }

        public TeuchiUdonLiteral(string text)
        {
            Index = -1;
            Text  = text;
            Type  = null;
            Value = null;
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
            return Text == obj.Text;
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
            return obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonLiteral obj1, TeuchiUdonLiteral obj2)
        {
            return !obj1.Equals(obj2);
        }

        public override string ToString()
        {
            return Text;
        }

        public string GetUdonName()
        {
            return $"literal[{Index}]";
        }
    }
}
