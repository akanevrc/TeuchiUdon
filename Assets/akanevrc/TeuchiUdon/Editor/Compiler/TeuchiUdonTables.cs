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
        public Dictionary<TeuchiUdonType, TeuchiUdonType> LogicalTypes { get; private set; }
        public Dictionary<TeuchiUdonMethod, TeuchiUdonMethod> Methods { get; private set; }
        public Dictionary<TeuchiUdonType, Dictionary<string, Dictionary<int, List<TeuchiUdonMethod>>>> TypeToMethods { get; private set; }
        public Dictionary<TeuchiUdonVar, TeuchiUdonVar> Vars { get; private set; }
        public Dictionary<TeuchiUdonVar, TeuchiUdonVar> UnbufferedVars { get; private set; }
        public Dictionary<TeuchiUdonVar, TeuchiUdonLiteral> ExportedVars { get; private set; }
        public Dictionary<TeuchiUdonVar, TeuchiUdonSyncMode> SyncedVars { get; private set; }
        public Dictionary<TeuchiUdonOutValue, TeuchiUdonOutValue> OutValues { get; private set; }
        public Dictionary<TeuchiUdonLiteral, TeuchiUdonLiteral> Literals { get; private set; }
        public Dictionary<TeuchiUdonThis, TeuchiUdonThis> This { get; private set; }
        public Dictionary<TeuchiUdonFunc, TeuchiUdonFunc> Funcs { get; private set; }
        public Dictionary<TeuchiUdonIndirect, uint> Indirects { get; private set; }

        public int ExpectCount { get; private set; }

        public Dictionary<string, string> UnaryOps { get; private set; }
        public Dictionary<string, string> BinaryOps { get; private set; }

        public int VarCounter { get; private set; }
        public int OutValueCounter { get; private set; }
        public int LiteralCounter { get; private set; }
        public int FuncCounter { get; private set; }
        public int VarBindCounter { get; private set; }
        public int BlockCounter { get; private set; }
        public int EvalFuncCounter { get; private set; }
        public int LetInCounter { get; private set; }
        public int IndirectCounter { get; private set; }

        private bool IsInitialized { get; set; } = false;

        protected TeuchiUdonTables()
        {
        }

        public void Init()
        {
            if (!IsInitialized)
            {
                Qualifiers    = new Dictionary<TeuchiUdonQualifier, TeuchiUdonQualifier>();
                Types         = new Dictionary<TeuchiUdonType     , TeuchiUdonType>();
                LogicalTypes  = new Dictionary<TeuchiUdonType     , TeuchiUdonType>(TeuchiUdonTypeLogicalEqualityComparer.Instance);
                Methods       = new Dictionary<TeuchiUdonMethod   , TeuchiUdonMethod>();
                TypeToMethods = new Dictionary<TeuchiUdonType     , Dictionary<string, Dictionary<int, List<TeuchiUdonMethod>>>>(TeuchiUdonTypeLogicalEqualityComparer.Instance);
                UnaryOps      = new Dictionary<string             , string>();
                BinaryOps     = new Dictionary<string             , string>();

                InitInternalTypes();
                InitExternalTypes();
                InitQualifiers();
                InitMethods();
                //InitOps();

                IsInitialized = true;
            }

            Vars           = new Dictionary<TeuchiUdonVar     , TeuchiUdonVar>();
            UnbufferedVars = new Dictionary<TeuchiUdonVar     , TeuchiUdonVar>();
            ExportedVars   = new Dictionary<TeuchiUdonVar     , TeuchiUdonLiteral>();
            SyncedVars     = new Dictionary<TeuchiUdonVar     , TeuchiUdonSyncMode>();
            OutValues      = new Dictionary<TeuchiUdonOutValue, TeuchiUdonOutValue>();
            Literals       = new Dictionary<TeuchiUdonLiteral , TeuchiUdonLiteral>();
            This           = new Dictionary<TeuchiUdonThis    , TeuchiUdonThis>();
            Funcs          = new Dictionary<TeuchiUdonFunc    , TeuchiUdonFunc>();
            Indirects      = new Dictionary<TeuchiUdonIndirect, uint>();

            ExpectCount = 0;

            VarCounter      = 0;
            OutValueCounter = 0;
            LiteralCounter  = 0;
            FuncCounter     = 0;
            VarBindCounter  = 0;
            BlockCounter    = 0;
            EvalFuncCounter = 0;
            LetInCounter    = 0;
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
                TeuchiUdonType.Qual,
                TeuchiUdonType.Type,
                TeuchiUdonType.Tuple,
                TeuchiUdonType.List,
                TeuchiUdonType.Func,
                TeuchiUdonType.Method,
                TeuchiUdonType.Object,
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
                TeuchiUdonType.Char,
                TeuchiUdonType.String,
                TeuchiUdonType.UnityObject,
                TeuchiUdonType.GameObject,
                TeuchiUdonType.Buffer
            };

            foreach (var t in types)
            {
                Types       .Add(t, t);
                LogicalTypes.Add(t, t);
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
                        var qualNames    = typeNames.Take(typeNames.Length - 1).Select(x => new TeuchiUdonScope(x, TeuchiUdonScopeMode.Type)).ToArray();
                        var typeName     = typeNames.Last().Replace("[]", "");
                        var realType     = def.type.IsArray ? def.type.GetElementType() : def.type;
                        var qual         = new TeuchiUdonQualifier(qualNames, qualNames);
                        var type         = new TeuchiUdonType(qual, typeName, udonTypeName, udonTypeName, realType);

                        if (!Types.ContainsKey(type))
                        {
                            Types.Add(type, type);
                        }

                        if (!LogicalTypes.ContainsKey(type))
                        {
                            LogicalTypes.Add(type, type);
                        }

                        var paramTypes         = def.parameters.Select(x => x.type).ToArray();
                        var paramUdonTypeNames = paramTypes.Select(x => GetUdonTypeName(x)).ToArray();
                        var paramTypeNamess    = paramTypes.Select(x => x.FullName.Split(new string[] { "." }, StringSplitOptions.None).ToArray());
                        var paramQualNamess    = paramTypeNamess.Select(x => x.Take(x.Length - 1).Select(y => new TeuchiUdonScope(y, TeuchiUdonScopeMode.Type)).ToArray());
                        var paramTypeNames     = paramTypeNamess.Select(x => x.Last()).ToArray();
                        var paramQuals         = paramQualNamess.Select(x => new TeuchiUdonQualifier(x, x)).ToArray();
                        
                        for (var i = 0; i < paramTypeNames.Length; i++)
                        {
                            var methodParamType = new TeuchiUdonType(paramQuals[i], paramTypeNames[i], paramUdonTypeNames[i], paramUdonTypeNames[i], paramTypes[i]);

                            if (!Types.ContainsKey(methodParamType))
                            {
                                Types.Add(methodParamType, methodParamType);
                            }

                            if (!LogicalTypes.ContainsKey(methodParamType))
                            {
                                LogicalTypes.Add(methodParamType, methodParamType);
                            }
                        }
                    }
                }
            }
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

                        var udonTypeName = GetUdonTypeName(def.type);
                        var typeNames    = def.type.FullName.Split(new string[] { "." }, StringSplitOptions.None);
                        var qualNames    = typeNames.Take(typeNames.Length - 1).Select(x => new TeuchiUdonScope(x, TeuchiUdonScopeMode.Type)).ToArray();
                        var typeName     = typeNames.Last();
                        var qual         = new TeuchiUdonQualifier(qualNames, qualNames);
                        var type         = new TeuchiUdonType(qual, typeName, udonTypeName, udonTypeName, def.type);
                        var methodNames  = def.name.Split(new string[] { " " }, StringSplitOptions.None);
                        var methodName   = methodNames[methodNames.Length - 1];
                        var instanceDef = def.parameters
                            .FirstOrDefault(x => x.parameterType == UdonNodeParameter.ParameterType.IN && x.name == "instance");
                        var instanceType = instanceDef == null ? TeuchiUdonType.Type.ApplyArgAsType(type) : GetTeuchiUdonType(instanceDef.type);
                        var inTypes = def.parameters
                            .Where(x => x.parameterType == UdonNodeParameter.ParameterType.IN || x.parameterType == UdonNodeParameter.ParameterType.IN_OUT)
                            .Select(x => GetTeuchiUdonType(x.type)).ToArray();
                        var outTypes = def.parameters
                            .Where(x => x.parameterType == UdonNodeParameter.ParameterType.OUT)
                            .Select(x => GetTeuchiUdonType(x.type));
                        var allParamTypes = def.parameters
                            .Select(x => GetTeuchiUdonType(x.type));
                        var allParamInOuts = def.parameters
                            .Select(x =>
                                x.parameterType == UdonNodeParameter.ParameterType.IN     ? TeuchiUdonMethodParamInOut.In    :
                                x.parameterType == UdonNodeParameter.ParameterType.IN_OUT ? TeuchiUdonMethodParamInOut.InOut : TeuchiUdonMethodParamInOut.Out);
                        var method = new TeuchiUdonMethod(instanceType, methodName, inTypes, outTypes, allParamTypes, allParamInOuts, def.fullName);

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
            var qual  = new TeuchiUdonQualifier(names.Take(names.Length - 1).Select(x => new TeuchiUdonScope(x, TeuchiUdonScopeMode.Type)));
            var t     = new TeuchiUdonType(qual, names.Last());
            return Types[t];
        }

        public void InitOps()
        {
            UnaryOps.Add("--", "op_Decrement");
            UnaryOps.Add("++", "op_Increment");
            UnaryOps.Add("-" , "op_UnaryMinus");
            UnaryOps.Add("!" , "op_UnaryNegation");
            UnaryOps.Add("+" , "op_UnaryPlus");

            BinaryOps.Add("&&", "op_ConditionalAnd");
            BinaryOps.Add("||", "op_ConditionalOr");
            BinaryOps.Add("^^", "op_ConditionalXor");
            BinaryOps.Add("+" , "op_Addition");
            BinaryOps.Add("/" , "op_Division");
            BinaryOps.Add("==", "op_Equality");
            BinaryOps.Add(">=", "op_GreaterThanOrEqual");
            BinaryOps.Add(">" , "op_GreaterThan");
            BinaryOps.Add("!=", "op_Inequality");
            BinaryOps.Add("<<", "op_LeftShift");
            BinaryOps.Add("<=", "op_LessThanOrEqual");
            BinaryOps.Add("<" , "op_LessThan");
            BinaryOps.Add("&" , "op_LogicalAnd");
            BinaryOps.Add("|" , "op_LogicalOr");
            BinaryOps.Add("^" , "op_LogicalXor");
            BinaryOps.Add("*" , "op_Multiplication");
            BinaryOps.Add("*" , "op_Multiply");
            BinaryOps.Add("%" , "op_Modulus");
            BinaryOps.Add("%" , "op_Remainder");
            BinaryOps.Add(">>", "op_RightShift");
            BinaryOps.Add("-" , "op_Subtraction");
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

        public void SetExpectCount(int count)
        {
            ExpectCount = ExpectCount >= count ? ExpectCount : count;
        }

        public int GetVarIndex()
        {
            return VarCounter++;
        }

        public IEnumerable<TeuchiUdonOutValue> GetOutValues(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var o = new TeuchiUdonOutValue(OutValueCounter++);
                if (!OutValues.ContainsKey(o))
                {
                    OutValues.Add(o, o);
                }
                yield return o;
            }
        }

        public void ResetOutValueIndex()
        {
            OutValueCounter = 0;
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

        public int GetIndirectIndex()
        {
            return IndirectCounter++;
        }

        public static string GetUdonTypeName(Type type)
        {
            return type.FullName.Replace(".", "").Replace("[]", "Array");
        }

        public static string GetGetterName(string name)
        {
            return $"get_{name}";
        }

        public static string GetSetterName(string name)
        {
            return $"set_{name}";
        }

        public IEnumerable<(string name, object value, Type type)> GetDefaultValues()
        {
            return
                ExportedVars           .Select(x => (x.Key.GetFullLabel(), x.Value.Value  , x.Value.Type       .RealType))
                .Concat(Literals.Values.Select(x => (x    .GetFullLabel(), x.Value        , x.Type             .RealType)))
                .Concat(Indirects      .Select(x => (x.Key.GetFullLabel(), (object)x.Value, TeuchiUdonType.UInt.RealType)));
        }
    }
}
