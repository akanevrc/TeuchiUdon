using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using akanevrc.TeuchiUdon.Compiler;

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
        static TeuchiUdonUnityCompilerRunner()
        {
            TeuchiUdonCompilerRunner.Init
            (
                new string[]
                {
                    Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "Data/Managed/UnityEngine.dll"),
                    "./Assets/VRCSDK/Plugins/VRCSDKBase.dll",
                    "./Assets/Udon/External/VRC.Udon.Common.dll",
                    "./Assets/Udon/Editor/External/VRC.Udon.Graph.dll",
                    "./Library/ScriptAssemblies/VRC.Udon.dll",
                    "./Library/ScriptAssemblies/VRC.Udon.Editor.dll"
                }
            );
        }

        public static TeuchiUdonProgramAsset SaveTeuchiUdonAsset(string srcPath, string assetPath)
        {
            var (output, error) = CompileFromPath(srcPath);
            if (!string.IsNullOrEmpty(error))
            {
                throw new TeuchiUdonCompilerException(error);
            }

            var program = AssetDatabase.LoadAssetAtPath<TeuchiUdonProgramAsset>(assetPath);
            if (program == null)
            {
                program = ScriptableObject.CreateInstance<TeuchiUdonProgramAsset>();
                program.SetUdonAssembly(output, TeuchiUdonTables.Instance.GetDefaultValues());
                program.RefreshProgram();
                AssetDatabase.CreateAsset(program, assetPath);
            }
            else
            {
                program.SetUdonAssembly(output, TeuchiUdonTables.Instance.GetDefaultValues());
                program.RefreshProgram();
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return program;
        }

        public static (string output, string error) CompileFromPath(string path)
        {
            var script = AssetDatabase.LoadAssetAtPath<TeuchiUdonScript>(path);
            if (script == null)
            {
                return ("", "asset file not found");
            }
            return TeuchiUdonCompilerRunner.CompileFromString(script.text);
        }
    }
}
