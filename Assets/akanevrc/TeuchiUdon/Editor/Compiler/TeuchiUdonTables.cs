using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VRC.Udon.Editor;
using VRC.Udon.Graph;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonTables
    {
        public static TeuchiUdonTables Instance { get; } = new TeuchiUdonTables();

        public Dictionary<TeuchiUdonQualifier, TeuchiUdonQualifier> Qualifiers { get; private set; }
        public Dictionary<TeuchiUdonType, TeuchiUdonType> Types { get; private set; }
        public Dictionary<TeuchiUdonType, TeuchiUdonType> LogicalTypes { get; private set; }
        public Dictionary<TeuchiUdonType, TeuchiUdonType> GenericRootTypes { get; private set; }
        public Dictionary<TeuchiUdonMethod, TeuchiUdonMethod> Methods { get; private set; }
        public Dictionary<TeuchiUdonType, Dictionary<string, Dictionary<int, List<TeuchiUdonMethod>>>> TypeToMethods { get; private set; }
        public Dictionary<string, TeuchiUdonMethod> Events { get; private set; }
        public Dictionary<TeuchiUdonVar, TeuchiUdonVar> Vars { get; private set; }
        public Dictionary<TeuchiUdonVar, TeuchiUdonVar> UnbufferedVars { get; private set; }
        public Dictionary<TeuchiUdonVar, TeuchiUdonLiteral> PublicVars { get; private set; }
        public Dictionary<TeuchiUdonVar, TeuchiUdonSyncMode> SyncedVars { get; private set; }
        public Dictionary<TeuchiUdonLiteral, TeuchiUdonLiteral> Literals { get; private set; }
        public Dictionary<TeuchiUdonThis, TeuchiUdonThis> This { get; private set; }
        public Dictionary<TeuchiUdonFunc, TeuchiUdonFunc> Funcs { get; private set; }
        public Dictionary<TeuchiUdonIndirect, uint> Indirects { get; private set; }

        private int VarCounter { get; set; }
        private int OutValueCounter { get; set; }
        private int LiteralCounter { get; set; }
        private int FuncCounter { get; set; }
        private int VarBindCounter { get; set; }
        private int BlockCounter { get; set; }
        private int EvalFuncCounter { get; set; }
        private int LetInCounter { get; set; }
        private int BranchCounter { get; set; }
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
                TypeToMethods    = new Dictionary<TeuchiUdonType     , Dictionary<string, Dictionary<int, List<TeuchiUdonMethod>>>>(TeuchiUdonTypeLogicalEqualityComparer.Instance);
                Events           = new Dictionary<string             , TeuchiUdonMethod>();

                InitInternalTypes();
                InitExternalTypes();
                InitQualifiers();
                InitMethods();

                IsInitialized = true;
            }

            Vars           = new Dictionary<TeuchiUdonVar     , TeuchiUdonVar>();
            UnbufferedVars = new Dictionary<TeuchiUdonVar     , TeuchiUdonVar>();
            PublicVars     = new Dictionary<TeuchiUdonVar     , TeuchiUdonLiteral>();
            SyncedVars     = new Dictionary<TeuchiUdonVar     , TeuchiUdonSyncMode>();
            Literals       = new Dictionary<TeuchiUdonLiteral , TeuchiUdonLiteral>();
            This           = new Dictionary<TeuchiUdonThis    , TeuchiUdonThis>();
            Funcs          = new Dictionary<TeuchiUdonFunc    , TeuchiUdonFunc>();
            Indirects      = new Dictionary<TeuchiUdonIndirect, uint>();

            VarCounter      = 0;
            OutValueCounter = 0;
            LiteralCounter  = 0;
            FuncCounter     = 0;
            VarBindCounter  = 0;
            BlockCounter    = 0;
            EvalFuncCounter = 0;
            LetInCounter    = 0;
            BranchCounter   = 0;
            IndirectCounter = 0;
        }

        private void InitInternalTypes()
        {
            var types = new TeuchiUdonType[]
            {
                TeuchiUdonType.Unknown,
                TeuchiUdonType.Any,
                TeuchiUdonType.Bottom,
                TeuchiUdonType.Unit,
                TeuchiUdonType.Object,
                TeuchiUdonType.DotNetType,
                TeuchiUdonType.Bool,
                TeuchiUdonType.Byte,
                TeuchiUdonType.SByte,
                TeuchiUdonType.Short,
                TeuchiUdonType.UShort,
                TeuchiUdonType.Int,
                TeuchiUdonType.UInt,
                TeuchiUdonType.Long,
                TeuchiUdonType.ULong,
                TeuchiUdonType.Float,
                TeuchiUdonType.Double,
                TeuchiUdonType.Decimal,
                TeuchiUdonType.DateTime,
                TeuchiUdonType.Char,
                TeuchiUdonType.String,
                TeuchiUdonType.UnityObject,
                TeuchiUdonType.GameObject
            };

            foreach (var t in types)
            {
                Types       .Add(t, t);
                LogicalTypes.Add(t, t);
            }

            var genericRoots = new TeuchiUdonType[]
            {
                TeuchiUdonType.Qual,
                TeuchiUdonType.Type,
                TeuchiUdonType.Tuple,
                TeuchiUdonType.List,
                TeuchiUdonType.Func,
                TeuchiUdonType.Method
            };

            foreach (var t in genericRoots)
            {
                GenericRootTypes.Add(t, t);
            }
        }

        private void InitExternalTypes()
        {
            var topRegistries = UdonEditorManager.Instance.GetTopRegistries();
            foreach (var topReg in topRegistries)
            {
                foreach (var reg in topReg.Value)
                {
                    foreach (var def in reg.Value.GetNodeDefinitions())
                    {
                        if (def.type == null) continue;

                        RegisterAndGetType(def.type);
                        foreach (var t in def.parameters.Select(x => x.type)) RegisterAndGetType(t);
                    }
                }
            }
        }

        private TeuchiUdonType RegisterAndGetType(Type type)
        {
            var udonTypeName = GetUdonTypeName(type);
            
            var typeName = GetTypeName(type);
            var scopes   = GetQualifierScopes(type);

            var argTypes = type.GenericTypeArguments.Select(x => RegisterAndGetType(x)).ToArray();

            var qual = new TeuchiUdonQualifier(scopes, scopes);
            var t    = new TeuchiUdonType(qual, typeName, argTypes, udonTypeName, udonTypeName, type);

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

            if (type.IsArray)
            {
                var elemType  = RegisterAndGetType(type.GetElementType());
                var arrayType = new TeuchiUdonType
                (
                    TeuchiUdonQualifier.Top,
                    TeuchiUdonType.List.Name,
                    new TeuchiUdonType[] { elemType },
                    TeuchiUdonType.List.LogicalName,
                    udonTypeName,
                    type
                );

                if (!Types.ContainsKey(arrayType))
                {
                    Types.Add(arrayType, arrayType);
                }
                if (!LogicalTypes.ContainsKey(arrayType))
                {
                    LogicalTypes.Add(arrayType, arrayType);
                }
            }

            return t;
        }

        private static string GetUdonTypeName(Type type)
        {
            var name = type.FullName;
            name = name.Replace(".", "");
            name = name.Replace("+", "");
            name = Regex.Replace(name, "`.*$"   , "");
            name = Regex.Replace(name, @"\[.*\]", "");
            name = $"{name}{(type.IsGenericType ? string.Join("", type.GenericTypeArguments.Select(x => GetUdonTypeName(x))) : "")}";
            name = $"{name}{(type.IsArray ? "Array" : "")}";
            return name;
        }

        private static string GetTypeName(Type type)
        {
            var name = type.Name;
            name = Regex.Replace(name, "`.*$"   , "");
            name = Regex.Replace(name, @"\[.*\]", "");
            name = $"{name}{(type.IsArray ? "Array" : "")}";
            return name;
        }

        private static IEnumerable<TeuchiUdonScope> GetQualifierScopes(Type type)
        {
            var typeName  = Regex.Replace(type.FullName, "`.*$"  , "");
            var qualNames = typeName.Split(new string[] { ".", "+" }, StringSplitOptions.None);
            return qualNames.Take(qualNames.Length - 1).Select(x => new TeuchiUdonScope(x, TeuchiUdonScopeMode.Type));
        }

        private void InitQualifiers()
        {
            foreach (var t in Types.Values)
            {
                for (var i = 1; i <= t.Qualifier.Logical.Length; i++)
                {
                    var names = t.Qualifier.Logical.Take(i);
                    var qual  = new TeuchiUdonQualifier(names, names);
                    if (!Qualifiers.ContainsKey(qual)) Qualifiers.Add(qual, qual);
                }
            }
        }

        private void InitMethods()
        {
            var topRegistries = UdonEditorManager.Instance.GetTopRegistries();
            foreach (var topReg in topRegistries)
            {
                foreach (var reg in topReg.Value)
                {
                    foreach (var def in reg.Value.GetNodeDefinitions())
                    {
                        if (def.type == null) continue;

                        var type = RegisterAndGetType(def.type);
                        var qual = type.Qualifier;

                        var methodNames  = def.name.Split(new string[] { " " }, StringSplitOptions.None);
                        var methodName   = methodNames[methodNames.Length - 1];
                        var instanceDef = def.parameters
                            .FirstOrDefault(x => x.parameterType == UdonNodeParameter.ParameterType.IN && x.name == "instance");
                        var instanceType = instanceDef == null ? TeuchiUdonType.Type.ApplyArgAsType(type) : RegisterAndGetType(instanceDef.type);
                        var allParamTypes = def.parameters
                            .Select(x => RegisterAndGetType(x.type));
                        var inTypes = def.parameters
                            .Where(x => x.parameterType == UdonNodeParameter.ParameterType.IN || x.parameterType == UdonNodeParameter.ParameterType.IN_OUT)
                            .Select(x => RegisterAndGetType(x.type)).ToArray();
                        var outTypes = def.parameters
                            .Where(x => x.parameterType == UdonNodeParameter.ParameterType.OUT)
                            .Select(x => RegisterAndGetType(x.type));
                        var allParamInOuts = def.parameters
                            .Select(x =>
                                x.parameterType == UdonNodeParameter.ParameterType.IN     ? TeuchiUdonMethodParamInOut.In    :
                                x.parameterType == UdonNodeParameter.ParameterType.IN_OUT ? TeuchiUdonMethodParamInOut.InOut : TeuchiUdonMethodParamInOut.Out);
                        var allParamUdonNames = def.parameters.Select(x => x.name);
                        var method = new TeuchiUdonMethod(instanceType, methodName, allParamTypes, inTypes, outTypes, allParamInOuts, def.fullName, allParamUdonNames);

                        if (method.UdonName.StartsWith("Const_"   )) continue;
                        if (method.UdonName.StartsWith("Variable_")) continue;
                        if (method.UdonName.StartsWith("Event_"))
                        {
                            if (!Events.ContainsKey(methodName))
                            {
                                Events.Add(methodName, method);
                            }
                            continue;
                        }
                        if (instanceType.LogicalTypeEquals(TeuchiUdonType.Type.ApplyArgAsType(new TeuchiUdonType("SystemVoid")))) continue;

                        if (!Methods.ContainsKey(method))
                        {
                            Methods.Add(method, method);
                        }

                        if (!TypeToMethods.ContainsKey(instanceType))
                        {
                            TypeToMethods.Add(instanceType, new Dictionary<string, Dictionary<int, List<TeuchiUdonMethod>>>());
                        }
                        var nameToMethods = TypeToMethods[instanceType];

                        if (!nameToMethods.ContainsKey(methodName))
                        {
                            nameToMethods.Add(methodName, new Dictionary<int, List<TeuchiUdonMethod>>());
                        }
                        var argsToMethods = nameToMethods[methodName];

                        if (!argsToMethods.ContainsKey(inTypes.Length))
                        {
                            argsToMethods.Add(inTypes.Length, new List<TeuchiUdonMethod>());
                        }
                        argsToMethods[inTypes.Length].Add(method);
                    }
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
            if (!TypeToMethods.ContainsKey(query.Type)) return new TeuchiUdonMethod[0];
            var methodToMethods = TypeToMethods[query.Type];

            if (!methodToMethods.ContainsKey(query.Name)) return new TeuchiUdonMethod[0];
            var argsToMethods = methodToMethods[query.Name];

            if (!argsToMethods.ContainsKey(inTypeCount)) return new TeuchiUdonMethod[0];
            var methods = argsToMethods[inTypeCount];

            if (methods.Count == 0) return new TeuchiUdonMethod[0];
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

            return new TeuchiUdonMethod[0];
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
                PublicVars             .Select(x => (x.Key.GetFullLabel(), x.Value.Value  , x.Key.Type         .RealType))
                .Concat(Literals.Values.Select(x => (x    .GetFullLabel(), x.Value        , x.Type             .RealType)))
                .Concat(Indirects      .Select(x => (x.Key.GetFullLabel(), (object)x.Value, TeuchiUdonType.UInt.RealType)));
        }
    }
}
