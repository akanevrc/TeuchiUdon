using System;
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
        public static TeuchiUdonProgramAsset SaveTeuchiUdonAsset(string srcPath, string assetPath)
        {
            var (output, error) = CompileFromPath(srcPath);
            if (!string.IsNullOrEmpty(error))
            {
                throw new TeuchiUdonCompilerException(error);
            }

            var program  = AssetDatabase.LoadAssetAtPath<TeuchiUdonProgramAsset>(assetPath);
            if (program == null)
            {
                program = ScriptableObject.CreateInstance<TeuchiUdonProgramAsset>();
                program.SetUdonAssembly(output/*, GetDefaultValues()*/);
                program.RefreshProgram();
                AssetDatabase.CreateAsset(program, assetPath);
            }
            else
            {
                program.SetUdonAssembly(output/*, GetDefaultValues()*/);
                program.RefreshProgram();
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return program;
        }

        private static (string output, string error) CompileFromPath(string path)
        {
            var script = AssetDatabase.LoadAssetAtPath<TeuchiUdonScript>(path);
            if (script == null)
            {
                return ("", "asset file not found");
            }
            return CompileFromText(script.text);
        }

        private static (string output, string error) CompileFromText(string text)
        {
            var output = (string)null;
            try
            {
                output = TeuchiUdonUnityCompiler.compile("let x: int = 123;");
                var parsed = ParseOutput(output);
                return parsed;
            }
            finally
            {
                if (output != null) TeuchiUdonUnityCompiler.free_str(output);
            }
        }

        private static (string output, string error) ParseOutput(string output)
        {
            if (output.Length > 0 && output[0] == '!')
            {
                return ("", output.Substring(1));
            }
            else
            {
                return (output.ToString(), "");
            }
        }
    }
}
