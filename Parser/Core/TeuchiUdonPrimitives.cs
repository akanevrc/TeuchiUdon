using System;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonPrimitives
    {
        public TeuchiUdonType Unknown { get; private set; }
        public TeuchiUdonType Any { get; private set; }
        public TeuchiUdonType Bottom { get; private set; }
        public TeuchiUdonType Unit { get; private set; }
        public TeuchiUdonType Qual { get; private set; }
        public TeuchiUdonType Type { get; private set; }
        public TeuchiUdonType Tuple { get; private set; }
        public TeuchiUdonType Array { get; private set; }
        public TeuchiUdonType List { get; private set; }
        public TeuchiUdonType Func { get; private set; }
        public TeuchiUdonType NdFunc { get; private set; }
        public TeuchiUdonType Lambda { get; private set; }
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
        public TeuchiUdonType AnyArray { get; private set; }

        private bool IsInitialized { get; set; } = false;

        private TeuchiUdonDllLoader DllLoader { get; }

        public TeuchiUdonPrimitives(TeuchiUdonDllLoader dllLoader)
        {
            DllLoader = dllLoader;
        }

        public void Init()
        {
            if (!IsInitialized)
            {
                Unknown       = new TeuchiUdonType(TeuchiUdonQualifier.Top, "unknown"      , "unknown"              , null, null);
                Any           = new TeuchiUdonType(TeuchiUdonQualifier.Top, "any"          , "any"                  , "SystemObject", typeof(object));
                Bottom        = new TeuchiUdonType(TeuchiUdonQualifier.Top, "bottom"       , "bottom"               , null, null);
                Unit          = new TeuchiUdonType(TeuchiUdonQualifier.Top, "unit"         , "unit"                 , null, null);
                Qual          = new TeuchiUdonType(TeuchiUdonQualifier.Top, "qual"         , "qual"                 , null, null);
                Type          = new TeuchiUdonType(TeuchiUdonQualifier.Top, "internaltype" , "internaltype"         , null, null);
                Tuple         = new TeuchiUdonType(TeuchiUdonQualifier.Top, "tuple"        , "tuple"                , null, null);
                Array         = new TeuchiUdonType(TeuchiUdonQualifier.Top, "array"        , "array"                , null, null);
                List          = new TeuchiUdonType(TeuchiUdonQualifier.Top, "list"         , "list"                 , null, null);
                Func          = new TeuchiUdonType(TeuchiUdonQualifier.Top, "func"         , "func"                 , null, null);
                NdFunc        = new TeuchiUdonType(TeuchiUdonQualifier.Top, "ndfunc"       , "ndfunc"               , null, null);
                Lambda        = new TeuchiUdonType(TeuchiUdonQualifier.Top, "lambda"       , "lambda"               , null, null);
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
                UnityObject   = new TeuchiUdonType(TeuchiUdonQualifier.Top, "unityobject"  , "UnityEngineObject"    , "UnityEngineObject"    , DllLoader.GetTypeFromAssembly("UnityEngine.dll", "UnityEngine.Object"));
                GameObject    = new TeuchiUdonType(TeuchiUdonQualifier.Top, "gameobject"   , "UnityEngineGameObject", "UnityEngineGameObject", DllLoader.GetTypeFromAssembly("UnityEngine.dll", "UnityEngine.GameObject"));
                Vector2       = new TeuchiUdonType(TeuchiUdonQualifier.Top, "vector2"      , "UnityEngineVector2"   , "UnityEngineVector2"   , DllLoader.GetTypeFromAssembly("UnityEngine.dll", "UnityEngine.Vector2"));
                Vector3       = new TeuchiUdonType(TeuchiUdonQualifier.Top, "vector3"      , "UnityEngineVector3"   , "UnityEngineVector3"   , DllLoader.GetTypeFromAssembly("UnityEngine.dll", "UnityEngine.Vector3"));
                Vector4       = new TeuchiUdonType(TeuchiUdonQualifier.Top, "vector4"      , "UnityEngineVector4"   , "UnityEngineVector4"   , DllLoader.GetTypeFromAssembly("UnityEngine.dll", "UnityEngine.Vector4"));
                Quaternion    = new TeuchiUdonType(TeuchiUdonQualifier.Top, "quaternion"   , "UnityEngineQuaternion", "UnityEngineQuaternion", DllLoader.GetTypeFromAssembly("UnityEngine.dll", "UnityEngine.Quaternion"));
                Color         = new TeuchiUdonType(TeuchiUdonQualifier.Top, "color"        , "UnityEngineColor"     , "UnityEngineColor"     , DllLoader.GetTypeFromAssembly("UnityEngine.dll", "UnityEngine.Color"));
                Color32       = new TeuchiUdonType(TeuchiUdonQualifier.Top, "color32"      , "UnityEngineColor32"   , "UnityEngineColor32"   , DllLoader.GetTypeFromAssembly("UnityEngine.dll", "UnityEngine.Color32"));
                VRCUrl        = new TeuchiUdonType(TeuchiUdonQualifier.Top, "vrcurl"       , "VRCSDKBaseVRCUrl"     , "VRCSDKBaseVRCUrl"     , DllLoader.GetTypeFromAssembly("VRCSDKBase.dll" , "VRC.SDKBase.VRCUrl"));
                UdonBehaviour = new TeuchiUdonType(TeuchiUdonQualifier.Top, "udonbehaviour", "VRCUdonUdonBehaviour" , "VRCUdonUdonBehaviour" , DllLoader.GetTypeFromAssembly("VRC.Udon.dll"   , "VRC.Udon.UdonBehaviour"));
                AnyArray      = new TeuchiUdonType(TeuchiUdonQualifier.Top, "anyarray", new TeuchiUdonType[] { new TeuchiUdonType(TeuchiUdonQualifier.Top, "object", "SystemObject", "SystemObject", typeof(object)) } , "array", "SystemObjectArray", typeof(object[]));
                IsInitialized = true;
            }
        }
    }
}
