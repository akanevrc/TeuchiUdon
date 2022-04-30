using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonSyntaxOps
    {
        private TeuchiUdonPrimitives Primitives { get; }
        private TeuchiUdonStaticTables StaticTables { get; }
        private TeuchiUdonTypeOps TypeOps { get; }
        private TeuchiUdonTables Tables { get; }

        public TeuchiUdonSyntaxOps
        (
            TeuchiUdonPrimitives primitives,
            TeuchiUdonStaticTables staticTables,
            TeuchiUdonTypeOps typeOps,
            TeuchiUdonTables tables
        )
        {
            Primitives   = primitives;
            StaticTables = staticTables;
            TypeOps      = typeOps;
            Tables       = tables;
        }

        public TeuchiUdonLiteral CreateValueLiteral(int index, string value, TeuchiUdonType type)
        {
            var result = (object)null;
            if (type.LogicalTypeEquals(Primitives.NullType))
            {
            }
            else if (type.LogicalTypeEquals(Primitives.Bool))
            {
                result = Convert.ToBoolean(value);
            }
            else if (type.LogicalTypeEquals(Primitives.Byte))
            {
                result = Convert.ToByte(value);
            }
            else if (type.LogicalTypeEquals(Primitives.SByte))
            {
                result = Convert.ToSByte(value);
            }
            else if (type.LogicalTypeEquals(Primitives.Short))
            {
                result = Convert.ToInt16(value);
            }
            else if (type.LogicalTypeEquals(Primitives.UShort))
            {
                result = Convert.ToUInt16(value);
            }
            else if (type.LogicalTypeEquals(Primitives.Int))
            {
                result = Convert.ToInt32(value);
            }
            else if (type.LogicalTypeEquals(Primitives.UInt))
            {
                result = Convert.ToUInt32(value);
            }
            else if (type.LogicalTypeEquals(Primitives.Long))
            {
                result = Convert.ToInt64(value);
            }
            else if (type.LogicalTypeEquals(Primitives.ULong))
            {
                result = Convert.ToUInt64(value);
            }
            else if (type.LogicalTypeEquals(Primitives.Float))
            {
                result = Convert.ToSingle(value);
            }
            else if (type.LogicalTypeEquals(Primitives.Double))
            {
                result = Convert.ToDouble(value);
            }
            else if (type.LogicalTypeEquals(Primitives.Decimal))
            {
                result = Convert.ToDecimal(value);
            }
            else if (type.LogicalTypeEquals(Primitives.Char))
            {
                result = Convert.ToChar(Convert.ToInt32(value));
            }
            else if (type.LogicalTypeEquals(Primitives.String))
            {
                result = value;
                value  = $"\"{value}\"";
            }
            else
            {
                return null;
            }

            var literal = new TeuchiUdonLiteral(index, value, type, result);
            if (!Tables.Literals.ContainsKey(literal))
            {
                Tables.Literals.Add(literal, literal);
            }
            else
            {
                literal = Tables.Literals[literal];
            }
            return literal;
        }

        public TeuchiUdonLiteral CreateMaskLiteral(int index, TeuchiUdonType type)
        {
            var result = (object)null;
            if (type.LogicalTypeEquals(Primitives.Bool))
            {
                result = true;
            }
            else if (type.LogicalTypeEquals(Primitives.Byte))
            {
                result = ~Convert.ToByte(0);
            }
            else if (type.LogicalTypeEquals(Primitives.SByte))
            {
                result = ~Convert.ToSByte(0);
            }
            else if (type.LogicalTypeEquals(Primitives.Short))
            {
                result = ~Convert.ToInt16(0);
            }
            else if (type.LogicalTypeEquals(Primitives.UShort))
            {
                result = ~Convert.ToUInt16(0);
            }
            else if (type.LogicalTypeEquals(Primitives.Int))
            {
                result = ~Convert.ToInt32(0);
            }
            else if (type.LogicalTypeEquals(Primitives.UInt))
            {
                result = ~Convert.ToUInt32(0);
            }
            else if (type.LogicalTypeEquals(Primitives.Long))
            {
                result = ~Convert.ToInt64(0);
            }
            else if (type.LogicalTypeEquals(Primitives.ULong))
            {
                result = ~Convert.ToUInt64(0);
            }
            else if (type.LogicalTypeEquals(Primitives.Char))
            {
                result = ~Convert.ToChar(Convert.ToInt32(0));
            }
            else
            {
                return null;
            }

            var literal = new TeuchiUdonLiteral(index, "~0", type, result);
            if (!Tables.Literals.ContainsKey(literal))
            {
                Tables.Literals.Add(literal, literal);
            }
            else
            {
                literal = Tables.Literals[literal];
            }
            return literal;
        }

        public TeuchiUdonLiteral CreateDotNetTypeLiteral(int index, TeuchiUdonType type)
        {
            if (type.RealType == null)
            {
                return null;
            }

            var literal = new TeuchiUdonLiteral(index, $"typeof({type.LogicalName})", Primitives.DotNetType, type.RealType);
            if (!Tables.Literals.ContainsKey(literal))
            {
                Tables.Literals.Add(literal, literal);
            }
            else
            {
                literal = Tables.Literals[literal];
            }
            return literal;
        }

        public string GetConvertMethodName(TeuchiUdonType type)
        {
            if (type.LogicalTypeEquals(Primitives.Bool))
            {
                return "ToBoolean";
            }
            else if (type.LogicalTypeEquals(Primitives.Byte))
            {
                return "ToByte";
            }
            else if (type.LogicalTypeEquals(Primitives.Char))
            {
                return "ToChar";
            }
            else if (type.LogicalTypeEquals(Primitives.DateTime))
            {
                return "ToDateTime";
            }
            else if (type.LogicalTypeEquals(Primitives.Decimal))
            {
                return "ToDecimal";
            }
            else if (type.LogicalTypeEquals(Primitives.Double))
            {
                return "ToDouble";
            }
            else if (type.LogicalTypeEquals(Primitives.Short))
            {
                return "ToInt16";
            }
            else if (type.LogicalTypeEquals(Primitives.Int))
            {
                return "ToInt32";
            }
            else if (type.LogicalTypeEquals(Primitives.Long))
            {
                return "ToInt64";
            }
            else if (type.LogicalTypeEquals(Primitives.SByte))
            {
                return "ToSByte";
            }
            else if (type.LogicalTypeEquals(Primitives.Float))
            {
                return "ToSingle";
            }
            else if (type.LogicalTypeEquals(Primitives.String))
            {
                return "ToString";
            }
            else if (type.LogicalTypeEquals(Primitives.UShort))
            {
                return "ToUInt16";
            }
            else if (type.LogicalTypeEquals(Primitives.UInt))
            {
                return "ToUInt32";
            }
            else if (type.LogicalTypeEquals(Primitives.ULong))
            {
                return "ToUInt64";
            }
            else
            {
                return "";
            }
        }

        public TeuchiUdonMethod GetDebugMethod()
        {
            return
                StaticTables.TypeToMethods[Primitives.Type.ApplyArgAsType(new TeuchiUdonType("UnityEngineDebug"))]["Log"]
                .First
                (
                    x => x.InTypes.Length == 1 &&
                    x.InTypes[0].LogicalTypeEquals(Primitives.Object)
                );
        }

        public TeuchiUdonType ToOneType(IEnumerable<TeuchiUdonType> types)
        {
            var count = types.Count();
            return
                count == 0 ? Primitives.Unit :
                count == 1 ? types.First() :
                Primitives.Tuple.ApplyArgsAsTuple(types);
        }

        public TeuchiUdonType GetUpperType(IEnumerable<TeuchiUdonType> types)
        {
            if (types.Any())
            {
                var upper    = types.First();
                var unknowns = new HashSet<TeuchiUdonType>();
                foreach (var t in types.Skip(1))
                {
                    if (TypeOps.IsAssignableFrom(upper, t))
                    {
                    }
                    else if (TypeOps.IsAssignableFrom(t, upper))
                    {
                        upper = t;
                    }
                    else if (!unknowns.Contains(t))
                    {
                        unknowns.Add(t);
                    }
                }

                if (unknowns.All(x => TypeOps.IsAssignableFrom(upper, x)))
                {
                    return upper;
                }
                else
                {
                    return Primitives.Unknown;
                }
            }
            else
            {
                return Primitives.Unit;
            }
        }
    }
}
