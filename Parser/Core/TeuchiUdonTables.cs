using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonTables
    {
        public static TeuchiUdonTables Instance { get; } = new TeuchiUdonTables();

        public Dictionary<TeuchiUdonQualifier, TeuchiUdonQualifier> Qualifiers { get; private set; }
        public Dictionary<TeuchiUdonType, TeuchiUdonType> Types { get; private set; }
        public Dictionary<TeuchiUdonType, TeuchiUdonType> LogicalTypes { get; private set; }
        public Dictionary<TeuchiUdonType, TeuchiUdonType> GenericRootTypes { get; private set; }
        public Dictionary<TeuchiUdonMethod, TeuchiUdonMethod> Methods { get; private set; }
        public Dictionary<TeuchiUdonType, Dictionary<string, List<TeuchiUdonMethod>>> TypeToMethods { get; private set; }
        public Dictionary<string, TeuchiUdonMethod> Events { get; private set; }
        public Dictionary<TeuchiUdonVar, TeuchiUdonVar> Vars { get; private set; }
        public Dictionary<TeuchiUdonVar, TeuchiUdonLiteral> PublicVars { get; private set; }
        public Dictionary<TeuchiUdonVar, TeuchiUdonSyncMode> SyncedVars { get; private set; }
        public Dictionary<TeuchiUdonVar, TeuchiUdonFunc> EventFuncs { get; private set; }
        public Dictionary<TeuchiUdonLiteral, TeuchiUdonLiteral> Literals { get; private set; }
        public Dictionary<TeuchiUdonThis, TeuchiUdonThis> This { get; private set; }
        public Dictionary<TeuchiUdonFunc, TeuchiUdonFunc> Funcs { get; private set; }
        public Dictionary<TeuchiUdonIndirect, uint> Indirects { get; private set; }
        public Dictionary<TeuchiUdonBlock, TeuchiUdonBlock> Blocks { get; private set; }
        public HashSet<IDataLabel> UsedData { get; set; }

        private int VarCounter { get; set; }
        private int OutValueCounter { get; set; }
        private int LiteralCounter { get; set; }
        private int FuncCounter { get; set; }
        private int VarBindCounter { get; set; }
        private int BlockCounter { get; set; }
        private int EvalFuncCounter { get; set; }
        private int LetInCounter { get; set; }
        private int BranchCounter { get; set; }
        private int LoopCounter { get; set; }
        private int IndirectCounter { get; set; }

        private bool IsInitialized { get; set; } = false;

        protected TeuchiUdonTables()
        {
        }

        public void Init()
        {
            if (!IsInitialized)
            {
                Qualifiers       = new Dictionary<TeuchiUdonQualifier, TeuchiUdonQualifier>();
                Types            = new Dictionary<TeuchiUdonType     , TeuchiUdonType>();
                LogicalTypes     = new Dictionary<TeuchiUdonType     , TeuchiUdonType>(TeuchiUdonTypeLogicalEqualityComparer.Instance);
                GenericRootTypes = new Dictionary<TeuchiUdonType     , TeuchiUdonType>();
                Methods          = new Dictionary<TeuchiUdonMethod   , TeuchiUdonMethod>();
                TypeToMethods    = new Dictionary<TeuchiUdonType     , Dictionary<string, List<TeuchiUdonMethod>>>(TeuchiUdonTypeLogicalEqualityComparer.Instance);
                Events           = new Dictionary<string             , TeuchiUdonMethod>();

                InitInternalTypes();
                InitExternalTypesAndMethods();
                InitQualifiers();

                IsInitialized = true;
            }

            Vars       = new Dictionary<TeuchiUdonVar     , TeuchiUdonVar>();
            PublicVars = new Dictionary<TeuchiUdonVar     , TeuchiUdonLiteral>();
            SyncedVars = new Dictionary<TeuchiUdonVar     , TeuchiUdonSyncMode>();
            EventFuncs = new Dictionary<TeuchiUdonVar     , TeuchiUdonFunc>();
            Literals   = new Dictionary<TeuchiUdonLiteral , TeuchiUdonLiteral>();
            This       = new Dictionary<TeuchiUdonThis    , TeuchiUdonThis>();
            Funcs      = new Dictionary<TeuchiUdonFunc    , TeuchiUdonFunc>();
            Indirects  = new Dictionary<TeuchiUdonIndirect, uint>();
            Blocks     = new Dictionary<TeuchiUdonBlock   , TeuchiUdonBlock>();
            UsedData   = new HashSet<IDataLabel>();

            VarCounter      = 0;
            OutValueCounter = 0;
            LiteralCounter  = 0;
            FuncCounter     = 0;
            VarBindCounter  = 0;
            BlockCounter    = 0;
            EvalFuncCounter = 0;
            LetInCounter    = 0;
            BranchCounter   = 0;
            LoopCounter     = 0;
            IndirectCounter = 0;
        }

        private void InitInternalTypes()
        {
            var types = new TeuchiUdonType[]
            {
                PrimitiveTypes.Instance.Any,
                PrimitiveTypes.Instance.Bottom,
                PrimitiveTypes.Instance.Unit,
                PrimitiveTypes.Instance.NullType,
                PrimitiveTypes.Instance.Object,
                PrimitiveTypes.Instance.DotNetType,
                PrimitiveTypes.Instance.Bool,
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
                PrimitiveTypes.Instance.Decimal,
                PrimitiveTypes.Instance.DateTime,
                PrimitiveTypes.Instance.Char,
                PrimitiveTypes.Instance.String,
                PrimitiveTypes.Instance.UnityObject,
                PrimitiveTypes.Instance.GameObject,
                PrimitiveTypes.Instance.Vector2,
                PrimitiveTypes.Instance.Vector3,
                PrimitiveTypes.Instance.Vector4,
                PrimitiveTypes.Instance.Quaternion,
                PrimitiveTypes.Instance.Color,
                PrimitiveTypes.Instance.Color32,
                PrimitiveTypes.Instance.VRCUrl,
                PrimitiveTypes.Instance.UdonBehaviour
            };

            foreach (var t in types)
            {
                Types       .Add(t, t);
                LogicalTypes.Add(t, t);
            }

            var genericRoots = new TeuchiUdonType[]
            {
                PrimitiveTypes.Instance.Qual,
                PrimitiveTypes.Instance.Type,
                PrimitiveTypes.Instance.Tuple,
                PrimitiveTypes.Instance.Array,
                PrimitiveTypes.Instance.List,
                PrimitiveTypes.Instance.Func,
                PrimitiveTypes.Instance.DetFunc
            };

            foreach (var t in genericRoots)
            {
                GenericRootTypes.Add(t, t);
            }
        }

        private void InitExternalTypesAndMethods()
        {
            var udonBehaviour     = RegisterAndGetType(TeuchiUdonDllLoader.Instance.GetTypeFromAssembly("VRC.Udon.dll"       , "VRC.Udon.UdonBehaviour"));
            var component         = RegisterAndGetType(TeuchiUdonDllLoader.Instance.GetTypeFromAssembly("UnityEngine.dll"    , "UnityEngine.Component"));
            var udonEventReceiver = RegisterAndGetType(TeuchiUdonDllLoader.Instance.GetTypeFromAssembly("VRC.Udon.Common.dll", "VRC.Udon.Common.Interfaces.IUdonEventReceiver"));

            var parameterType     = TeuchiUdonDllLoader.Instance.GetTypeFromAssembly("VRC.Udon.Graph.dll" , "VRC.Udon.Graph.UdonNodeParameter").GetNestedType("ParameterType");
            var udonEditorManager = TeuchiUdonDllLoader.Instance.GetTypeFromAssembly("VRC.Udon.Editor.dll", "VRC.Udon.Editor.UdonEditorManager");
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
                        var instanceType      = instanceDef.t == null ? PrimitiveTypes.Instance.Type.ApplyArgAsType(type) : instanceDef.t;
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
                t = PrimitiveTypes.Instance.Array
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
                    var genericName = qual.Qualify("", typeName);
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
            if (method.Type.LogicalTypeEquals(PrimitiveTypes.Instance.Type.ApplyArgAsType(new TeuchiUdonType("SystemVoid")))) return;

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

        public IEnumerable<TeuchiUdonMethod> GetMostCompatibleMethods(TeuchiUdonMethod query)
        {
            return GetMostCompatibleMethodsCore(query, query.InTypes.Length, false);
        }

        public IEnumerable<TeuchiUdonMethod> GetMostCompatibleMethodsWithoutInTypes(TeuchiUdonMethod query, int inTypeCount)
        {
            return GetMostCompatibleMethodsCore(query, inTypeCount, true);
        }

        private IEnumerable<TeuchiUdonMethod> GetMostCompatibleMethodsCore(TeuchiUdonMethod query, int inTypeCount, bool withoutInTypes)
        {
            if (!TypeToMethods.ContainsKey(query.Type)) return Enumerable.Empty<TeuchiUdonMethod>();
            var methodToMethods = TypeToMethods[query.Type];

            if (!methodToMethods.ContainsKey(query.Name)) return Enumerable.Empty<TeuchiUdonMethod>();
            var methods = methodToMethods[query.Name]
                .Where(x => x.InTypes.Length == inTypeCount)
                .ToArray();

            if (methods.Length == 0) return Enumerable.Empty<TeuchiUdonMethod>();
            if (withoutInTypes) return methods;

            var justCountToMethods = new Dictionary<int, List<TeuchiUdonMethod>>();
            foreach (var method in methods)
            {
                var isCompatible = true;
                var justCount    = 0;
                foreach (var (m, q) in method.InTypes.Zip(query.InTypes, (m, q) => (m, q)))
                {
                    if (!m.IsAssignableFrom(q))
                    {
                        isCompatible = false;
                        break;
                    }

                    if (m.LogicalTypeEquals(q)) justCount++;
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

            for (var i = query.InTypes.Length; i >= 0; i--)
            {
                if (justCountToMethods.ContainsKey(i)) return justCountToMethods[i];
            }

            return Enumerable.Empty<TeuchiUdonMethod>();
        }

        public int GetVarIndex()
        {
            return VarCounter++;
        }

        public int GetLiteralIndex()
        {
            return LiteralCounter++;
        }

        public int GetFuncIndex()
        {
            return FuncCounter++;
        }

        public int GetVarBindIndex()
        {
            return VarBindCounter++;
        }

        public int GetBlockIndex()
        {
            return BlockCounter++;
        }

        public int GetEvalFuncIndex()
        {
            return EvalFuncCounter++;
        }

        public int GetLetInIndex()
        {
            return LetInCounter++;
        }

        public int GetBranchIndex()
        {
            return BranchCounter++;
        }

        public int GetLoopIndex()
        {
            return LoopCounter++;
        }

        public int GetIndirectIndex()
        {
            return IndirectCounter++;
        }

        public static string GetGenericTypeName(TeuchiUdonType rootType, IEnumerable<TeuchiUdonType> argTypes)
        {
            return $"{rootType.LogicalName}{string.Join("", argTypes.Select(x => x.LogicalName))}";
        }

        public static string GetGetterName(string name)
        {
            return $"get_{name}";
        }

        public static string GetSetterName(string name)
        {
            return $"set_{name}";
        }

        public static string GetEventName(string name)
        {
            return name.Length <= 1 ? name : $"_{char.ToLower(name[0])}{name.Substring(1)}";
        }

        public static string GetEventParamName(string eventName, string paramName)
        {
            var ev    = eventName.Length == 0 ? eventName : $"{char.ToLower(eventName[0])}{eventName.Substring(1)}";
            var param = paramName.Length == 0 ? paramName : $"{char.ToUpper(paramName[0])}{paramName.Substring(1)}";
            return $"{ev}{param}";
        }

        public static bool IsValidVarName(string name)
        {
            return name.Length >= 1 && !name.StartsWith("_");
        }

        public IEnumerable<(string name, object value, Type type)> GetDefaultValues()
        {
            return
                PublicVars             .Where(x => UsedData.Contains(x.Key)).Select(x => (x.Key.GetFullLabel(), x.Value.Value  , x.Key.Type.RealType))
                .Concat(Literals.Values.Where(x => UsedData.Contains(x    )).Select(x => (x    .GetFullLabel(), x.Value        , x.Type    .RealType)))
                .Concat(Indirects      .Where(x => UsedData.Contains(x.Key)).Select(x => (x.Key.GetFullLabel(), (object)x.Value, PrimitiveTypes.Instance.UInt.RealType)));
        }
    }
}
