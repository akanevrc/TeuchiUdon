using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public static class TypeConverter
    {
        public static (Type type, object value) Convert(string typeName, string literal)
        {
            switch (typeName)
            {
                case "SystemString":
                    return (typeof(string), literal);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
