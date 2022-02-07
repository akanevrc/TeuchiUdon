using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public interface ITeuchiUdonTypeArg
    {
    }

    public class TeuchiUdonType : ITeuchiUdonTypeArg, IEquatable<TeuchiUdonType>
    {
        public static TeuchiUdonType Unknown { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "unknown", "_unknown", null, null);
        public static TeuchiUdonType Any { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "any", "_any", null, null);
        public static TeuchiUdonType Bottom { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "bottom", "_bottom", null, null);
        public static TeuchiUdonType Qual { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "qual", "_qual", null, null);
        public static TeuchiUdonType Type { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "type", "_type", null, null);
        public static TeuchiUdonType Unit { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "unit", "_unit", null, null);
        public static TeuchiUdonType Tuple { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "tuple", "_tuple", null, null);
        public static TeuchiUdonType List { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "list", "_list", null, null);
        public static TeuchiUdonType Func { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "func", "_func", "SystemUInt32", typeof(uint));
        public static TeuchiUdonType Object { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "object", "SystemObject", "SystemObject", typeof(object));
        public static TeuchiUdonType Int { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "int", "SystemInt32", "SystemInt32", typeof(int));
        public static TeuchiUdonType UInt { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "uint", "SystemUInt32", "SystemUInt32", typeof(uint));
        public static TeuchiUdonType Long { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "long", "SystemInt64", "SystemInt64", typeof(long));
        public static TeuchiUdonType ULong { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "ulong", "SystemUInt64", "SystemUInt64", typeof(ulong));
        public static TeuchiUdonType Float { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "float", "SystemSingle", "SystemSingle", typeof(float));
        public static TeuchiUdonType Double { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "double", "SystemDouble", "SystemDouble", typeof(double));
        public static TeuchiUdonType Decimal { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "decimal", "SystemDecimal", "SystemDecimal", typeof(decimal));
        public static TeuchiUdonType Char { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "char", "SystemChar", "SystemChar", typeof(char));
        public static TeuchiUdonType String { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "string", "SystemString", "SystemString", typeof(string));
        public static TeuchiUdonType UObject { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "uobject", "UnityEngineObject", "UnityEngineObject", typeof(UnityEngine.Object));

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

        public TeuchiUdonType(TeuchiUdonQualifier qualifier, string name, string logicalName, string realName, Type realType)
            : this(qualifier, name, new TeuchiUdonType[0], logicalName, realName, realType)
        {
        }

        public TeuchiUdonType(TeuchiUdonQualifier qualifier, string name, IEnumerable<ITeuchiUdonTypeArg> args, string logicalName, string realName, Type realType)
        {
            Qualifier   = qualifier;
            Name        = name;
            Args        = args?.ToArray();
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
            return $"{Qualifier.Qualify(".", Name)}{(Args.Length == 0 ? "" : $"({string.Join(", ", Args.Select(x => x.ToString()))})")}";
        }

        public string GetRealName()
        {
            if (RealName == null)
            {
                throw new InvalidOperationException("No real name");
            }
            return RealName;
        }

        public bool TypeNameEquals(TeuchiUdonType obj)
        {
            return obj != null && Qualifier == obj.Qualifier && Name == obj.Name;
        }

        public bool IsAssignableFrom(TeuchiUdonType obj)
        {
            return
                obj != null &&
                (
                        TypeNameEquals(TeuchiUdonType.Unknown) ||
                        TypeNameEquals(TeuchiUdonType.Any    ) ||
                    obj.TypeNameEquals(TeuchiUdonType.Bottom ) ||
                    this == obj ||
                    RealType != null && obj.RealType != null && RealType.IsAssignableFrom(obj.RealType)
                );
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

        public TeuchiUdonType ApplyArgAsList(TeuchiUdonType type)
        {
            return ApplyArgs(new ITeuchiUdonTypeArg[] { type });
        }

        public TeuchiUdonType ApplyArgsAsFunc(TeuchiUdonType inType, TeuchiUdonType outType)
        {
            return ApplyArgs(new ITeuchiUdonTypeArg[] { inType, outType });
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

        public bool IsAssignableFromFunc(TeuchiUdonType obj)
        {
            if (obj == null) return false;
            if (!TypeNameEquals(TeuchiUdonType.Func) || !obj.TypeNameEquals(TeuchiUdonType.Func)) return false;
            if (Args.Length != 2 || obj.Args.Length != 2) return false;

            return
                    GetArgAsFuncInType ().IsAssignableFrom(obj.GetArgAsFuncInType ()) &&
                obj.GetArgAsFuncOutType().IsAssignableFrom(    GetArgAsFuncOutType());
        }
    }
}