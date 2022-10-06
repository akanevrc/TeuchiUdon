using System;
using System.Runtime.InteropServices;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public static class TeuchiUdonUnityCompiler
    {
        [DllImport("Assets/akanevrc/TeuchiUdon/Editor/Compiler/External/teuchiudon_bin.dll")]
        [return: MarshalAs(UnmanagedType.LPUTF8Str)]
        public static extern string compile([MarshalAs(UnmanagedType.LPUTF8Str)] string input);

        [DllImport("Assets/akanevrc/TeuchiUdon/Editor/Compiler/External/teuchiudon_bin.dll")]
        public static extern void free_str([MarshalAs(UnmanagedType.LPUTF8Str)] string input);
    }
}
