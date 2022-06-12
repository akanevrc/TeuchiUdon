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
        private static TeuchiUdonDllLoader DllLoader { get; set; }
        private static TeuchiUdonPrimitives Primitives { get; set; }
        private static TeuchiUdonStaticTables StaticTables { get; set; }
        private static TeuchiUdonInvalids Invalids { get; set; }

        static TeuchiUdonUnityCompilerRunner()
        {
            CreateSingletons();
        }

        private static void CreateSingletons()
        {
            var dllPaths = new string[]
            {
                Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "Data/Managed/UnityEngine.dll"),
                "./Assets/VRCSDK/Plugins/VRCSDKBase.dll",
                "./Assets/Udon/External/VRC.Udon.Common.dll",
                "./Assets/Udon/Editor/External/VRC.Udon.Graph.dll",
                "./Library/ScriptAssemblies/VRC.Udon.dll",
                "./Library/ScriptAssemblies/VRC.Udon.Editor.dll"
            };

            DllLoader    = new TeuchiUdonDllLoader(dllPaths);
            Primitives   = new TeuchiUdonPrimitives(DllLoader);
            StaticTables = new TeuchiUdonStaticTables(DllLoader, Primitives);
            Invalids     = new TeuchiUdonInvalids();
        }

        private static void Init()
        {
            DllLoader   .Init();
            Primitives  .Init();
            StaticTables.Init();
            Invalids    .Init();
        }

        public static TeuchiUdonProgramAsset SaveTeuchiUdonAsset(string srcPath, string assetPath)
        {
            Init();

            var tables          = new TeuchiUdonTables(Primitives);
            var parserErrorOps  = new TeuchiUdonParserErrorOps(tables);
            var typeOps         = new TeuchiUdonTypeOps(Primitives, StaticTables, Invalids, parserErrorOps);
            var labelOps        = new TeuchiUdonLabelOps(StaticTables, tables);
            var tableOps        = new TeuchiUdonTableOps(Primitives, StaticTables, tables, typeOps, labelOps);
            var qualifierStack  = new TeuchiUdonQualifierStack();
            var outValuePool    = new TeuchiUdonOutValuePool(Invalids);
            var syntaxOps       = new TeuchiUdonSyntaxOps(Primitives, StaticTables, typeOps, tables);
            var parserResultOps = new TeuchiUdonParserResultOps(Primitives, StaticTables, Invalids, parserErrorOps, tables, typeOps, tableOps, outValuePool, syntaxOps);
            var listener        = new TeuchiUdonListener(Primitives, StaticTables, Invalids, parserErrorOps, tables, typeOps, tableOps, qualifierStack, outValuePool, syntaxOps, parserResultOps);

            var dataLabelWrapperOps = new TeuchiUdonDataLabelWrapperOps(Primitives, typeOps, labelOps);
            var compilerStrategy    = new TeuchiUdonCompilerStrategy(Primitives, StaticTables, tables, typeOps, labelOps, outValuePool, syntaxOps, dataLabelWrapperOps);
            var assemblyOps         = new TeuchiUdonAssemblyOps(typeOps, labelOps);
            var assemblyWriter      = new TeuchiUdonAssemblyWriter(tables, compilerStrategy, assemblyOps);

            var (output, error) = CompileFromPath(srcPath, listener, tables, parserErrorOps, compilerStrategy, assemblyWriter);
            if (!string.IsNullOrEmpty(error))
            {
                throw new TeuchiUdonCompilerException(error);
            }

            var program  = AssetDatabase.LoadAssetAtPath<TeuchiUdonProgramAsset>(assetPath);
            if (program == null)
            {
                program = ScriptableObject.CreateInstance<TeuchiUdonProgramAsset>();
                program.SetUdonAssembly(output, tableOps.GetDefaultValues());
                program.RefreshProgram();
                AssetDatabase.CreateAsset(program, assetPath);
            }
            else
            {
                program.SetUdonAssembly(output, tableOps.GetDefaultValues());
                program.RefreshProgram();
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return program;
        }

        private static (string output, string error) CompileFromPath
        (
            string path,
            TeuchiUdonListener listener,
            TeuchiUdonTables tables,
            TeuchiUdonParserErrorOps parserErrorOps,
            TeuchiUdonCompilerStrategy compilerStrategy,
            TeuchiUdonAssemblyWriter assemblyWriter
        )
        {
            var script = AssetDatabase.LoadAssetAtPath<TeuchiUdonScript>(path);
            if (script == null)
            {
                return ("", "asset file not found");
            }
            return TeuchiUdonCompilerRunner.CompileFromString(script.text, listener, tables, parserErrorOps, compilerStrategy, assemblyWriter);
        }
    }
}
