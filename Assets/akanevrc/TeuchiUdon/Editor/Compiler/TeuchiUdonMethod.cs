using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public enum TeuchiUdonMethodParamInOut
    {
        In,
        InOut,
        Out
    }

    public class TeuchiUdonMethod : ITeuchiUdonTypeArg, ITeuchiUdonLeftValue, IEquatable<TeuchiUdonMethod>
    {
        public static TeuchiUdonMethod InvalidMethod { get; } =
            new TeuchiUdonMethod
            (
                TeuchiUdonType.Invalid,
                "_",
                Enumerable.Empty<TeuchiUdonType>(),
                Enumerable.Empty<TeuchiUdonType>(),
                Enumerable.Empty<TeuchiUdonType>(),
                Enumerable.Empty<TeuchiUdonMethodParamInOut>(),
                "_",
                Enumerable.Empty<string>()
            );

        public TeuchiUdonType Type { get; }
        public string Name { get; }
        public TeuchiUdonType[] AllParamTypes { get; }
        public TeuchiUdonType[] InTypes { get; }
        public TeuchiUdonType[] OutTypes { get; }
        public TeuchiUdonMethodParamInOut[] AllParamInOuts { get; }
        public TeuchiUdonMethodParamInOut[] InParamInOuts { get; }
        public string UdonName { get; }
        public string[] AllParamUdonNames { get; }
        public string[] InParamUdonNames { get; }
        public string[] OutParamUdonNames { get; }

        public TeuchiUdonMethod(TeuchiUdonType type, string name)
            : this(type, name, null, null, null, null, null, null)
        {
        }

        public TeuchiUdonMethod(TeuchiUdonType type, string name, IEnumerable<TeuchiUdonType> inTypes)
            : this(type, name, null, inTypes, null, null, null, null)
        {
        }

        public TeuchiUdonMethod
        (
            TeuchiUdonType type,
            string name,
            IEnumerable<TeuchiUdonType> allParamTypes,
            IEnumerable<TeuchiUdonType> inTypes,
            IEnumerable<TeuchiUdonType> outTypes,
            IEnumerable<TeuchiUdonMethodParamInOut> allParamInOuts,
            string udonName,
            IEnumerable<string> allParamUdonNames
        )
        {
            Type              = type;
            Name              = name;
            AllParamTypes     = allParamTypes ?.ToArray();
            InTypes           = inTypes       ?.ToArray();
            OutTypes          = outTypes      ?.ToArray();
            AllParamInOuts    = allParamInOuts?.ToArray();
            InParamInOuts     = allParamInOuts?.Where(x => x != TeuchiUdonMethodParamInOut.Out).ToArray();
            UdonName          = udonName;
            AllParamUdonNames = allParamUdonNames?.ToArray();
            InParamUdonNames  = allParamUdonNames?.Zip(allParamInOuts, (un, io) => (un, io)).Where(x => x.io != TeuchiUdonMethodParamInOut.Out).Select(x => x.un).ToArray();
            OutParamUdonNames = allParamUdonNames?.Zip(allParamInOuts, (un, io) => (un, io)).Where(x => x.io == TeuchiUdonMethodParamInOut.Out).Select(x => x.un).ToArray();
        }

        public bool Equals(TeuchiUdonMethod obj)
        {
            return !object.ReferenceEquals(obj, null) && Type.LogicalTypeEquals(obj.Type) && Name == obj.Name && InTypes.SequenceEqual(obj.InTypes, TeuchiUdonTypeLogicalEqualityComparer.Instance);
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonMethod method ? Equals(method) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonMethod obj1, TeuchiUdonMethod obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonMethod obj1, TeuchiUdonMethod obj2)
        {
            return !(obj1 == obj2);
        }

        public override string ToString()
        {
            return $"{Type}.{Name}({string.Join<TeuchiUdonType>(", ", InTypes)})";
        }

        public string GetLogicalName()
        {
            return $"{Type.GetLogicalName()}{Name}{string.Join("", InTypes.Select(x => x.GetLogicalName()))}";
        }

        public static string GetConvertMethodName(TeuchiUdonType type)
        {
            if (type.LogicalTypeEquals(TeuchiUdonType.Bool))
            {
                return "ToBoolean";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Byte))
            {
                return "ToByte";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Char))
            {
                return "ToChar";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.DateTime))
            {
                return "ToDateTime";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Decimal))
            {
                return "ToDecimal";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Double))
            {
                return "ToDouble";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Short))
            {
                return "ToInt16";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Int))
            {
                return "ToInt32";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Long))
            {
                return "ToInt64";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.SByte))
            {
                return "ToSByte";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Float))
            {
                return "ToSingle";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.String))
            {
                return "ToString";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.UShort))
            {
                return "ToUInt16";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.UInt))
            {
                return "ToUInt32";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.ULong))
            {
                return "ToUInt64";
            }
            else
            {
                return "";
            }
        }
    }
}
