
namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    internal class TySymbol
    {
        public string[] scopes;
        public string name;
        public string logicalName;
        public string realName;
        public string[] args;

        public TySymbol
        (
            string[] scopes,
            string name,
            string logicalName,
            string realName,
            string[] args
        )
        {
            this.scopes = scopes;
            this.name = name;
            this.logicalName = logicalName;
            this.realName = realName;
            this.args = args;
        }
    }

    internal class ArrayTySymbol
    {
        public string realName;
        public string elementTy;

        public ArrayTySymbol(string realName, string elementTy)
        {
            this.realName = realName;
            this.elementTy = elementTy;
        }
    }

    internal class GenericBaseTySymbol
    {
        public string[] scopes;
        public string name;
        public string logicalName;

        public GenericBaseTySymbol(string[] scopes, string name, string logicalName)
        {
            this.scopes = scopes;
            this.name = name;
            this.logicalName = logicalName;
        }
    }

    internal class MethodSymbol
    {
        public bool isStatic;
        public string ty;
        public string name;
        public string[] paramTys;
        public string[] paramInOuts;
        public string realName;
        public string[] paramRealNames;

        public MethodSymbol
        (
            bool isStatic,
            string ty,
            string name,
            string[] paramTys,
            string[] paramInOuts,
            string realName,
            string[] paramRealNames
        )
        {
            this.isStatic = isStatic;
            this.ty = ty;
            this.name = name;
            this.paramTys = paramTys;
            this.paramInOuts = paramInOuts;
            this.realName = realName;
            this.paramRealNames = paramRealNames;
        }
    }

    internal class EvSymbol
    {
        public string name;
        public string[] paramTys;
        public string[] paramInOuts;
        public string realName;
        public string[] paramRealNames;

        public EvSymbol
        (
            string name,
            string[] paramTys,
            string[] paramInOuts,
            string realName,
            string[] paramRealNames
        )
        {
            this.name = name;
            this.paramTys = paramTys;
            this.paramInOuts = paramInOuts;
            this.realName = realName;
            this.paramRealNames = paramRealNames;
        }
    }
}
