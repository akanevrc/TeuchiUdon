using System;
using System.Runtime.InteropServices;
using System.Text;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public static class TeuchiUdonUnityCompiler
    {
        [DllImport("Assets/akanevrc/TeuchiUdon/Editor/Compiler/External/teuchiudon_bin.dll")]
        public static extern IntPtr compile([MarshalAs(UnmanagedType.LPUTF8Str)] string input, [MarshalAs(UnmanagedType.LPUTF8Str)] string json);

        [DllImport("Assets/akanevrc/TeuchiUdon/Editor/Compiler/External/teuchiudon_bin.dll")]
        public static extern void free_str(IntPtr ptr);

        public static string PtrToStringUTF8(IntPtr ptr)
        {
            var len = 0;
            while (Marshal.ReadByte(ptr, len) != 0) ++len;
            var buffer = new byte[len];
            Marshal.Copy(ptr, buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }
    }
}
