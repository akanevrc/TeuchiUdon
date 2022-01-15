using System;
using System.Collections.Generic;
using System.Linq;
using VRC.Udon.Editor;
using VRC.Udon.Graph;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonTables
    {
        public static TeuchiUdonTables Instance { get; } = new TeuchiUdonTables();

        public Dictionary<TeuchiUdonQualifier, TeuchiUdonQualifier> Qualifiers { get; private set; }
        public Dictionary<TeuchiUdonType, TeuchiUdonType> Types { get; private set; }
        public Dictionary<string, TeuchiUdonType> UdonTypes { get; private set; }
        public Dictionary<TeuchiUdonMethod, TeuchiUdonMethod> Methods { get; private set; }
        public Dictionary<TeuchiUdonType, Dictionary<string, Dictionary<int, List<TeuchiUdonMethod>>>> TypeToMethods { get; private set; }
        public Dictionary<TeuchiUdonVar, TeuchiUdonVar> Vars { get; private set; }
        public Dictionary<int, LiteralResult> Literals { get; private set; }
        public Dictionary<int, FuncResult> Funcs { get; private set; }

        private int LiteralCounter { get; set; }
        private int FuncCounter { get; set; }

        private bool IsInitialized { get; set; } = false;

        protected TeuchiUdonTables()
        {
        }

        public void Init()
        {
            if (!IsInitialized)
            {
                Qualifiers    = new Dictionary<TeuchiUdonQualifier, TeuchiUdonQualifier>();
                Types         = new Dictionary<TeuchiUdonType, TeuchiUdonType>();
                UdonTypes     = new Dictionary<string, TeuchiUdonType>();
                Methods       = new Dictionary<TeuchiUdonMethod, TeuchiUdonMethod>();
                TypeToMethods = new Dictionary<TeuchiUdonType, Dictionary<string, Dictionary<int, List<TeuchiUdonMethod>>>>();

                InitInternalTypes();
                InitExternalTypes();
                InitQualifiers();
                InitMethods();

                IsInitialized = true;
            }

            Vars     = new Dictionary<TeuchiUdonVar, TeuchiUdonVar>();
            Literals = new Dictionary<int, LiteralResult>();
            Funcs    = new Dictionary<int, FuncResult>();

            LiteralCounter = 0;
            FuncCounter    = 0;
        }

        private void InitInternalTypes()
        {
            var types = new TeuchiUdonType[]
            {
                TeuchiUdonType.Bottom,
                TeuchiUdonType.Qual,
                TeuchiUdonType.Type,
                TeuchiUdonType.Void,
                TeuchiUdonType.Unit,
                TeuchiUdonType.Func,
                TeuchiUdonType.Object,
                TeuchiUdonType.Int,
                TeuchiUdonType.UInt,
                TeuchiUdonType.Long,
                TeuchiUdonType.ULong,
                TeuchiUdonType.String,
                TeuchiUdonType.UObject,
                TeuchiUdonType.List,
                TeuchiUdonType.T,
                TeuchiUdonType.TArray
            };

            foreach (var t in types)
            {
                Types.Add(t, t);
                if (!UdonTypes.ContainsKey(t.UdonName)) UdonTypes.Add(t.UdonName, t);
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

                        var udonTypeName = GetUdonTypeName(def.type);
                        var typeNames    = def.type.FullName.Split(new string[] { "." }, StringSplitOptions.None);
                        var qualNames    = typeNames.Take(typeNames.Length - 1);
                        var typeName     = typeNames.Last().Replace("[]", "");
                        var realType     = def.type.IsArray ? def.type.GetElementType() : def.type;
                        var qual         = new TeuchiUdonQualifier(qualNames, qualNames);
                        var type         = new TeuchiUdonType(qual, typeName, realType, udonTypeName);

                        if (def.type.IsArray)
                        {
                            type = TeuchiUdonType.List.Apply(new object[] { type });
                        }

                        if (!Types.ContainsKey(type))
                        {
                            Types.Add(type, type);
                        }

                        if (!UdonTypes.ContainsKey(udonTypeName))
                        {
                            UdonTypes.Add(udonTypeName, type);
                        }

                        var paramTypes         = def.parameters.Select(x => x.type).ToArray();
                        var paramUdonTypeNames = paramTypes.Select(x => GetUdonTypeName(x)).ToArray();
                        var paramTypeNamess    = paramTypes.Select(x => x.FullName.Split(new string[] { "." }, StringSplitOptions.None).ToArray());
                        var paramQualNamess    = paramTypeNamess.Select(x => x.Take(x.Length - 1));
                        var paramTypeNames     = paramTypeNamess.Select(x => x.Last()).ToArray();
                        var paramQuals         = paramQualNamess.Select(x => new TeuchiUdonQualifier(x, x)).ToArray();
                        
                        for (var i = 0; i < paramTypeNames.Length; i++)
                        {
                            var methodParamType = new TeuchiUdonType(paramQuals[i], paramTypeNames[i], paramTypes[i], paramUdonTypeNames[i]);

                            if (!Types.ContainsKey(methodParamType))
                            {
                                Types.Add(methodParamType, methodParamType);
                            }

                            if (!UdonTypes.ContainsKey(paramUdonTypeNames[i]))
                            {
                                UdonTypes.Add(paramUdonTypeNames[i], methodParamType);
                            }
                        }
                    }
                }
            }
        }

        private void InitQualifiers()
        {
            foreach (var t in Types.Keys)
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

                        var udonTypeName = GetUdonTypeName(def.type);
                        var typeNames    = def.type.FullName.Split(new string[] { "." }, StringSplitOptions.None);
                        var qualNames    = typeNames.Take(typeNames.Length - 1);
                        var typeName     = typeNames.Last();
                        var qual         = new TeuchiUdonQualifier(qualNames, qualNames);
                        var type         = new TeuchiUdonType(qual, typeName, def.type, udonTypeName);
                        var methodNames  = def.name.Split(new string[] { " " }, StringSplitOptions.None);
                        var methodName   = methodNames[methodNames.Length - 1];
                        var instanceDef = def.parameters
                            .FirstOrDefault(x => x.parameterType == UdonNodeParameter.ParameterType.IN && x.name == "instance");
                        var instanceType = instanceDef == null ? TeuchiUdonType.Type.Apply(new TeuchiUdonType[] { type }) : GetTeuchiUdonType(instanceDef.type);
                        var inTypes = def.parameters
                            .Where(x => x.parameterType == UdonNodeParameter.ParameterType.IN || x.parameterType == UdonNodeParameter.ParameterType.IN_OUT)
                            .Select(x => GetTeuchiUdonType(x.type)).ToArray();
                        var outTypes = def.parameters
                            .Where(x => x.parameterType == UdonNodeParameter.ParameterType.OUT)
                            .Select(x => GetTeuchiUdonType(x.type));
                        var allArgTypes = def.parameters
                            .Select(x => GetTeuchiUdonType(x.type));
                        var method = new TeuchiUdonMethod(instanceType, methodName, inTypes, outTypes, allArgTypes, def.fullName);

                        if (!Methods.ContainsKey(method))
                        {
                            Methods.Add(method, method);
                        }

                        if (!TypeToMethods.ContainsKey(instanceType))
                        {
                            TypeToMethods.Add(instanceType, new Dictionary<string, Dictionary<int, List<TeuchiUdonMethod>>>());
                        }
                        var methodToMethods = TypeToMethods[instanceType];

                        if (!methodToMethods.ContainsKey(methodName))
                        {
                            methodToMethods.Add(methodName, new Dictionary<int, List<TeuchiUdonMethod>>());
                        }
                        var argsToMethods = methodToMethods[methodName];

                        if (!argsToMethods.ContainsKey(inTypes.Length))
                        {
                            argsToMethods.Add(inTypes.Length, new List<TeuchiUdonMethod>());
                        }
                        argsToMethods[inTypes.Length].Add(method);
                    }
                }
            }
        }

        private TeuchiUdonType GetTeuchiUdonType(Type type)
        {
            var names = type.FullName.Split(new string[] { "." }, StringSplitOptions.None);
            var qual  = new TeuchiUdonQualifier(names.Take(names.Length - 1));
            var t     = new TeuchiUdonType(qual, names.Last());
            return Types[t];
        }

        public IEnumerable<TeuchiUdonMethod> GetMostCompatibleMethods(TeuchiUdonMethod query)
        {
            if (!TypeToMethods.ContainsKey(query.Type)) return new TeuchiUdonMethod[0];
            var methodToMethods = TypeToMethods[query.Type];

            if (!methodToMethods.ContainsKey(query.Name)) return new TeuchiUdonMethod[0];
            var argsToMethods = methodToMethods[query.Name];

            if (!argsToMethods.ContainsKey(query.InTypes.Length)) return new TeuchiUdonMethod[0];
            var methods = argsToMethods[query.InTypes.Length];

            if (methods.Count == 0) return new TeuchiUdonMethod[0];

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

                    if (m == q) justCount++;
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

        public int GetLiteralIndex()
        {
            return LiteralCounter++;
        }

        public int GetFuncIndex()
        {
            return FuncCounter++;
        }

        public static string GetUdonTypeName(Type type)
        {
            if (type.IsArray) return "_list";

            var name = type.FullName.Replace(".", "");
            return name;
        }

        public static string GetLiteralName(int index)
        {
            return $"literal[{index}]";
        }

        public static string GetFuncName(int index)
        {
            return $"func[{index}]";
        }

        public static string GetGetterName(string name)
        {
            return $"get_{name}";
        }

        public static string GetSetterName(string name)
        {
            return $"set_{name}";
        }
    }
}
