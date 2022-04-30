using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonStaticTables
    {
        public Dictionary<TeuchiUdonQualifier, TeuchiUdonQualifier> Qualifiers { get; }
        public Dictionary<TeuchiUdonType, TeuchiUdonType> Types { get; }
        public Dictionary<TeuchiUdonType, TeuchiUdonType> LogicalTypes { get; }
        public Dictionary<TeuchiUdonType, TeuchiUdonType> GenericRootTypes { get; }
        public Dictionary<TeuchiUdonMethod, TeuchiUdonMethod> Methods { get; }
        public Dictionary<TeuchiUdonType, Dictionary<string, List<TeuchiUdonMethod>>> TypeToMethods { get; }
        public Dictionary<string, TeuchiUdonMethod> Events { get; }

        private bool IsInitialized { get; set; } = false;

        private TeuchiUdonDllLoader DllLoader { get; }
        private TeuchiUdonPrimitives Primitives { get; set; }

        public TeuchiUdonStaticTables(TeuchiUdonDllLoader dllLoader, TeuchiUdonPrimitives primitives)
        {
            DllLoader  = dllLoader;
            Primitives = primitives;

            Qualifiers       = new Dictionary<TeuchiUdonQualifier, TeuchiUdonQualifier>();
            Types            = new Dictionary<TeuchiUdonType     , TeuchiUdonType>();
            LogicalTypes     = new Dictionary<TeuchiUdonType     , TeuchiUdonType>(TeuchiUdonTypeLogicalEqualityComparer.Instance);
            GenericRootTypes = new Dictionary<TeuchiUdonType     , TeuchiUdonType>();
            Methods          = new Dictionary<TeuchiUdonMethod   , TeuchiUdonMethod>();
            TypeToMethods    = new Dictionary<TeuchiUdonType     , Dictionary<string, List<TeuchiUdonMethod>>>(TeuchiUdonTypeLogicalEqualityComparer.Instance);
            Events           = new Dictionary<string             , TeuchiUdonMethod>();
        }

        public void Init()
        {
            if (!IsInitialized)
            {
                InitInternalTypes();
                InitExternalTypesAndMethods();
                InitQualifiers();
                IsInitialized = true;
            }
        }

        private void InitInternalTypes()
        {
            var types = new TeuchiUdonType[]
            {
                Primitives.Any,
                Primitives.Bottom,
                Primitives.Unit,
                Primitives.NullType,
                Primitives.Object,
                Primitives.DotNetType,
                Primitives.Bool,
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
                Primitives.Decimal,
                Primitives.DateTime,
                Primitives.Char,
                Primitives.String,
                Primitives.UnityObject,
                Primitives.GameObject,
                Primitives.Vector2,
                Primitives.Vector3,
                Primitives.Vector4,
                Primitives.Quaternion,
                Primitives.Color,
                Primitives.Color32,
                Primitives.VRCUrl,
                Primitives.UdonBehaviour
            };

            foreach (var t in types)
            {
                Types       .Add(t, t);
                LogicalTypes.Add(t, t);
            }

            var genericRoots = new TeuchiUdonType[]
            {
                Primitives.Qual,
                Primitives.Type,
                Primitives.Tuple,
                Primitives.Array,
                Primitives.List,
                Primitives.Func,
                Primitives.DetFunc
            };

            foreach (var t in genericRoots)
            {
                GenericRootTypes.Add(t, t);
            }
        }

        private void InitExternalTypesAndMethods()
        {
            var udonBehaviour     = RegisterAndGetType(DllLoader.GetTypeFromAssembly("VRC.Udon.dll"       , "VRC.Udon.UdonBehaviour"));
            var component         = RegisterAndGetType(DllLoader.GetTypeFromAssembly("UnityEngine.dll"    , "UnityEngine.Component"));
            var udonEventReceiver = RegisterAndGetType(DllLoader.GetTypeFromAssembly("VRC.Udon.Common.dll", "VRC.Udon.Common.Interfaces.IUdonEventReceiver"));

            var parameterType     = DllLoader.GetTypeFromAssembly("VRC.Udon.Graph.dll" , "VRC.Udon.Graph.UdonNodeParameter").GetNestedType("ParameterType");
            var udonEditorManager = DllLoader.GetTypeFromAssembly("VRC.Udon.Editor.dll", "VRC.Udon.Editor.UdonEditorManager");
            dynamic uemInstance   = udonEditorManager.GetProperty("Instance").GetGetMethod().Invoke(null, null);

            var topRegistries = uemInstance.GetTopRegistries();
            foreach (var topReg in topRegistries)
            {
                foreach (var reg in topReg.Value)
                {
                    foreach (var def in reg.Value.GetNodeDefinitions())
                    {
                        if (def.type == null) continue;

                        var type = RegisterAndGetType(def.type);
                        var qual = type.Qualifier;

                        var methodName    = def.name.Substring(def.name.LastIndexOf(' ') + 1);
                        var parameters    = (IEnumerable<dynamic>)def.parameters;
                        var allParamTypes =
                            parameters
                            .Select(x => RegisterAndGetType(x.type))
                            .Cast<TeuchiUdonType>()
                            .ToArray();
                        var allParamInOuts =
                            parameters
                            .Select(x =>
                                ((int)x.parameterType) == GetEnumValue(parameterType, "IN")     ? TeuchiUdonMethodParamInOut.In    :
                                ((int)x.parameterType) == GetEnumValue(parameterType, "IN_OUT") ? TeuchiUdonMethodParamInOut.InOut : TeuchiUdonMethodParamInOut.Out
                            )
                            .Cast<TeuchiUdonMethodParamInOut>()
                            .ToArray();
                        var inTypes =
                            allParamTypes
                            .Zip(allParamInOuts, (t, p) => (t, p))
                            .Where(x => x.p == TeuchiUdonMethodParamInOut.In || x.p == TeuchiUdonMethodParamInOut.InOut)
                            .Select(x => x.t)
                            .ToArray();
                        var outTypes =
                            allParamTypes
                            .Zip(allParamInOuts, (t, p) => (t, p))
                            .Where(x => x.p == TeuchiUdonMethodParamInOut.Out)
                            .Select(x => x.t)
                            .ToArray();
                        var instanceDef =
                            allParamTypes
                            .Zip(parameters, (t, p) => (t, p))
                            .FirstOrDefault(x => ((int)x.p.parameterType) == GetEnumValue(parameterType, "IN") && x.p.name == "instance");
                        var instanceType      = instanceDef.t == null ? Primitives.Type.ApplyArgAsType(type) : instanceDef.t;
                        var allParamUdonNames =
                            parameters
                            .Select(x => x.name)
                            .Cast<string>();

                        AddMethod(new TeuchiUdonMethod(instanceType, methodName, allParamTypes, inTypes, outTypes, allParamInOuts, def.fullName, allParamUdonNames));
                        if (instanceType.LogicalTypeEquals(component) || instanceType.LogicalTypeEquals(udonEventReceiver))
                        {
                            AddMethod(new TeuchiUdonMethod(udonBehaviour, methodName, allParamTypes, inTypes, outTypes, allParamInOuts, def.fullName, allParamUdonNames));
                        }
                    }
                }
            }
        }

        private TeuchiUdonType RegisterAndGetType(Type type)
        {
            var udonTypeName = GetUdonTypeName(type);

            var t = (TeuchiUdonType)null;
            if (type.IsArray)
            {
                var elemType = RegisterAndGetType(type.GetElementType());
                t = Primitives.Array
                    .ApplyArgAsArray(elemType)
                    .ApplyRealType(udonTypeName, type);
                
                if (!Types.ContainsKey(t))
                {
                    Types.Add(t, t);
                }
                if (!LogicalTypes.ContainsKey(t))
                {
                    LogicalTypes.Add(t, t);
                }
            }
            else
            {
                var typeName = GetTypeName(type);
                var argTypes = type.GenericTypeArguments.Select(x => RegisterAndGetType(x)).ToArray();
                var scopes   = GetQualifierScopes(type);
                var qual     = new TeuchiUdonQualifier(scopes);

                t = new TeuchiUdonType(qual, typeName, argTypes, udonTypeName, udonTypeName, type);

                if (!Types.ContainsKey(t))
                {
                    Types.Add(t, t);
                }
                if (!LogicalTypes.ContainsKey(t))
                {
                    LogicalTypes.Add(t, t);
                }

                if (argTypes.Length != 0)
                {
                    var genericName = GetQualifiedName(qual, typeName);
                    var genericRoot = new TeuchiUdonType(qual, typeName, genericName, null, null);

                    if (!GenericRootTypes.ContainsKey(genericRoot))
                    {
                        GenericRootTypes.Add(genericRoot, genericRoot);
                    }
                }
            }

            return t;
        }

        private static string GetUdonTypeName(Type type)
        {
            var genericIndex = type.FullName.IndexOf('`');
            var arrayIndex   = type.FullName.IndexOf('[');
            var index        = genericIndex == -1 ? arrayIndex : arrayIndex == -1 ? genericIndex : genericIndex < arrayIndex ? genericIndex : arrayIndex;
            var name         = index == -1 ? type.FullName : type.FullName.Substring(0, index);
            name = name.Replace(".", "");
            name = name.Replace("+", "");
            name = $"{name}{(type.IsGenericType ? string.Join("", type.GenericTypeArguments.Select(x => GetUdonTypeName(x))) : "")}";
            name = $"{name}{(type.IsArray ? "Array" : "")}";
            return name;
        }

        private static string GetTypeName(Type type)
        {
            var genericIndex = type.Name.IndexOf('`');
            var arrayIndex   = type.Name.IndexOf('[');
            var index        = genericIndex == -1 ? arrayIndex : arrayIndex == -1 ? genericIndex : genericIndex < arrayIndex ? genericIndex : arrayIndex;
            var name         = index == -1 ? type.Name : type.Name.Substring(0, index);
            name = $"{name}{(type.IsArray ? "Array" : "")}";
            return name;
        }

        private static IEnumerable<TeuchiUdonScope> GetQualifierScopes(Type type)
        {
            var genericIndex = type.FullName.IndexOf('`');
            var typeName     = genericIndex == -1 ? type.FullName : type.FullName.Substring(0, genericIndex);
            var qualNames    = typeName.Split(new char[] { '.', '+' });
            return qualNames.Take(qualNames.Length - 1).Select(x => new TeuchiUdonScope(x, TeuchiUdonScopeMode.Type));
        }

        private static string GetQualifiedName(TeuchiUdonQualifier qualifier, string name)
        {
            return string.Join("", qualifier.Logical.Select(x => ((TextLabel)x.Label).Text).Concat(new string[] { name }));
        }

        private void AddMethod(TeuchiUdonMethod method)
        {
            if (method.UdonName.StartsWith("Const_"   )) return;
            if (method.UdonName.StartsWith("Variable_")) return;
            if (method.UdonName.StartsWith("Event_"))
            {
                if (!Events.ContainsKey(method.Name))
                {
                    Events.Add(method.Name, method);
                }
                return;
            }
            if (method.Type.LogicalTypeEquals(Primitives.Type.ApplyArgAsType(new TeuchiUdonType("SystemVoid")))) return;

            if (method.UdonName.EndsWith("__T")) return;

            if (!Methods.ContainsKey(method))
            {
                Methods.Add(method, method);
            }

            if (!TypeToMethods.ContainsKey(method.Type))
            {
                TypeToMethods.Add(method.Type, new Dictionary<string, List<TeuchiUdonMethod>>());
            }
            var nameToMethods = TypeToMethods[method.Type];

            if (!nameToMethods.ContainsKey(method.Name))
            {
                nameToMethods.Add(method.Name, new List<TeuchiUdonMethod>());
            }
            nameToMethods[method.Name].Add(method);
        }

        private int GetEnumValue(Type enumType, string name)
        {
            return Enum.GetNames(enumType).Cast<string>().Zip(Enum.GetValues(enumType).Cast<int>(), (k, v) => (k, v)).First(x => x.k == name).v;
        }

        private void InitQualifiers()
        {
            foreach (var t in Types.Values)
            {
                for (var i = 1; i <= t.Qualifier.Logical.Length; i++)
                {
                    var names = t.Qualifier.Logical.Take(i);
                    var qual  = new TeuchiUdonQualifier(names);
                    if (!Qualifiers.ContainsKey(qual)) Qualifiers.Add(qual, qual);
                }
            }
        }
    }
}
