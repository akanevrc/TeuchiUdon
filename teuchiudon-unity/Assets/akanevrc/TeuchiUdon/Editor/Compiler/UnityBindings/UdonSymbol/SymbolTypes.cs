using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    [Serializable]
    public class UdonSymbols
    {
        public BaseTySymbol[] base_tys;
        public TySymbol[] tys;
        public MethodSymbol[] methods;
        public EvSymbol[] evs;

        public UdonSymbols
        (
            BaseTySymbol[] base_tys,
            TySymbol[] tys,
            MethodSymbol[] methods,
            EvSymbol[] evs
        )
        {
            this.base_tys = base_tys;
            this.tys = tys;
            this.methods = methods;
            this.evs = evs;
        }
    }

    [Serializable]
    public class BaseTySymbol
    {
        public string[] scopes;
        public string name;
        public string logical_name;

        public BaseTySymbol(string[] scopes, string name, string logical_name)
        {
            this.scopes = scopes;
            this.name = name;
            this.logical_name = logical_name;
        }
    }

    [Serializable]
    public class TySymbol
    {
        public string[] scopes;
        public string name;
        public string real_name;
        public string[] args;

        public TySymbol
        (
            string[] scopes,
            string name,
            string real_name,
            string[] args
        )
        {
            this.scopes = scopes;
            this.name = name;
            this.real_name = real_name;
            this.args = args;
        }
    }

    [Serializable]
    public class MethodSymbol
    {
        public bool is_static;
        public string ty;
        public string name;
        public string[] param_tys;
        public string[] param_in_outs;
        public string real_name;
        public string[] param_real_names;

        public MethodSymbol
        (
            bool is_static,
            string ty,
            string name,
            string[] param_tys,
            string[] param_in_outs,
            string real_name,
            string[] param_real_names
        )
        {
            this.is_static = is_static;
            this.ty = ty;
            this.name = name;
            this.param_tys = param_tys;
            this.param_in_outs = param_in_outs;
            this.real_name = real_name;
            this.param_real_names = param_real_names;
        }
    }

    [Serializable]
    public class EvSymbol
    {
        public string name;
        public string[] param_tys;
        public string[] param_in_outs;
        public string real_name;
        public string[] param_real_names;

        public EvSymbol
        (
            string name,
            string[] param_tys,
            string[] param_in_outs,
            string real_name,
            string[] param_real_names
        )
        {
            this.name = name;
            this.param_tys = param_tys;
            this.param_in_outs = param_in_outs;
            this.real_name = real_name;
            this.param_real_names = param_real_names;
        }
    }
}
