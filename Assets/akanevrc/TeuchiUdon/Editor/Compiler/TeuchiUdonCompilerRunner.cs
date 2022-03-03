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
        [MenuItem("Tools/Run Compile")]
        public static void Tool_RunCompile()
        {
            var (output, error) = CompileFromPath("Assets/akanevrc/TeuchiUdon/Tests/tmp/TeuchiUdonTest.teuchi");
            Debug.Log(output);
            Debug.Log(error);
        }

        [UnityEditor.MenuItem("Tools/Save TeuchiUdon Asset")]
        public static void Tool_SaveTeuchiUdonAsset()
        {
            SaveTeuchiUdonAsset
            (
                "Assets/akanevrc/TeuchiUdon/Tests/tmp/TeuchiUdonTest.teuchi",
                "Assets/akanevrc/TeuchiUdon/Tests/tmp/TeuchiUdonAsm.asset"
            );
            Debug.Log("save succeeded");
        }

        public static TeuchiUdonProgramAsset SaveTeuchiUdonAsset(string srcPath, string assetPath)
        {
            var (output, error) = CompileFromPath(srcPath);
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

        public static (string output, string error) CompileFromPath(string path)
        {
            var script = AssetDatabase.LoadAssetAtPath<TeuchiUdonScript>(path);
            if (script == null)
            {
                return ("", "asset file not found");
            }
            return CompileFromString(script.text);
        }

        public static (string output, string error) CompileFromString(string input)
        {
            using (var inputReader = new StringReader(input))
            {
                return CompileFromReader(inputReader);
            }
        }

        public static (string output, string error) CompileFromReader(TextReader inputReader)
        {
            using (var outputWriter = new StringWriter())
            using (var errorWriter  = new StringWriter())
            {
                var inputStream = CharStreams.fromTextReader(inputReader);
                var lexer       = new TeuchiUdonLexer(inputStream, outputWriter, errorWriter);
                var tokenStream = new CommonTokenStream(lexer);
                var parser      = new TeuchiUdonParser(tokenStream, outputWriter, errorWriter);
                var listener    = new TeuchiUdonListener(parser);

                TeuchiUdonLogicalErrorHandler.Instance.Init(parser);
                TeuchiUdonTables             .Instance.Init();
                TeuchiUdonQualifierStack     .Instance.Init();
                TeuchiUdonAssemblyWriter     .Instance.Init();
                TeuchiUdonCompilerStrategy           .Instance.Init();

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
