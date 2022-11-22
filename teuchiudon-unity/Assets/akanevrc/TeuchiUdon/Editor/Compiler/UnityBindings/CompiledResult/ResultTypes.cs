using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    [Serializable]
    public class Compiled
    {
        public string output;
        public string[] errors;
        public DefaultValue[] default_values;

        public Compiled
        (
            string output,
            string[] errors,
            DefaultValue[] default_values
        )
        {
            this.output = output;
            this.errors = errors;
            this.default_values = default_values;
        }
    }

    [Serializable]
    public class DefaultValue
    {
        public string name;
        public string ty;
        public string value;

        public DefaultValue
        (
            string name,
            string ty,
            string value
        )
        {
            this.name = name;
            this.ty = ty;
            this.value = value;
        }
    }
}
