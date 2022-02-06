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
        public static void RunCompile()
        {
            var (output, error) = CompileFromPath(@"Assets\akanevrc\TeuchiUdon\Test\TeuchiUdonTest.teuchi");
            Debug.Log(output);
            Debug.Log(error);
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

                ParseTreeWalker.Default.Walk(listener, parser.target());

                return (outputWriter.ToString(), errorWriter.ToString());
            }
        }

        [UnityEditor.MenuItem("Tools/Save TeuchiUdon Asset...")]
        public static void SaveTeuchiUdonAsset()
        {
            var compilerResult = CompileFromPath("Assets/akanevrc/TeuchiUdon/Test/TeuchiUdonTest.teuchi");
            if (!string.IsNullOrEmpty(compilerResult.error))
            {
                Debug.LogError(compilerResult.error);
                return;
            }

            var assetPath = "Assets/akanevrc/TeuchiUdon/Test/TeuchiUdonAsm.asset";
            var program   = AssetDatabase.LoadAssetAtPath<TeuchiUdonProgramAsset>(assetPath);
            if (program == null)
            {
                program = ScriptableObject.CreateInstance<TeuchiUdonProgramAsset>();
                program.SetUdonAssembly(compilerResult.output);
                program.RefreshProgram();
                AssetDatabase.CreateAsset(program, assetPath);
            }
            else
            {
                program.SetUdonAssembly(compilerResult.output);
                program.RefreshProgram();
            }
            AssetDatabase.Refresh();
        }
    }
}
