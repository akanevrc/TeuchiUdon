using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonTypeOps
    {
        private TeuchiUdonPrimitives Primitives { get; }
        private TeuchiUdonStaticTables StaticTables { get; }
        private TeuchiUdonInvalids Invalids { get; }
        private TeuchiUdonParserErrorOps ParserErrorOps { get; }

        public TeuchiUdonTypeOps
        (
            TeuchiUdonPrimitives primitives,
            TeuchiUdonStaticTables staticTables,
            TeuchiUdonInvalids invalids,
            TeuchiUdonParserErrorOps parserErrorOps
        )
        {
            Primitives     = primitives;
            StaticTables   = staticTables;
            Invalids       = invalids;
            ParserErrorOps = parserErrorOps;
        }

        public string GetRealName(TeuchiUdonType type)
        {
            if (type.RealName == null)
            {
                ParserErrorOps.AppendError(null, null, $"no real type name");
                return "";
            }
            return type.RealName;
        }

        public IEnumerable<(string name, TeuchiUdonType type)> GetMembers(TeuchiUdonType type)
        {
            if (type.RealType == null)
            {
                return Enumerable.Empty<(string, TeuchiUdonType)>();
            }
            else if (type.LogicalTypeNameEquals(Primitives.Tuple))
            {
                return type.GetArgsAsTuple().Select((x, i) => (i.ToString(), x));
            }
            else if (type.LogicalTypeNameEquals(Primitives.Array))
            {
                return new (string, TeuchiUdonType)[] { ("", type) };
            }
            else
            {
                return new (string, TeuchiUdonType)[] { ("", type) };
            }
        }

        public int GetMemberCount(TeuchiUdonType type)
        {
            if (type.RealType == null)
            {
                return 0;
            }
            else if (type.LogicalTypeNameEquals(Primitives.Tuple))
            {
                return type.GetArgsAsTuple().Sum(x => GetMemberCount(x));
            }
            else if (type.LogicalTypeNameEquals(Primitives.Array))
            {
                return 1;
            }
            else
            {
                return 1;
            }
        }

        public bool IsAssignableFrom(TeuchiUdonType obj1, TeuchiUdonType obj2)
        {
            return
                obj1 != null && obj2 != null &&
                (
                    IsAssignableFromUnknown (obj1, obj2) ||
                    IsAssignableFromAny     (obj1, obj2) ||
                    IsAssignableFromBottom  (obj1, obj2) ||
                    IsAssignableFromUnit    (obj1, obj2) ||
                    IsAssignableFromQual    (obj1, obj2) ||
                    IsAssignableFromType    (obj1, obj2) ||
                    IsAssignableFromTuple   (obj1, obj2) ||
                    IsAssignableFromArray   (obj1, obj2) ||
                    IsAssignableFromList    (obj1, obj2) ||
                    IsAssignableFromNdFunc  (obj1, obj2) ||
                    IsAssignableFromFunc    (obj1, obj2) ||
                    IsAssignableFromSetter  (obj1, obj2) ||
                    IsAssignableFromNullType(obj1, obj2) ||
                    IsAssignableFromDotNet  (obj1, obj2)
                );
        }

        private bool IsAssignableFromUnknown(TeuchiUdonType obj1, TeuchiUdonType obj2)
        {
            if (obj1 == null || obj2 == null) return false;
            if (!obj1.LogicalTypeEquals(Primitives.Unknown) && !obj2.LogicalTypeEquals(Primitives.Unknown)) return false;

            return true;
        }

        private bool IsAssignableFromAny(TeuchiUdonType obj1, TeuchiUdonType obj2)
        {
            if (obj1 == null || obj2 == null) return false;
            if (!obj1.LogicalTypeEquals(Primitives.Any)) return false;

            return true;
        }

        private bool IsAssignableFromBottom(TeuchiUdonType obj1, TeuchiUdonType obj2)
        {
            if (obj1 == null || obj2 == null) return false;
            if (!obj2.LogicalTypeEquals(Primitives.Bottom)) return false;

            return true;
        }

        private bool IsAssignableFromUnit(TeuchiUdonType obj1, TeuchiUdonType obj2)
        {
            if (obj1 == null || obj2 == null) return false;
            if (!obj1.LogicalTypeEquals(Primitives.Unit) || !obj2.LogicalTypeEquals(Primitives.Unit)) return false;

            return true;
        }

        private bool IsAssignableFromQual(TeuchiUdonType obj1, TeuchiUdonType obj2)
        {
            if (obj1 == null || obj2 == null) return false;
            if (!obj1.LogicalTypeEquals(Primitives.Qual) || !obj2.LogicalTypeNameEquals(Primitives.Qual)) return false;

            return true;
        }

        private bool IsAssignableFromType(TeuchiUdonType obj1, TeuchiUdonType obj2)
        {
            if (obj1 == null || obj2 == null) return false;
            if (!obj1.LogicalTypeEquals(Primitives.Type) || !obj2.LogicalTypeNameEquals(Primitives.Type)) return false;

            return IsAssignableFrom(obj1.GetArgAsType(), obj2.GetArgAsType());
        }

        private bool IsAssignableFromTuple(TeuchiUdonType obj1, TeuchiUdonType obj2)
        {
            if (obj1 == null || obj2 == null) return false;
            if (!obj1.LogicalTypeNameEquals(Primitives.Tuple) || !obj2.LogicalTypeNameEquals(Primitives.Tuple)) return false;

            return obj1.GetArgsAsTuple().Zip(obj2.GetArgsAsTuple(), (o1, o2) => (o1, o2)).All(x => IsAssignableFrom(x.o1, x.o2));
        }

        private bool IsAssignableFromArray(TeuchiUdonType obj1, TeuchiUdonType obj2)
        {
            if (obj1 == null || obj2 == null) return false;
            if (!obj1.LogicalTypeNameEquals(Primitives.Array) || !obj2.LogicalTypeNameEquals(Primitives.Array)) return false;

            return
                StaticTables.Types.ContainsKey(obj1) && StaticTables.Types.ContainsKey(obj2) ?
                obj1.RealType.IsAssignableFrom(obj2.RealType) :
                IsAssignableFrom(obj1.GetArgAsArray(), obj2.GetArgAsArray());
        }

        private bool IsAssignableFromList(TeuchiUdonType obj1, TeuchiUdonType obj2)
        {
            if (obj1 == null || obj2 == null) return false;
            if (!obj1.LogicalTypeNameEquals(Primitives.List) || !obj2.LogicalTypeNameEquals(Primitives.List)) return false;

            var arrayType    = obj1.GetArgAsListArrayType();
            var objArrayType = obj2.GetArgAsListArrayType();
            return
                StaticTables.Types.ContainsKey(arrayType) && StaticTables.Types.ContainsKey(objArrayType) ?
                arrayType.RealType.IsAssignableFrom(objArrayType.RealType) :
                IsAssignableFrom(obj1.GetArgAsListElementType(), obj2.GetArgAsListElementType());
        }

        private bool IsAssignableFromNdFunc(TeuchiUdonType obj1, TeuchiUdonType obj2)
        {
            if (obj1 == null || obj2 == null) return false;
            if
            (
                !obj1.LogicalTypeNameEquals(Primitives.NdFunc) ||
                !obj2.LogicalTypeNameEquals(Primitives.NdFunc) && !obj2.LogicalTypeNameEquals(Primitives.Func)
            ) return false;

            return
                IsAssignableFrom(obj1.GetArgAsFuncInType (), obj2.GetArgAsFuncInType ()) &&
                IsAssignableFrom(obj2.GetArgAsFuncOutType(), obj1.GetArgAsFuncOutType());
        }

        private bool IsAssignableFromFunc(TeuchiUdonType obj1, TeuchiUdonType obj2)
        {
            if (obj1 == null || obj2 == null) return false;
            if (!obj1.LogicalTypeNameEquals(Primitives.Func) || !obj2.LogicalTypeNameEquals(Primitives.Func)) return false;

            return
                IsAssignableFrom(obj1.GetArgAsFuncInType (), obj2.GetArgAsFuncInType ()) &&
                IsAssignableFrom(obj2.GetArgAsFuncOutType(), obj1.GetArgAsFuncOutType());
        }

        private bool IsAssignableFromSetter(TeuchiUdonType obj1, TeuchiUdonType obj2)
        {
            if (obj1 == null || obj2 == null) return false;
            if (!obj1.LogicalTypeNameEquals(Primitives.Setter)) return false;

            return IsAssignableFrom(obj1.GetArgAsSetter(), obj2);
        }

        private bool IsAssignableFromNullType(TeuchiUdonType obj1, TeuchiUdonType obj2)
        {
            if (obj1 == null || obj2 == null) return false;
            if (!IsDotNetType(obj1) && !obj1.LogicalTypeNameEquals(Primitives.NullType) || !obj2.LogicalTypeNameEquals(Primitives.NullType)) return false;

            return true;
        }

        private bool IsAssignableFromDotNet(TeuchiUdonType obj1, TeuchiUdonType obj2)
        {
            if (obj1 == null || obj2 == null) return false;
            if (!IsDotNetType(obj1) || !IsDotNetType(obj2)) return false;

            return obj1.RealType != null && obj2.RealType != null && obj1.RealType.IsAssignableFrom(obj2.RealType);
        }

        public bool IsDotNetType(TeuchiUdonType type)
        {
            return new TeuchiUdonType[]
            {
                Primitives.Unknown,
                Invalids.InvalidType,
                Primitives.Any,
                Primitives.Bottom,
                Primitives.Unit,
                Primitives.Qual,
                Primitives.Type,
                Primitives.Tuple,
                Primitives.List,
                Primitives.NdFunc,
                Primitives.Func,
                Primitives.Method,
                Primitives.Setter,
                Primitives.Cast,
                Primitives.TypeOf
            }
            .All(x => !type.LogicalTypeNameEquals(x));
        }

        public bool IsSyncableType(TeuchiUdonType type)
        {
            return new TeuchiUdonType[]
            {
                Primitives.Bool,
                Primitives.Char,
                Primitives.Byte,
                Primitives.SByte,
                Primitives.Short,
                Primitives.UShort,
                Primitives.Int,
                Primitives.UInt,
                Primitives.Long,
                Primitives.ULong,
                Primitives.Float,
                Primitives.Double,
                Primitives.String,
                Primitives.Vector2,
                Primitives.Vector3,
                Primitives.Vector4,
                Primitives.Quaternion,
                Primitives.Color,
                Primitives.Color32,
                Primitives.VRCUrl
            }
            .Any(x => type.LogicalTypeEquals(x)) ||
            type.LogicalTypeNameEquals(Primitives.Array) &&
            new TeuchiUdonType[]
            {
                Primitives.Bool,
                Primitives.Char,
                Primitives.Byte,
                Primitives.SByte,
                Primitives.Short,
                Primitives.UShort,
                Primitives.Int,
                Primitives.UInt,
                Primitives.Long,
                Primitives.ULong,
                Primitives.Float,
                Primitives.Double,
                Primitives.Vector2,
                Primitives.Vector3,
                Primitives.Vector4,
                Primitives.Quaternion,
                Primitives.Color,
                Primitives.Color32
            }
            .Any(x => type.GetArgAsArray().LogicalTypeEquals(x));
        }

        public bool IsLinearSyncableType(TeuchiUdonType type)
        {
            return new TeuchiUdonType[]
            {
                Primitives.Byte,
                Primitives.SByte,
                Primitives.Short,
                Primitives.UShort,
                Primitives.Int,
                Primitives.UInt,
                Primitives.Long,
                Primitives.ULong,
                Primitives.Float,
                Primitives.Double,
                Primitives.Vector2,
                Primitives.Vector3,
                Primitives.Quaternion,
                Primitives.Color,
                Primitives.Color32
            }
            .Any(x => type.LogicalTypeEquals(x));
        }

        public bool IsSmoothSyncableType(TeuchiUdonType type)
        {
            return new TeuchiUdonType[]
            {
                Primitives.Byte,
                Primitives.SByte,
                Primitives.Short,
                Primitives.UShort,
                Primitives.Int,
                Primitives.UInt,
                Primitives.Long,
                Primitives.ULong,
                Primitives.Float,
                Primitives.Double,
                Primitives.Vector2,
                Primitives.Vector3,
                Primitives.Quaternion
            }
            .Any(x => type.LogicalTypeEquals(x));
        }

        public bool IsSignedIntegerType(TeuchiUdonType type)
        {
            return new TeuchiUdonType[]
            {
                Primitives.SByte,
                Primitives.Short,
                Primitives.Int,
                Primitives.Long
            }
            .Any(x => type.LogicalTypeEquals(x));
        }

        public bool IsFunc(TeuchiUdonType type)
        {
            return type.LogicalTypeNameEquals(Primitives.NdFunc) || type.LogicalTypeNameEquals(Primitives.Func);
        }

        public bool ContainsUnknown(TeuchiUdonType type)
        {
            return type.LogicalTypeEquals(Primitives.Unknown) || type.Args.Any(x => x is TeuchiUdonType t && ContainsUnknown(t));
        }

        public bool ContainsFunc(TeuchiUdonType type)
        {
            return IsFunc(type) || type.Args.Any(x => x is TeuchiUdonType t && ContainsFunc(t));
        }

        public bool ContainsNdFunc(TeuchiUdonType type)
        {
            return type.LogicalTypeNameEquals(Primitives.NdFunc) || type.Args.Any(x => x is TeuchiUdonType t && ContainsNdFunc(t));
        }

        public TeuchiUdonType Fix(TeuchiUdonType type, TeuchiUdonType fix)
        {
            if (type.LogicalTypeEquals(Primitives.Unknown))
            {
                return fix;
            }
            else if (IsAssignableFrom(type, fix) || IsAssignableFrom(fix, type))
            {
                var t =
                    type.ApplyArgs
                    (
                        type.Args
                        .Zip(fix.Args, (ta, fa) => (ta, fa))
                        .Select(x => x.ta is TeuchiUdonType tat && x.fa is TeuchiUdonType fat ? Fix(tat, fat) : x.fa)
                    );
                if (StaticTables.Types.ContainsKey(t))
                {
                    return StaticTables.Types[t];
                }
                else
                {
                    ParserErrorOps.AppendError(null, null, $"cannot bind type");
                    return Invalids.InvalidType;
                }
            }
            else
            {
                ParserErrorOps.AppendError(null, null, $"cannot bind type");
                return Invalids.InvalidType;
            }
        }

        public TeuchiUdonType ToArrayType(TeuchiUdonType type)
        {
            var qt = Primitives.Array.ApplyArgAsArray(type);
            return
                StaticTables.LogicalTypes.ContainsKey(qt) ?
                    StaticTables.LogicalTypes[qt] :
                    new TeuchiUdonType
                    (
                        TeuchiUdonQualifier.Top,
                        Primitives.Array.Name,
                        new TeuchiUdonType[] { type },
                        Primitives.Array.LogicalName,
                        Primitives.AnyArray.RealName,
                        Primitives.AnyArray.RealType
                    );
        }

        public IEnumerable<TeuchiUdonMethod> GetMostCompatibleMethods(TeuchiUdonType type, IEnumerable<TeuchiUdonType> inTypes)
        {
            if (!type.LogicalTypeNameEquals(Primitives.Method)) return Enumerable.Empty<TeuchiUdonMethod>();

            var it      = inTypes.ToArray();
            var methods = type.GetArgsAsMethod()
                .Where(x => x.InTypes.Length == it.Length)
                .ToArray();
            if (methods.Length == 0) return Enumerable.Empty<TeuchiUdonMethod>();

            var justCountToMethods = new Dictionary<int, List<TeuchiUdonMethod>>();
            foreach (var method in methods)
            {
                var isCompatible = true;
                var justCount    = 0;
                foreach (var (m, i) in method.InTypes.Zip(it, (m, i) => (m, i)))
                {
                    if (!IsAssignableFrom(m, i))
                    {
                        isCompatible = false;
                        break;
                    }

                    if (m.LogicalTypeEquals(i)) justCount++;
                }

                if (isCompatible)
                {
                    if (!justCountToMethods.ContainsKey(justCount))
                    {
                        justCountToMethods.Add(justCount, new List<TeuchiUdonMethod>());
                    }
                    justCountToMethods[justCount].Add(method);
                }
            }

            for (var i = it.Length; i >= 0; i--)
            {
                if (justCountToMethods.ContainsKey(i)) return justCountToMethods[i];
            }

            return Enumerable.Empty<TeuchiUdonMethod>();
        }
    }
}
