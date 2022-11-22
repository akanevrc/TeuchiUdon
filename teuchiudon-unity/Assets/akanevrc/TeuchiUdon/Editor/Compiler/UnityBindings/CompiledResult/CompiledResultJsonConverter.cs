using UnityEngine;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    internal static class CompiledResultJsonConverter
    {
        public static Compiled FromJson(string json)
        {
            return JsonUtility.FromJson<Compiled>(json);
        }
    }
}
