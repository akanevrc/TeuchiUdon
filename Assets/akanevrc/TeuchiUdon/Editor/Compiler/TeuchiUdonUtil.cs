
namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public static class TeuchiUdonUtil
    {
        public static T Bind<T>(this T t, out T bind)
        {
            bind = t;
            return t;
        }
    }
}
