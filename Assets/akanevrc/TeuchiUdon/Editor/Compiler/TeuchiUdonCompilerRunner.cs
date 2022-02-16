using System;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using UnityEditor;
using UnityEngine;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public static class TeuchiUdonCompilerRunner
    {
        [MenuItem("Tools/Run Compile (Simple)")]
        public static void RunCompile_Simple()
        {
            RunCompile<TeuchiUdonStrategySimple>("Assets/akanevrc/TeuchiUdon/Test/TeuchiUdonTest.teuchi");
        }

        [MenuItem("Tools/Run Compile (Buffered)")]
        public static void RunCompile_Buffered()
        {
            RunCompile<TeuchiUdonStrategyBuffered>("Assets/akanevrc/TeuchiUdon/Test/TeuchiUdonTest.teuchi");
        }

        private static void RunCompile<T>(string srcPath) where T : TeuchiUdonStrategy, new()
        {
            var (output, error) = CompileFromPath<T>(srcPath);
            Debug.Log(output);
            Debug.Log(error);
        }

        [UnityEditor.MenuItem("Tools/Save TeuchiUdon Asset (Simple)")]
        public static void SaveTeuchiUdonAsset_Simple()
        {
            SaveTeuchiUdonAsset<TeuchiUdonStrategySimple>
            (
                "Assets/akanevrc/TeuchiUdon/Test/TeuchiUdonTest.teuchi",
                "Assets/akanevrc/TeuchiUdon/Test/TeuchiUdonAsm.asset"
            );
            Debug.Log("save succeeded");
        }

        [UnityEditor.MenuItem("Tools/Save TeuchiUdon Asset (Buffered)")]
        public static void SaveTeuchiUdonAsset_Buffered()
        {
            SaveTeuchiUdonAsset<TeuchiUdonStrategyBuffered>
            (
                "Assets/akanevrc/TeuchiUdon/Test/TeuchiUdonTest.teuchi",
                "Assets/akanevrc/TeuchiUdon/Test/TeuchiUdonAsm.asset"
            );
            Debug.Log("save succeeded");
        }

        public static TeuchiUdonProgramAsset SaveTeuchiUdonAsset<T>(string srcPath, string assetPath) where T : TeuchiUdonStrategy, new()
        {
            var (output, error) = CompileFromPath<T>(srcPath);
            if (!string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException(error);
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

        public static (string output, string error) CompileFromPath<T>(string path) where T : TeuchiUdonStrategy, new()
        {
            var script = AssetDatabase.LoadAssetAtPath<TeuchiUdonScript>(path);
            if (script == null)
            {
                return ("", "asset file not found");
            }
            return CompileFromString<T>(script.text);
        }

        public static (string output, string error) CompileFromString<T>(string input) where T : TeuchiUdonStrategy, new()
        {
            using (var inputReader = new StringReader(input))
            {
                return CompileFromReader<T>(inputReader);
            }
        }

        public static (string output, string error) CompileFromReader<T>(TextReader inputReader) where T : TeuchiUdonStrategy, new()
        {
            using (var outputWriter = new StringWriter())
            using (var errorWriter  = new StringWriter())
            {
                var inputStream = CharStreams.fromTextReader(inputReader);
                var lexer       = new TeuchiUdonLexer(inputStream, outputWriter, errorWriter);
                var tokenStream = new CommonTokenStream(lexer);
                var parser      = new TeuchiUdonParser(tokenStream, outputWriter, errorWriter);
                var listener    = new TeuchiUdonListener(parser);

                TeuchiUdonStrategy.SetStrategy<T>();

                TeuchiUdonLogicalErrorHandler.Instance.Init(parser);
                TeuchiUdonTables             .Instance.Init();
                TeuchiUdonQualifierStack     .Instance.Init();
                TeuchiUdonAssemblyWriter     .Instance.Init();

                try
                {
                    ParseTreeWalker.Default.Walk(listener, parser.target());
                }
                catch (Exception ex)
                {
                    return ("", $"{ex}\n{errorWriter}");
                }

                return (outputWriter.ToString(), errorWriter.ToString());
            }
        }
    }
}
