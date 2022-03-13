using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public interface ITeuchiUdonTypeArg
    {
        string GetLogicalName();
    }

    public class TeuchiUdonType : ITeuchiUdonTypeArg, IEquatable<TeuchiUdonType>
    {
        public static TeuchiUdonType Unknown { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "unknown", "unknown", null, null);
        public static TeuchiUdonType Any { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "any", "any", "SystemObject", typeof(object));
        public static TeuchiUdonType Bottom { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "bottom", "bottom", "SystemObject", typeof(object));
        public static TeuchiUdonType Unit { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "unit", "unit", null, null);
        public static TeuchiUdonType Qual { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "qual", "qual", null, null);
        public static TeuchiUdonType Type { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "type", "type", null, null);
        public static TeuchiUdonType Tuple { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "tuple", "tuple", null, null);
        public static TeuchiUdonType Array { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "array", "array", null, null);
        public static TeuchiUdonType List { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "list", "list", null, null);
        public static TeuchiUdonType Buffer { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "buffer", "buffer", "SystemObjectArray", typeof(object[]));
        public static TeuchiUdonType Func { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "func", "func", "SystemUInt32", typeof(uint));
        public static TeuchiUdonType Method { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "method", "method", null, null);
        public static TeuchiUdonType Object { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "object", "SystemObject", "SystemObject", typeof(object));
        public static TeuchiUdonType DotNetType { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "dotnettype", "SystemType", "SystemType", typeof(Type));
        public static TeuchiUdonType Bool { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "bool", "SystemBoolean", "SystemBoolean", typeof(bool));
        public static TeuchiUdonType Byte { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "byte", "SystemByte", "SystemByte", typeof(byte));
        public static TeuchiUdonType SByte { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "sbyte", "SystemSByte", "SystemSByte", typeof(sbyte));
        public static TeuchiUdonType Short { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "short", "SystemInt16", "SystemInt16", typeof(short));
        public static TeuchiUdonType UShort { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "ushort", "SystemUInt16", "SystemUInt16", typeof(ushort));
        public static TeuchiUdonType Int { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "int", "SystemInt32", "SystemInt32", typeof(int));
        public static TeuchiUdonType UInt { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "uint", "SystemUInt32", "SystemUInt32", typeof(uint));
        public static TeuchiUdonType Long { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "long", "SystemInt64", "SystemInt64", typeof(long));
        public static TeuchiUdonType ULong { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "ulong", "SystemUInt64", "SystemUInt64", typeof(ulong));
        public static TeuchiUdonType Float { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "float", "SystemSingle", "SystemSingle", typeof(float));
        public static TeuchiUdonType Double { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "double", "SystemDouble", "SystemDouble", typeof(double));
        public static TeuchiUdonType Decimal { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "decimal", "SystemDecimal", "SystemDecimal", typeof(decimal));
        public static TeuchiUdonType DateTime { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "datetime", "SystemDateTime", "SystemDateTime", typeof(DateTime));
        public static TeuchiUdonType Char { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "char", "SystemChar", "SystemChar", typeof(char));
        public static TeuchiUdonType String { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "string", "SystemString", "SystemString", typeof(string));
        public static TeuchiUdonType UnityObject { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "unityobject", "UnityEngineObject", "UnityEngineObject", typeof(UnityEngine.Object));
        public static TeuchiUdonType GameObject { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "gameobject", "UnityEngineGameObject", "UnityEngineGameObject", typeof(UnityEngine.GameObject));

        public TeuchiUdonQualifier Qualifier { get; }
        public string Name { get; }
        public ITeuchiUdonTypeArg[] Args { get; }
        public string LogicalName { get; }
        private string RealName { get; }
        public Type RealType { get; }

        public TeuchiUdonType(TeuchiUdonQualifier qualifier, string name)
            : this(qualifier, name, new TeuchiUdonType[0], null, null, null)
        {
        }

        public TeuchiUdonType(TeuchiUdonQualifier qualifier, string name, IEnumerable<ITeuchiUdonTypeArg> args)
            : this(qualifier, name, args, null, null, null)
        {
        }

        public TeuchiUdonType(string logicalName)
            : this(null, null, new TeuchiUdonType[0], logicalName, null, null)
        {
        }

        public TeuchiUdonType(string logicalName, IEnumerable<ITeuchiUdonTypeArg> args)
            : this(null, null, args, logicalName, null, null)
        {
        }

        public TeuchiUdonType(TeuchiUdonQualifier qualifier, string name, string logicalName, string realName, Type realType)
            : this(qualifier, name, new TeuchiUdonType[0], logicalName, realName, realType)
        {
        }

        public TeuchiUdonType(TeuchiUdonQualifier qualifier, string name, IEnumerable<ITeuchiUdonTypeArg> args, string logicalName, string realName, Type realType)
        {
            Qualifier   = qualifier;
            Name        = name;
            Args        = args?.ToArray() ?? new TeuchiUdonType[0];
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
            return LogicalName;
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

        public bool IsAssignableFrom(TeuchiUdonType obj)
        {
            return
                obj != null &&
                (
                    IsAssignableFromUnknown(obj) ||
                    IsAssignableFromAny    (obj) ||
                    IsAssignableFromBottom (obj) ||
                    IsAssignableFromUnit   (obj) ||
                    IsAssignableFromQual   (obj) ||
                    IsAssignableFromType   (obj) ||
                    IsAssignableFromTuple  (obj) ||
                    IsAssignableFromArray  (obj) ||
                    IsAssignableFromList   (obj) ||
                    IsAssignableFromBuffer (obj) ||
                    IsAssignableFromFunc   (obj) ||
                    IsAssignableFromDotNet (obj)
                );
        }

        private bool IsAssignableFromUnknown(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeEquals(TeuchiUdonType.Unknown)) return false;

            return true;
        }

        private bool IsAssignableFromAny(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeEquals(TeuchiUdonType.Any)) return false;

            return true;
        }

        private bool IsAssignableFromBottom(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!obj.LogicalTypeEquals(TeuchiUdonType.Bottom)) return false;

            return
                !LogicalTypeEquals    (TeuchiUdonType.Unit  ) &&
                !LogicalTypeNameEquals(TeuchiUdonType.Qual  ) &&
                !LogicalTypeNameEquals(TeuchiUdonType.Type  ) &&
                !LogicalTypeNameEquals(TeuchiUdonType.Tuple ) &&
                !LogicalTypeNameEquals(TeuchiUdonType.Array ) &&
                !LogicalTypeNameEquals(TeuchiUdonType.List  ) &&
                !LogicalTypeEquals    (TeuchiUdonType.Buffer) &&
                !LogicalTypeNameEquals(TeuchiUdonType.Func  ) &&
                !LogicalTypeNameEquals(TeuchiUdonType.Method);
        }

        private bool IsAssignableFromUnit(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeEquals(TeuchiUdonType.Unit) || !obj.LogicalTypeEquals(TeuchiUdonType.Unit)) return false;

            return true;
        }

        private bool IsAssignableFromQual(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeEquals(TeuchiUdonType.Qual) || !obj.LogicalTypeNameEquals(TeuchiUdonType.Qual)) return false;

            return obj.Args.Length == 1;
        }

        private bool IsAssignableFromType(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeEquals(TeuchiUdonType.Type) || !obj.LogicalTypeNameEquals(TeuchiUdonType.Type)) return false;

            return obj.Args.Length == 1;
        }

        private bool IsAssignableFromTuple(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeNameEquals(TeuchiUdonType.Tuple) || !obj.LogicalTypeNameEquals(TeuchiUdonType.Tuple)) return false;

            return GetArgsAsTuple().Zip(obj.GetArgsAsTuple(), (t, o) => (t, o)).All(x => x.t.IsAssignableFrom(x.o));
        }

        private bool IsAssignableFromArray(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeNameEquals(TeuchiUdonType.Array) || !obj.LogicalTypeNameEquals(TeuchiUdonType.Array)) return false;

            return GetArgAsArray().IsAssignableFrom(obj.GetArgAsArray());
        }

        private bool IsAssignableFromList(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeNameEquals(TeuchiUdonType.List) || !obj.LogicalTypeNameEquals(TeuchiUdonType.List)) return false;

            return GetArgAsList().IsAssignableFrom(obj.GetArgAsList());
        }

        private bool IsAssignableFromBuffer(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeEquals(TeuchiUdonType.Buffer) || !obj.LogicalTypeEquals(TeuchiUdonType.Buffer)) return false;

            return true;
        }

        private bool IsAssignableFromFunc(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!LogicalTypeNameEquals(TeuchiUdonType.Func) || !obj.LogicalTypeNameEquals(TeuchiUdonType.Func)) return false;

            return
                    GetArgAsFuncInType ().IsAssignableFrom(obj.GetArgAsFuncInType ()) &&
                obj.GetArgAsFuncOutType().IsAssignableFrom(    GetArgAsFuncOutType());
        }

        private bool IsAssignableFromDotNet(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!IsDotNetType() || !obj.IsDotNetType()) return false;

            return RealType != null && obj.RealType != null && RealType.IsAssignableFrom(obj.RealType);
        }

        private bool IsDotNetType()
        {
            return new TeuchiUdonType[]
            {
                TeuchiUdonType.Unknown,
                TeuchiUdonType.Any,
                TeuchiUdonType.Bottom,
                TeuchiUdonType.Unit,
                TeuchiUdonType.Qual,
                TeuchiUdonType.Type,
                TeuchiUdonType.Tuple,
                TeuchiUdonType.Array,
                TeuchiUdonType.List,
                TeuchiUdonType.Buffer,
                TeuchiUdonType.Func,
                TeuchiUdonType.Method
            }
            .All(x => !LogicalTypeNameEquals(x));
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
            return ApplyArgs(new ITeuchiUdonTypeArg[] { type });
        }

        public TeuchiUdonType ApplyArgsAsFunc(TeuchiUdonType inType, TeuchiUdonType outType)
        {
            return ApplyArgs(new ITeuchiUdonTypeArg[] { inType, outType });
        }

        public TeuchiUdonType ApplyArgsAsMethod(IEnumerable<TeuchiUdonMethod> methods)
        {
            return ApplyArgs(methods);
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

        public TeuchiUdonType GetArgAsList()
        {
            return (TeuchiUdonType)Args[0];
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

        public static TeuchiUdonType ToOneType(IEnumerable<TeuchiUdonType> types)
        {
            var count = types.Count();
            return
                count == 0 ? TeuchiUdonType.Unit :
                count == 1 ? types.First() :
                TeuchiUdonType.Tuple.ApplyArgsAsTuple(types);
        }

        public IEnumerable<TeuchiUdonMethod> GetMostCompatibleMethods(IEnumerable<TeuchiUdonType> inTypes)
        {
            if (!LogicalTypeNameEquals(TeuchiUdonType.Method)) return new TeuchiUdonMethod[0];

            var methods = GetArgsAsMethod().ToArray();
            if (methods.Length == 0) return new TeuchiUdonMethod[0];

            var it = inTypes.ToArray();
            var justCountToMethods = new Dictionary<int, List<TeuchiUdonMethod>>();
            foreach (var method in methods)
            {
                if (method.InTypes.Length != it.Length) continue;

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

            return new TeuchiUdonMethod[0];
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
                obj1 is TeuchiUdonType      t1 && obj2 is TeuchiUdonType      t2 && t1.LogicalTypeEquals(t2);
        }

        public int GetHashCode(ITeuchiUdonTypeArg obj)
        {
            return obj is TeuchiUdonType t ? t.LogicalName.GetHashCode() : obj.GetHashCode();
        }
    }
}
