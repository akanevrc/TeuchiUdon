
namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    internal class UdonTypeSymbol
    {
        public string[] Scopes { get; }
        public string Name { get; }
        public string LogicalName { get; }
        public string RealName { get; }
        public string[] Args { get; }

        public UdonTypeSymbol
        (
            string[] scopes,
            string name,
            string logicalName,
            string realName,
            string[] args
        )
        {
            Scopes = scopes;
            Name = name;
            LogicalName = logicalName;
            RealName = realName;
            Args = args;
        }
    }

    internal class UdonArrayTypeSymbol
    {
        public string RealName { get; }
        public string ElementType { get; }

        public UdonArrayTypeSymbol(string realName, string elementType)
        {
            RealName = realName;
            ElementType = elementType;
        }
    }

    internal class UdonGenericBaseTypeSymbol
    {
        public string[] Scopes { get; }
        public string Name { get; }
        public string LogicalName { get; }

        public UdonGenericBaseTypeSymbol(string[] scopes, string name, string logicalName)
        {
            Scopes = scopes;
            Name = name;
            LogicalName = logicalName;
        }
    }

    internal class UdonMethodSymbol
    {
        public bool IsStatic { get; }
        public string Type { get; }
        public string Name { get; }
        public string[] ParamTypes { get; }
        public string[] ParamInOuts { get; }
        public string RealName { get; }
        public string[] ParamRealNames { get; }

        public UdonMethodSymbol
        (
            bool isStatic,
            string type,
            string name,
            string[] paramTypes,
            string[] paramInOuts,
            string realName,
            string[] paramRealNames
        )
        {
            IsStatic = isStatic;
            Type = type;
            Name = name;
            ParamTypes = paramTypes;
            ParamInOuts = paramInOuts;
            RealName = realName;
            ParamRealNames = paramRealNames;
        }
    }

    internal class UdonEventSymbol
    {
        public string Name { get; }
        public string[] ParamTypes { get; }
        public string[] ParamInOuts { get; }
        public string RealName { get; }
        public string[] ParamRealNames { get; }

        public UdonEventSymbol
        (
            string name,
            string[] paramTypes,
            string[] paramInOuts,
            string realName,
            string[] paramRealNames
        )
        {
            Name = name;
            ParamTypes = paramTypes;
            ParamInOuts = paramInOuts;
            RealName = realName;
            ParamRealNames = paramRealNames;
        }
    }
}
