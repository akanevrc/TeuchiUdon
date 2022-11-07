using UnityEngine;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    internal static class UdonSymbolJsonConverter
    {
        public static string ToJson(UdonSymbols symbols)
        {
            return JsonUtility.ToJson(symbols);
        }
    }
}
