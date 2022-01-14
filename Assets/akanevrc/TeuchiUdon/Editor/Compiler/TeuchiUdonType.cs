using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonType : IEquatable<TeuchiUdonType>
    {
        public static TeuchiUdonType Bottom { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "bottom", null, "_bottom");
        public static TeuchiUdonType Qual { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "qual", null, "_qual");
        public static TeuchiUdonType Type { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "type", null, "_type");
        public static TeuchiUdonType Void { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "void", null, "SystemVoid");
        public static TeuchiUdonType Unit { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "unit", null, "_unit");
        public static TeuchiUdonType Func { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "func", null, "_func");
        public static TeuchiUdonType Object { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "object", typeof(object), "SystemObject");
        public static TeuchiUdonType Int { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "int", typeof(int), "SystemInt32");
        public static TeuchiUdonType UInt { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "uint", typeof(uint), "SystemUInt32");
        public static TeuchiUdonType Long { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "long", typeof(long), "SystemInt64");
        public static TeuchiUdonType ULong { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "ulong", typeof(ulong), "SystemUInt64");
        public static TeuchiUdonType String { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "string", typeof(string), "SystemString");
        public static TeuchiUdonType UObject { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "uobject", typeof(UnityEngine.Object), "UnityEngineObject");
        public static TeuchiUdonType List { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "list", null, "_list");
        public static TeuchiUdonType T { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "generic", null, "_T");
        public static TeuchiUdonType TArray { get; } = new TeuchiUdonType(TeuchiUdonQualifier.Top, "genericarray", null, "_TArray");

        public TeuchiUdonQualifier Qualifier { get; }
        public string Name { get; }
        public object[] Args { get; }
        public Type RealType { get; }
        public string UdonName { get; }

        public TeuchiUdonType(TeuchiUdonQualifier qualifier, string name)
        {
            Qualifier = qualifier;
            Name      = name;
            Args      = new TeuchiUdonType[0];
            RealType  = null;
            UdonName  = null;
        }

        public TeuchiUdonType(TeuchiUdonQualifier qualifier, string name, IEnumerable<object> args)
        {
            Qualifier = qualifier;
            Name      = name;
            Args      = args.ToArray();
            RealType  = null;
            UdonName  = null;
        }

        public TeuchiUdonType(TeuchiUdonQualifier qualifier, string name, Type realType, string udonName)
        {
            Qualifier = qualifier;
            Name      = name;
            Args      = new TeuchiUdonType[0];
            RealType  = realType;
            UdonName  = udonName;
        }

        public TeuchiUdonType(TeuchiUdonQualifier qualifier, string name, IEnumerable<object> args, Type realType, string udonName)
        {
            Qualifier = qualifier;
            Name      = name;
            Args      = args.ToArray();
            RealType  = realType;
            UdonName  = udonName;
        }

        public bool Equals(TeuchiUdonType obj)
        {
            return Qualifier == obj.Qualifier && Name == obj.Name && Args.SequenceEqual(obj.Args);
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
            return obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonType obj1, TeuchiUdonType obj2)
        {
            return !obj1.Equals(obj2);
        }

        public override string ToString()
        {
            return $"{Qualifier}.{Name}{(Args.Length == 0 ? "" : $"<{string.Join(", ", Args.Select(x => x.ToString()))}>")}";
        }

        public bool TypeNameEquals(TeuchiUdonType obj)
        {
            return Qualifier == obj.Qualifier && Name == obj.Name;
        }

        public TeuchiUdonType Apply(IEnumerable<object> args)
        {
            return new TeuchiUdonType(Qualifier, Name, args, RealType, UdonName);
        }

        public bool IsAssignableFrom(TeuchiUdonType obj)
        {
            return RealType != null && obj.RealType != null && RealType.IsAssignableFrom(obj.RealType);
        }
    }
}
