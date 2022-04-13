
namespace akanevrc.TeuchiUdon
{
    public static class TeuchiUdonUtil
    {
        public static T Store<T>(this T t, out T v)
        {
            v = t;
            return t;
        }
    }
}
