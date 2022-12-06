using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonCompilerException : Exception
    {
        public TeuchiUdonCompilerException(string message)
            : base(message)
        {
        }
    }

    public static class TeuchiUdonUnityCompilerRunner
    {
        private static UdonSymbolExtractor UdonSymbolExtractor { get; } = new UdonSymbolExtractor();

        public static TeuchiUdonProgramAsset SaveTeuchiUdonAsset(string srcPath, string assetPath)
        {
            var (output, errors, defaultValues) = CompileFromPath(srcPath);
            if (errors.Length != 0)
            {
                foreach (var err in errors)
                {
                    Debug.LogError(err);
                }
                throw new TeuchiUdonCompilerException("Compile failed");
            }

            var program  = AssetDatabase.LoadAssetAtPath<TeuchiUdonProgramAsset>(assetPath);
            if (program == null)
            {
                program = ScriptableObject.CreateInstance<TeuchiUdonProgramAsset>();
                program.SetUdonAssembly(output, defaultValues);
                program.RefreshProgram();
                AssetDatabase.CreateAsset(program, assetPath);
            }
            else
            {
                program.SetUdonAssembly(output, defaultValues);
                program.RefreshProgram();
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return program;
        }

        private static (string output, string[] errors, (string name, Type type, object value)[] defaultValues) CompileFromPath(string path)
        {
            var script = AssetDatabase.LoadAssetAtPath<TeuchiUdonScript>(path);
            if (script == null)
            {
                return ("", new string[] { "Asset file not found" }, new (string name, Type type, object value)[0]);
            }
            return CompileFromText(script.text);
        }

        private static (string output, string[] errors, (string name, Type type, object value)[] defaultValues) CompileFromText(string text)
        {
            var ptr = IntPtr.Zero;
            try
            {
                var json = UdonSymbolJsonConverter.ToJson(UdonSymbolExtractor.ExtractSymbols());
                ptr = TeuchiUdonUnityCompiler.compile(text, json);
                var parsed = ParseOutput(ptr);
                return parsed;
            }
            finally
            {
                if (ptr != IntPtr.Zero) TeuchiUdonUnityCompiler.free_str(ptr);
            }
        }

        private static (string output, string[] errors, (string name, Type type, object value)[] defaultValues) ParseOutput(IntPtr ptr)
        {
            var output = TeuchiUdonUnityCompiler.PtrToStringUTF8(ptr);

            if (output.Length > 0 && output[0] == '!')
            {
                return ("", new string[] { output.Substring(1) }, new (string name, Type type, object value)[0]);
            }
            else
            {
                var compiled = CompiledResultJsonConverter.FromJson(output);
                var defaultValues = compiled.default_values.Select(x => {
                    var (ty, value) = TypeConverter.ConvertLiteral(x.ty, x.value);
                    return (x.name, ty, value);
                }).ToArray();
                return (compiled.output, compiled.errors, defaultValues);
            }
        }
    }
}
