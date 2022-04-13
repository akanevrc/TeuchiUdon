using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon
{
    public interface ITeuchiUdonTypeArg
    {
        string GetLogicalName();
    }

    public class PrimitiveTypes
    {
        public static PrimitiveTypes Instance { get; } = new PrimitiveTypes();

        public TeuchiUdonType Unknown { get; private set; }
        public TeuchiUdonType Invalid { get; private set; }
        public TeuchiUdonType Any { get; private set; }
        public TeuchiUdonType Bottom { get; private set; }
        public TeuchiUdonType Unit { get; private set; }
        public TeuchiUdonType Qual { get; private set; }
        public TeuchiUdonType Type { get; private set; }
        public TeuchiUdonType Tuple { get; private set; }
        public TeuchiUdonType Array { get; private set; }
        public TeuchiUdonType AnyArray { get; private set; }
        public TeuchiUdonType List { get; private set; }
        public TeuchiUdonType Func { get; private set; }
        public TeuchiUdonType DetFunc { get; private set; }
        public TeuchiUdonType Method { get; private set; }
        public TeuchiUdonType Setter { get; private set; }
        public TeuchiUdonType Cast { get; private set; }
        public TeuchiUdonType TypeOf { get; private set; }
        public TeuchiUdonType NullType { get; private set; }
        public TeuchiUdonType Object { get; private set; }
        public TeuchiUdonType DotNetType { get; private set; }
        public TeuchiUdonType Bool { get; private set; }
        public TeuchiUdonType Byte { get; private set; }
        public TeuchiUdonType SByte { get; private set; }
        public TeuchiUdonType Short { get; private set; }
        public TeuchiUdonType UShort { get; private set; }
        public TeuchiUdonType Int { get; private set; }
        public TeuchiUdonType UInt { get; private set; }
        public TeuchiUdonType Long { get; private set; }
        public TeuchiUdonType ULong { get; private set; }
        public TeuchiUdonType Float { get; private set; }
        public TeuchiUdonType Double { get; private set; }
        public TeuchiUdonType Decimal { get; private set; }
        public TeuchiUdonType DateTime { get; private set; }
        public TeuchiUdonType Char { get; private set; }
        public TeuchiUdonType String { get; private set; }
        public TeuchiUdonType UnityObject { get; private set; }
        public TeuchiUdonType GameObject { get; private set; }
        public TeuchiUdonType Vector2 { get; private set; }
        public TeuchiUdonType Vector3 { get; private set; }
        public TeuchiUdonType Vector4 { get; private set; }
        public TeuchiUdonType Quaternion { get; private set; }
        public TeuchiUdonType Color { get; private set; }
        public TeuchiUdonType Color32 { get; private set; }
        public TeuchiUdonType VRCUrl { get; private set; }
        public TeuchiUdonType UdonBehaviour { get; private set; }

        private PrimitiveTypes()
        {
        }

        public void Init()
        {
            Unknown       = new TeuchiUdonType(TeuchiUdonQualifier.Top, "unknown"      , "unknown"              , null, null);
            Invalid       = new TeuchiUdonType(TeuchiUdonQualifier.Top, "invalid"      , "invalid"              , "SystemObject", typeof(object));
            Any           = new TeuchiUdonType(TeuchiUdonQualifier.Top, "any"          , "any"                  , "SystemObject", typeof(object));
            Bottom        = new TeuchiUdonType(TeuchiUdonQualifier.Top, "bottom"       , "bottom"               , null, null);
            Unit          = new TeuchiUdonType(TeuchiUdonQualifier.Top, "unit"         , "unit"                 , null, null);
            Qual          = new TeuchiUdonType(TeuchiUdonQualifier.Top, "qual"         , "qual"                 , null, null);
            Type          = new TeuchiUdonType(TeuchiUdonQualifier.Top, "internaltype" , "internaltype"         , null, null);
            Tuple         = new TeuchiUdonType(TeuchiUdonQualifier.Top, "tuple"        , "tuple"                , null, null);
            Array         = new TeuchiUdonType(TeuchiUdonQualifier.Top, "array"        , "array"                , null, null);
            AnyArray      = new TeuchiUdonType(TeuchiUdonQualifier.Top, "anyarray"     , "array"                , "SystemObjectArray", typeof(object[])).ApplyArgAsArray(new TeuchiUdonType(TeuchiUdonQualifier.Top, "object", "SystemObject", "SystemObject", typeof(object)));
            List          = new TeuchiUdonType(TeuchiUdonQualifier.Top, "list"         , "list"                 , null, null);
            Func          = new TeuchiUdonType(TeuchiUdonQualifier.Top, "func"         , "func"                 , null, null);
            DetFunc       = new TeuchiUdonType(TeuchiUdonQualifier.Top, "detfunc"      , "detfunc"              , null, null);
            Method        = new TeuchiUdonType(TeuchiUdonQualifier.Top, "method"       , "method"               , null, null);
            Setter        = new TeuchiUdonType(TeuchiUdonQualifier.Top, "setter"       , "setter"               , null, null);
            Cast          = new TeuchiUdonType(TeuchiUdonQualifier.Top, "cast"         , "cast"                 , null, null);
            TypeOf        = new TeuchiUdonType(TeuchiUdonQualifier.Top, "typeof"       , "typeof"               , null, null);
            NullType      = new TeuchiUdonType(TeuchiUdonQualifier.Top, "nulltype"     , "nulltype"             , "SystemObject"  , typeof(object));
            Object        = new TeuchiUdonType(TeuchiUdonQualifier.Top, "object"       , "SystemObject"         , "SystemObject"  , typeof(object));
            DotNetType    = new TeuchiUdonType(TeuchiUdonQualifier.Top, "type"         , "SystemType"           , "SystemType"    , typeof(Type));
            Bool          = new TeuchiUdonType(TeuchiUdonQualifier.Top, "bool"         , "SystemBoolean"        , "SystemBoolean" , typeof(bool));
            Byte          = new TeuchiUdonType(TeuchiUdonQualifier.Top, "byte"         , "SystemByte"           , "SystemByte"    , typeof(byte));
            SByte         = new TeuchiUdonType(TeuchiUdonQualifier.Top, "sbyte"        , "SystemSByte"          , "SystemSByte"   , typeof(sbyte));
            Short         = new TeuchiUdonType(TeuchiUdonQualifier.Top, "short"        , "SystemInt16"          , "SystemInt16"   , typeof(short));
            UShort        = new TeuchiUdonType(TeuchiUdonQualifier.Top, "ushort"       , "SystemUInt16"         , "SystemUInt16"  , typeof(ushort));
            Int           = new TeuchiUdonType(TeuchiUdonQualifier.Top, "int"          , "SystemInt32"          , "SystemInt32"   , typeof(int));
            UInt          = new TeuchiUdonType(TeuchiUdonQualifier.Top, "uint"         , "SystemUInt32"         , "SystemUInt32"  , typeof(uint));
            Long          = new TeuchiUdonType(TeuchiUdonQualifier.Top, "long"         , "SystemInt64"          , "SystemInt64"   , typeof(long));
            ULong         = new TeuchiUdonType(TeuchiUdonQualifier.Top, "ulong"        , "SystemUInt64"         , "SystemUInt64"  , typeof(ulong));
            Float         = new TeuchiUdonType(TeuchiUdonQualifier.Top, "float"        , "SystemSingle"         , "SystemSingle"  , typeof(float));
            Double        = new TeuchiUdonType(TeuchiUdonQualifier.Top, "double"       , "SystemDouble"         , "SystemDouble"  , typeof(double));
            Decimal       = new TeuchiUdonType(TeuchiUdonQualifier.Top, "decimal"      , "SystemDecimal"        , "SystemDecimal" , typeof(decimal));
            DateTime      = new TeuchiUdonType(TeuchiUdonQualifier.Top, "datetime"     , "SystemDateTime"       , "SystemDateTime", typeof(DateTime));
            Char          = new TeuchiUdonType(TeuchiUdonQualifier.Top, "char"         , "SystemChar"           , "SystemChar"    , typeof(char));
            String        = new TeuchiUdonType(TeuchiUdonQualifier.Top, "string"       , "SystemString"         , "SystemString"  , typeof(string));
            UnityObject   = new TeuchiUdonType(TeuchiUdonQualifier.Top, "unityobject"  , "UnityEngineObject"    , "UnityEngineObject"    , TeuchiUdonDllLoader.Instance.GetTypeFromAssembly("UnityEngine.dll", "UnityEngine.Object"));
            GameObject    = new TeuchiUdonType(TeuchiUdonQualifier.Top, "gameobject"   , "UnityEngineGameObject", "UnityEngineGameObject", TeuchiUdonDllLoader.Instance.GetTypeFromAssembly("UnityEngine.dll", "UnityEngine.GameObject"));
            Vector2       = new TeuchiUdonType(TeuchiUdonQualifier.Top, "vector2"      , "UnityEngineVector2"   , "UnityEngineVector2"   , TeuchiUdonDllLoader.Instance.GetTypeFromAssembly("UnityEngine.dll", "UnityEngine.Vector2"));
            Vector3       = new TeuchiUdonType(TeuchiUdonQualifier.Top, "vector3"      , "UnityEngineVector3"   , "UnityEngineVector3"   , TeuchiUdonDllLoader.Instance.GetTypeFromAssembly("UnityEngine.dll", "UnityEngine.Vector3"));
            Vector4       = new TeuchiUdonType(TeuchiUdonQualifier.Top, "vector4"      , "UnityEngineVector4"   , "UnityEngineVector4"   , TeuchiUdonDllLoader.Instance.GetTypeFromAssembly("UnityEngine.dll", "UnityEngine.Vector4"));
            Quaternion    = new TeuchiUdonType(TeuchiUdonQualifier.Top, "quaternion"   , "UnityEngineQuaternion", "UnityEngineQuaternion", TeuchiUdonDllLoader.Instance.GetTypeFromAssembly("UnityEngine.dll", "UnityEngine.Quaternion"));
            Color         = new TeuchiUdonType(TeuchiUdonQualifier.Top, "color"        , "UnityEngineColor"     , "UnityEngineColor"     , TeuchiUdonDllLoader.Instance.GetTypeFromAssembly("UnityEngine.dll", "UnityEngine.Color"));
            Color32       = new TeuchiUdonType(TeuchiUdonQualifier.Top, "color32"      , "UnityEngineColor32"   , "UnityEngineColor32"   , TeuchiUdonDllLoader.Instance.GetTypeFromAssembly("UnityEngine.dll", "UnityEngine.Color32"));
            VRCUrl        = new TeuchiUdonType(TeuchiUdonQualifier.Top, "vrcurl"       , "VRCSDKBaseVRCUrl"     , "VRCSDKBaseVRCUrl"     , TeuchiUdonDllLoader.Instance.GetTypeFromAssembly("VRCSDKBase.dll" , "VRC.SDKBase.VRCUrl"));
            UdonBehaviour = new TeuchiUdonType(TeuchiUdonQualifier.Top, "udonbehaviour", "VRCUdonUdonBehaviour" , "VRCUdonUdonBehaviour" , TeuchiUdonDllLoader.Instance.GetTypeFromAssembly("VRC.Udon.dll"   , "VRC.Udon.UdonBehaviour"));
        }
    }

    public class TeuchiUdonType : ITeuchiUdonTypeArg, IEquatable<TeuchiUdonType>
    {
        public TeuchiUdonQualifier Qualifier { get; }
        public string Name { get; }
        public ITeuchiUdonTypeArg[] Args { get; }
        public string LogicalName { get; }
        private string RealName { get; }
        public Type RealType { get; }

        public TeuchiUdonType(TeuchiUdonQualifier qualifier, string name)
            : this(qualifier, name, Enumerable.Empty<TeuchiUdonType>(), null, null, null)
        {
        }

        public TeuchiUdonType(TeuchiUdonQualifier qualifier, string name, IEnumerable<ITeuchiUdonTypeArg> args)
            : this(qualifier, name, args, null, null, null)
        {
        }

        public TeuchiUdonType(string logicalName)
            : this(null, null, Enumerable.Empty<TeuchiUdonType>(), logicalName, null, null)
        {
        }

        public TeuchiUdonType(string logicalName, IEnumerable<ITeuchiUdonTypeArg> args)
            : this(null, null, args, logicalName, null, null)
        {
        }

        public TeuchiUdonType(TeuchiUdonQualifier qualifier, string name, string logicalName, string realName, Type realType)
            : this(qualifier, name, Enumerable.Empty<TeuchiUdonType>(), logicalName, realName, realType)
        {
        }

        public TeuchiUdonType(TeuchiUdonQualifier qualifier, string name, IEnumerable<ITeuchiUdonTypeArg> args, string logicalName, string realName, Type realType)
        {
            Qualifier   = qualifier;
            Name        = name;
            Args        = args?.ToArray() ?? System.Array.Empty<TeuchiUdonType>();
            LogicalName = logicalName;
            RealName    = realName;
            RealType    = realType;
        }

        public bool Equals(TeuchiUdonType obj)
        {
            return !object.ReferenceEquals(obj, null) && Qualifier == obj.Qualifier && Name == obj.Name && Args.SequenceEqual(obj.Args);
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonType type ? Equals(type) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonType obj1, TeuchiUdonType obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonType obj1, TeuchiUdonType obj2)
        {
            return !(obj1 == obj2);
        }

        public override string ToString()
        {
            return $"{Qualifier.Qualify(".", Name)}{(Args.Length == 0 ? "" : $"[{string.Join(", ", Args.Select(x => x.ToString()))}]")}";
        }

        public string GetLogicalName()
        {
            return $"{LogicalName}{string.Join("", Args.Select(x => x.GetLogicalName()))}";
        }

        public string GetRealName()
        {
            if (RealName == null)
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(null, $"no real type name");
                return "";
            }
            return RealName;
        }

        public bool LogicalTypeNameEquals(TeuchiUdonType obj)
        {
            return obj != null && LogicalName == obj.LogicalName;
        }

        public bool LogicalTypeEquals(TeuchiUdonType obj)
        {
            return obj != null && LogicalName == obj.LogicalName && Args.SequenceEqual(obj.Args, ITeuchiUdonTypeArgLogicalEqualityComparer.Instance);
        }

        public IEnumerable<(string name, TeuchiUdonType type)> GetMembers()
        {
            if (RealType == null)
            {
                return Enumerable.Empty<(string, TeuchiUdonType)>();
            }
            else if (LogicalTypeNameEquals(PrimitiveTypes.Instance.Tuple))
            {
                return GetArgsAsTuple().Select((x, i) => (i.ToString(), x));
            }
            else if (LogicalTypeNameEquals(PrimitiveTypes.Instance.Array))
            {
                return new (string, TeuchiUdonType)[] { ("", this) };
            }
            else
            {
                return new (string, TeuchiUdonType)[] { ("", this) };
            }
        }

        public int GetMemberCount()
        {
            if (RealType == null)
            {
                return 0;
            }
            else if (LogicalTypeNameEquals(PrimitiveTypes.Instance.Tuple))
            {
                return GetArgsAsTuple().Sum(x => x.GetMemberCount());
            }
            else if (LogicalTypeNameEquals(PrimitiveTypes.Instance.Array))
            {
                return 1;
            }
            else
            {
                return 1;
            }
        }

        public bool IsAssignableFrom(TeuchiUdonType obj)
        {
            return
                obj != null &&
                (
                    IsAssignableFromUnknown (obj) ||
                    IsAssignableFromAny     (obj) ||
                    IsAssignableFromBottom  (obj) ||
                    IsAssignableFromUnit    (obj) ||
                    IsAssignableFromQual    (obj) ||
                    IsAssignableFromType    (obj) ||
                    IsAssignableFromTuple   (obj) ||
                    IsAssignableFromArray   (obj) ||
                    IsAssignableFromList    (obj) ||
                    IsAssignableFromFunc    (obj) ||
                    IsAssignableFromDetFunc (obj) ||
                    IsAssignableFromSetter  (obj) ||
                    IsAssignableFromNullType(obj) ||
                    IsAssignableFromDotNet  (obj)
                );
        }

        private bool IsAssignableFromUnknown(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeEquals(PrimitiveTypes.Instance.Unknown) && !obj.LogicalTypeEquals(PrimitiveTypes.Instance.Unknown)) return false;

            return true;
        }

        private bool IsAssignableFromAny(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeEquals(PrimitiveTypes.Instance.Any)) return false;

            return true;
        }

        private bool IsAssignableFromBottom(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!obj.LogicalTypeEquals(PrimitiveTypes.Instance.Bottom)) return false;

            return true;
        }

        private bool IsAssignableFromUnit(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeEquals(PrimitiveTypes.Instance.Unit) || !obj.LogicalTypeEquals(PrimitiveTypes.Instance.Unit)) return false;

            return true;
        }

        private bool IsAssignableFromQual(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeEquals(PrimitiveTypes.Instance.Qual) || !obj.LogicalTypeNameEquals(PrimitiveTypes.Instance.Qual)) return false;

            return obj.Args.Length == 1;
        }

        private bool IsAssignableFromType(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeEquals(PrimitiveTypes.Instance.Type) || !obj.LogicalTypeNameEquals(PrimitiveTypes.Instance.Type)) return false;

            return obj.Args.Length == 1;
        }

        private bool IsAssignableFromTuple(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeNameEquals(PrimitiveTypes.Instance.Tuple) || !obj.LogicalTypeNameEquals(PrimitiveTypes.Instance.Tuple)) return false;

            return GetArgsAsTuple().Zip(obj.GetArgsAsTuple(), (t, o) => (t, o)).All(x => x.t.IsAssignableFrom(x.o));
        }

        private bool IsAssignableFromArray(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeNameEquals(PrimitiveTypes.Instance.Array) || !obj.LogicalTypeNameEquals(PrimitiveTypes.Instance.Array)) return false;

            return
                TeuchiUdonTables.Instance.Types.ContainsKey(this) && TeuchiUdonTables.Instance.Types.ContainsKey(obj) ?
                RealType.IsAssignableFrom(obj.RealType) :
                GetArgAsArray().IsAssignableFrom(obj.GetArgAsArray());
        }

        private bool IsAssignableFromList(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeNameEquals(PrimitiveTypes.Instance.List) || !obj.LogicalTypeNameEquals(PrimitiveTypes.Instance.List)) return false;

            var arrayType    =     GetArgAsListArrayType();
            var objArrayType = obj.GetArgAsListArrayType();
            return
                TeuchiUdonTables.Instance.Types.ContainsKey(arrayType) && TeuchiUdonTables.Instance.Types.ContainsKey(objArrayType) ?
                arrayType.RealType.IsAssignableFrom(objArrayType.RealType) :
                GetArgAsListElementType().IsAssignableFrom(obj.GetArgAsListElementType());
        }

        private bool IsAssignableFromFunc(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if
            (
                !    LogicalTypeNameEquals(PrimitiveTypes.Instance.Func) ||
                !obj.LogicalTypeNameEquals(PrimitiveTypes.Instance.Func) && !obj.LogicalTypeNameEquals(PrimitiveTypes.Instance.DetFunc)
            ) return false;

            return
                    GetArgAsFuncInType ().IsAssignableFrom(obj.GetArgAsFuncInType ()) &&
                obj.GetArgAsFuncOutType().IsAssignableFrom(    GetArgAsFuncOutType());
        }

        private bool IsAssignableFromDetFunc(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeNameEquals(PrimitiveTypes.Instance.DetFunc) || !obj.LogicalTypeNameEquals(PrimitiveTypes.Instance.DetFunc)) return false;

            return
                    GetArgAsFuncInType ().IsAssignableFrom(obj.GetArgAsFuncInType ()) &&
                obj.GetArgAsFuncOutType().IsAssignableFrom(    GetArgAsFuncOutType());
        }

        private bool IsAssignableFromSetter(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeNameEquals(PrimitiveTypes.Instance.Setter)) return false;

            return GetArgAsSetter().IsAssignableFrom(obj);
        }

        private bool IsAssignableFromNullType(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!IsDotNetType() && !LogicalTypeNameEquals(PrimitiveTypes.Instance.NullType) || !obj.LogicalTypeNameEquals(PrimitiveTypes.Instance.NullType)) return false;

            return true;
        }

        private bool IsAssignableFromDotNet(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!IsDotNetType() || !obj.IsDotNetType()) return false;

            return RealType != null && obj.RealType != null && RealType.IsAssignableFrom(obj.RealType);
        }

        public bool IsDotNetType()
        {
            return new TeuchiUdonType[]
            {
                PrimitiveTypes.Instance.Unknown,
                PrimitiveTypes.Instance.Invalid,
                PrimitiveTypes.Instance.Any,
                PrimitiveTypes.Instance.Bottom,
                PrimitiveTypes.Instance.Unit,
                PrimitiveTypes.Instance.Qual,
                PrimitiveTypes.Instance.Type,
                PrimitiveTypes.Instance.Tuple,
                PrimitiveTypes.Instance.List,
                PrimitiveTypes.Instance.Func,
                PrimitiveTypes.Instance.DetFunc,
                PrimitiveTypes.Instance.Method,
                PrimitiveTypes.Instance.Setter,
                PrimitiveTypes.Instance.Cast,
                PrimitiveTypes.Instance.TypeOf
            }
            .All(x => !LogicalTypeNameEquals(x));
        }

        public bool IsSyncableType()
        {
            return new TeuchiUdonType[]
            {
                PrimitiveTypes.Instance.Bool,
                PrimitiveTypes.Instance.Char,
                PrimitiveTypes.Instance.Byte,
                PrimitiveTypes.Instance.SByte,
                PrimitiveTypes.Instance.Short,
                PrimitiveTypes.Instance.UShort,
                PrimitiveTypes.Instance.Int,
                PrimitiveTypes.Instance.UInt,
                PrimitiveTypes.Instance.Long,
                PrimitiveTypes.Instance.ULong,
                PrimitiveTypes.Instance.Float,
                PrimitiveTypes.Instance.Double,
                PrimitiveTypes.Instance.String,
                PrimitiveTypes.Instance.Vector2,
                PrimitiveTypes.Instance.Vector3,
                PrimitiveTypes.Instance.Vector4,
                PrimitiveTypes.Instance.Quaternion,
                PrimitiveTypes.Instance.Color,
                PrimitiveTypes.Instance.Color32,
                PrimitiveTypes.Instance.VRCUrl
            }
            .Any(x => LogicalTypeEquals(x)) ||
            LogicalTypeNameEquals(PrimitiveTypes.Instance.Array) &&
            new TeuchiUdonType[]
            {
                PrimitiveTypes.Instance.Bool,
                PrimitiveTypes.Instance.Char,
                PrimitiveTypes.Instance.Byte,
                PrimitiveTypes.Instance.SByte,
                PrimitiveTypes.Instance.Short,
                PrimitiveTypes.Instance.UShort,
                PrimitiveTypes.Instance.Int,
                PrimitiveTypes.Instance.UInt,
                PrimitiveTypes.Instance.Long,
                PrimitiveTypes.Instance.ULong,
                PrimitiveTypes.Instance.Float,
                PrimitiveTypes.Instance.Double,
                PrimitiveTypes.Instance.Vector2,
                PrimitiveTypes.Instance.Vector3,
                PrimitiveTypes.Instance.Vector4,
                PrimitiveTypes.Instance.Quaternion,
                PrimitiveTypes.Instance.Color,
                PrimitiveTypes.Instance.Color32
            }
            .Any(x => GetArgAsArray().LogicalTypeEquals(x))
            ;
        }

        public bool IsLinearSyncableType()
        {
            return new TeuchiUdonType[]
            {
                PrimitiveTypes.Instance.Byte,
                PrimitiveTypes.Instance.SByte,
                PrimitiveTypes.Instance.Short,
                PrimitiveTypes.Instance.UShort,
                PrimitiveTypes.Instance.Int,
                PrimitiveTypes.Instance.UInt,
                PrimitiveTypes.Instance.Long,
                PrimitiveTypes.Instance.ULong,
                PrimitiveTypes.Instance.Float,
                PrimitiveTypes.Instance.Double,
                PrimitiveTypes.Instance.Vector2,
                PrimitiveTypes.Instance.Vector3,
                PrimitiveTypes.Instance.Quaternion,
                PrimitiveTypes.Instance.Color,
                PrimitiveTypes.Instance.Color32
            }
            .Any(x => LogicalTypeEquals(x));
        }

        public bool IsSmoothSyncableType()
        {
            return new TeuchiUdonType[]
            {
                PrimitiveTypes.Instance.Byte,
                PrimitiveTypes.Instance.SByte,
                PrimitiveTypes.Instance.Short,
                PrimitiveTypes.Instance.UShort,
                PrimitiveTypes.Instance.Int,
                PrimitiveTypes.Instance.UInt,
                PrimitiveTypes.Instance.Long,
                PrimitiveTypes.Instance.ULong,
                PrimitiveTypes.Instance.Float,
                PrimitiveTypes.Instance.Double,
                PrimitiveTypes.Instance.Vector2,
                PrimitiveTypes.Instance.Vector3,
                PrimitiveTypes.Instance.Quaternion
            }
            .Any(x => LogicalTypeEquals(x));
        }

        public bool IsSignedIntegerType()
        {
            return new TeuchiUdonType[]
            {
                PrimitiveTypes.Instance.SByte,
                PrimitiveTypes.Instance.Short,
                PrimitiveTypes.Instance.Int,
                PrimitiveTypes.Instance.Long
            }
            .Any(x => LogicalTypeEquals(x));
        }

        public bool IsFunc()
        {
            return LogicalTypeNameEquals(PrimitiveTypes.Instance.Func) || LogicalTypeNameEquals(PrimitiveTypes.Instance.DetFunc);
        }

        public bool ContainsUnknown()
        {
            return LogicalTypeEquals(PrimitiveTypes.Instance.Unknown) || Args.Any(x => x is TeuchiUdonType t && t.ContainsUnknown());
        }

        public bool ContainsFunc()
        {
            return IsFunc() || Args.Any(x => x is TeuchiUdonType t && t.ContainsFunc());
        }

        public bool ContainsNonDetFunc()
        {
            return LogicalTypeNameEquals(PrimitiveTypes.Instance.Func) || Args.Any(x => x is TeuchiUdonType t && t.ContainsNonDetFunc());
        }

        public TeuchiUdonType Fix(TeuchiUdonType type)
        {
            if (LogicalTypeEquals(PrimitiveTypes.Instance.Unknown))
            {
                return type;
            }
            else if (IsAssignableFrom(type) || type.IsAssignableFrom(this))
            {
                var t =
                    ApplyArgs
                    (
                        Args
                        .Zip(type.Args, (a, ta) => (a, ta))
                        .Select(x => x.a is TeuchiUdonType at && x.ta is TeuchiUdonType tat ? at.Fix(tat) : x.ta)
                    );
                if (TeuchiUdonTables.Instance.Types.ContainsKey(t))
                {
                    return TeuchiUdonTables.Instance.Types[t];
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(null, $"cannot bind type");
                    return PrimitiveTypes.Instance.Invalid;
                }
            }
            else
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(null, $"cannot bind type");
                return PrimitiveTypes.Instance.Invalid;
            }
        }

        protected TeuchiUdonType ApplyArgs(IEnumerable<ITeuchiUdonTypeArg> args)
        {
            return new TeuchiUdonType(Qualifier, Name, args, LogicalName, RealName, RealType);
        }

        public TeuchiUdonType ApplyArgAsQual(TeuchiUdonQualifier qualifier)
        {
            return ApplyArgs(new ITeuchiUdonTypeArg[] { qualifier });
        }

        public TeuchiUdonType ApplyArgAsType(TeuchiUdonType type)
        {
            return ApplyArgs(new ITeuchiUdonTypeArg[] { type });
        }

        public TeuchiUdonType ApplyArgsAsTuple(IEnumerable<TeuchiUdonType> types)
        {
            return ApplyArgs(types);
        }

        public TeuchiUdonType ApplyArgAsArray(TeuchiUdonType type)
        {
            return ApplyArgs(new ITeuchiUdonTypeArg[] { type });
        }

        public TeuchiUdonType ApplyArgAsList(TeuchiUdonType type)
        {
            return ApplyArgs(new ITeuchiUdonTypeArg[] { type, type.ToArrayType() });
        }

        public TeuchiUdonType ApplyArgsAsFunc(TeuchiUdonType inType, TeuchiUdonType outType)
        {
            return ApplyArgs(new ITeuchiUdonTypeArg[] { inType, outType });
        }

        public TeuchiUdonType ApplyArgsAsMethod(IEnumerable<TeuchiUdonMethod> methods)
        {
            return ApplyArgs(methods);
        }

        public TeuchiUdonType ApplyArgAsSetter(TeuchiUdonType type)
        {
            return ApplyArgs(new ITeuchiUdonTypeArg[] { type });
        }

        public TeuchiUdonType ApplyArgAsCast(TeuchiUdonType type)
        {
            return ApplyArgs(new ITeuchiUdonTypeArg[] { type });
        }

        public TeuchiUdonQualifier GetArgAsQual()
        {
            return (TeuchiUdonQualifier)Args[0];
        }

        public TeuchiUdonType GetArgAsType()
        {
            return (TeuchiUdonType)Args[0];
        }

        public IEnumerable<TeuchiUdonType> GetArgsAsTuple()
        {
            return Args.Cast<TeuchiUdonType>();
        }

        public TeuchiUdonType GetArgAsArray()
        {
            return (TeuchiUdonType)Args[0];
        }

        public TeuchiUdonType GetArgAsListElementType()
        {
            return (TeuchiUdonType)Args[0];
        }

        public TeuchiUdonType GetArgAsListArrayType()
        {
            return (TeuchiUdonType)Args[1];
        }

        public TeuchiUdonType GetArgAsFuncInType()
        {
            return (TeuchiUdonType)Args[0];
        }

        public TeuchiUdonType GetArgAsFuncOutType()
        {
            return (TeuchiUdonType)Args[1];
        }

        public IEnumerable<TeuchiUdonMethod> GetArgsAsMethod()
        {
            return Args.Cast<TeuchiUdonMethod>();
        }

        public TeuchiUdonType GetArgAsSetter()
        {
            return (TeuchiUdonType)Args[0];
        }

        public TeuchiUdonType GetArgAsCast()
        {
            return (TeuchiUdonType)Args[0];
        }

        public TeuchiUdonType ApplyRealType(string realName, Type realType)
        {
            return new TeuchiUdonType(Qualifier, Name, Args, LogicalName, realName, realType);
        }

        public TeuchiUdonType ToArrayType()
        {
            var qt = PrimitiveTypes.Instance.Array.ApplyArgAsArray(this);
            return
                TeuchiUdonTables.Instance.LogicalTypes.ContainsKey(qt) ?
                    TeuchiUdonTables.Instance.LogicalTypes[qt] :
                    new TeuchiUdonType
                    (
                        TeuchiUdonQualifier.Top,
                        PrimitiveTypes.Instance.Array.Name,
                        new TeuchiUdonType[] { this },
                        PrimitiveTypes.Instance.Array.LogicalName,
                        PrimitiveTypes.Instance.AnyArray.GetRealName(),
                        PrimitiveTypes.Instance.AnyArray.RealType
                    );
        }

        public static TeuchiUdonType ToOneType(IEnumerable<TeuchiUdonType> types)
        {
            var count = types.Count();
            return
                count == 0 ? PrimitiveTypes.Instance.Unit :
                count == 1 ? types.First() :
                PrimitiveTypes.Instance.Tuple.ApplyArgsAsTuple(types);
        }

        public static TeuchiUdonType GetUpperType(IEnumerable<TeuchiUdonType> types)
        {
            if (types.Any())
            {
                var upper    = types.First();
                var unknowns = new HashSet<TeuchiUdonType>();
                foreach (var t in types.Skip(1))
                {
                    if (upper.IsAssignableFrom(t))
                    {
                    }
                    else if (t.IsAssignableFrom(upper))
                    {
                        upper = t;
                    }
                    else if (!unknowns.Contains(t))
                    {
                        unknowns.Add(t);
                    }
                }

                if (unknowns.All(x => upper.IsAssignableFrom(x)))
                {
                    return upper;
                }
                else
                {
                    return PrimitiveTypes.Instance.Unknown;
                }
            }
            else
            {
                return PrimitiveTypes.Instance.Unit;
            }
        }

        public IEnumerable<TeuchiUdonMethod> GetMostCompatibleMethods(IEnumerable<TeuchiUdonType> inTypes)
        {
            if (!LogicalTypeNameEquals(PrimitiveTypes.Instance.Method)) return Enumerable.Empty<TeuchiUdonMethod>();

            var it      = inTypes.ToArray();
            var methods = GetArgsAsMethod()
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
                    if (!m.IsAssignableFrom(i))
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

    public class TeuchiUdonTypeLogicalEqualityComparer : IEqualityComparer<TeuchiUdonType>
    {
        public static TeuchiUdonTypeLogicalEqualityComparer Instance { get; } = new TeuchiUdonTypeLogicalEqualityComparer();

        protected TeuchiUdonTypeLogicalEqualityComparer()
        {
        }

        public bool Equals(TeuchiUdonType obj1, TeuchiUdonType obj2)
        {
            return obj1.LogicalTypeEquals(obj2);
        }

        public int GetHashCode(TeuchiUdonType obj)
        {
            return obj.LogicalName.GetHashCode();
        }
    }

    public class ITeuchiUdonTypeArgLogicalEqualityComparer : IEqualityComparer<ITeuchiUdonTypeArg>
    {
        public static ITeuchiUdonTypeArgLogicalEqualityComparer Instance { get; } = new ITeuchiUdonTypeArgLogicalEqualityComparer();

        protected ITeuchiUdonTypeArgLogicalEqualityComparer()
        {
        }

        public bool Equals(ITeuchiUdonTypeArg obj1, ITeuchiUdonTypeArg obj2)
        {
            return
                obj1 is TeuchiUdonQualifier q1 && obj2 is TeuchiUdonQualifier q2 && q1.Equals(q2) ||
                obj1 is TeuchiUdonMethod    m1 && obj2 is TeuchiUdonMethod    m2 && m1.Equals(m2) ||
                obj1 is TeuchiUdonType      t1 && obj2 is TeuchiUdonType      t2 && t1.LogicalTypeEquals(t2);
        }

        public int GetHashCode(ITeuchiUdonTypeArg obj)
        {
            return obj is TeuchiUdonType t ? t.LogicalName.GetHashCode() : obj.GetHashCode();
        }
    }
}
