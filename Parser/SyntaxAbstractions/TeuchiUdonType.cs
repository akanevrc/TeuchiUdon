using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon
{
    public interface ITeuchiUdonTypeArg
    {
    }

    public class TeuchiUdonType : ITeuchiUdonTypeArg, IEquatable<TeuchiUdonType>
    {
        public TeuchiUdonQualifier Qualifier { get; }
        public string Name { get; }
        public ITeuchiUdonTypeArg[] Args { get; }
        public string LogicalName { get; }
        public string RealName { get; }
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

        public TeuchiUdonType
        (
            TeuchiUdonQualifier qualifier,
            string name,
            string logicalName,
            string realName,
            Type realType
        ) : this(qualifier, name, Enumerable.Empty<TeuchiUdonType>(), logicalName, realName, realType)
        {
        }

        public TeuchiUdonType
        (
            TeuchiUdonQualifier qualifier,
            string name,
            IEnumerable<ITeuchiUdonTypeArg> args,
            string logicalName,
            string realName,
            Type realType
        )
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

        public bool LogicalTypeNameEquals(TeuchiUdonType obj)
        {
            return obj != null && LogicalName == obj.LogicalName;
        }

        public bool LogicalTypeEquals(TeuchiUdonType obj)
        {
            return obj != null && LogicalName == obj.LogicalName && Args.SequenceEqual(obj.Args, ITeuchiUdonTypeArgLogicalEqualityComparer.Instance);
        }

        public TeuchiUdonType ApplyArgs(IEnumerable<ITeuchiUdonTypeArg> args)
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

        public TeuchiUdonType ApplyArgAsList(TeuchiUdonType type, TeuchiUdonType arrayType)
        {
            return ApplyArgs(new ITeuchiUdonTypeArg[] { type, arrayType });
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
    }

    public class TeuchiUdonTypeLogicalEqualityComparer : IEqualityComparer<TeuchiUdonType>
    {
        public static TeuchiUdonTypeLogicalEqualityComparer Instance { get; } = new TeuchiUdonTypeLogicalEqualityComparer();

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
