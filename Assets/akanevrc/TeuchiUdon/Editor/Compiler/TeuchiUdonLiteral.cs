using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonLiteral : IIndexedLabel, IEquatable<TeuchiUdonLiteral>
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
            return !object.ReferenceEquals(obj, null) && Text == obj.Text && Type.LogicalTypeEquals(obj.Type);
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

        public string GetFullLabel()
        {
            return $"literal[{Index}]";
        }

        public static TeuchiUdonLiteral CreateValue(int index, string value, TeuchiUdonType type)
        {
            var result = (object)null;
            if (type.LogicalTypeEquals(TeuchiUdonType.Bottom))
            {
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Bool))
            {
                result = Convert.ToBoolean(value);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Byte))
            {
                result = Convert.ToByte(value);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.SByte))
            {
                result = Convert.ToSByte(value);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Short))
            {
                result = Convert.ToInt16(value);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.UShort))
            {
                result = Convert.ToUInt16(value);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Int))
            {
                result = Convert.ToInt32(value);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.UInt))
            {
                result = Convert.ToUInt32(value);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Long))
            {
                result = Convert.ToInt64(value);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.ULong))
            {
                result = Convert.ToUInt64(value);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Float))
            {
                result = Convert.ToSingle(value);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Double))
            {
                result = Convert.ToDouble(value);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Decimal))
            {
                result = Convert.ToDecimal(value);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Char))
            {
                result = Convert.ToChar(Convert.ToInt32(value));
            }
            else
            {
                return null;
            }

            var literal = new TeuchiUdonLiteral(index, value, type, result);
            if (!TeuchiUdonTables.Instance.Literals.ContainsKey(literal))
            {
                TeuchiUdonTables.Instance.Literals.Add(literal, literal);
            }
            else
            {
                literal = TeuchiUdonTables.Instance.Literals[literal];
            }
            return literal;
        }

        public static TeuchiUdonLiteral CreateMask(int index, TeuchiUdonType type)
        {
            var result = (object)null;
            if (type.LogicalTypeEquals(TeuchiUdonType.Bool))
            {
                result = true;
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Byte))
            {
                result = ~Convert.ToByte(0);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.SByte))
            {
                result = ~Convert.ToSByte(0);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Short))
            {
                result = ~Convert.ToInt16(0);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.UShort))
            {
                result = ~Convert.ToUInt16(0);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Int))
            {
                result = ~Convert.ToInt32(0);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.UInt))
            {
                result = ~Convert.ToUInt32(0);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Long))
            {
                result = ~Convert.ToInt64(0);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.ULong))
            {
                result = ~Convert.ToUInt64(0);
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Char))
            {
                result = ~Convert.ToChar(Convert.ToInt32(0));
            }
            else
            {
                return null;
            }

            var literal = new TeuchiUdonLiteral(index, "~0", type, result);
            if (!TeuchiUdonTables.Instance.Literals.ContainsKey(literal))
            {
                TeuchiUdonTables.Instance.Literals.Add(literal, literal);
            }
            else
            {
                literal = TeuchiUdonTables.Instance.Literals[literal];
            }
            return literal;
        }

        public static TeuchiUdonLiteral CreateDotNetType(int index, TeuchiUdonType type)
        {
            return new TeuchiUdonLiteral(index, $"typeof({type.LogicalName})", TeuchiUdonType.DotNetType, type.RealType);
        }
    }
}
