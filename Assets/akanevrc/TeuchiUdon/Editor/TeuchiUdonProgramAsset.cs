using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using UnityEditor;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.ProgramSources;
using VRC.Udon.Editor.ProgramSources;
using akanevrc.TeuchiUdon.Editor.Compiler;

namespace akanevrc.TeuchiUdon.Editor
{
    public class TeuchiUdonProgramAsset : UdonAssemblyProgramAsset
    {
        [UnityEditor.MenuItem("Tools/Save TeuchiUdon Asset...")]
        public static void SaveTeuchiUdonAsset()
        {
            var script  = UnityEditor.AssetDatabase.LoadAssetAtPath<TeuchiUdonScript>("Assets/akanevrc/TeuchiUdon/Test/TeuchiUdonTest.teuchi");
            var program = ScriptableObject.CreateInstance<TeuchiUdonProgramAsset>();
            program.sourceScript = script;
            program.RefreshProgram();
            UnityEditor.AssetDatabase.CreateAsset(program, "Assets/akanevrc/TeuchiUdon/Test/TeuchiUdonAsm.asset");
            UnityEditor.AssetDatabase.Refresh();
        }

        public TeuchiUdonScript sourceScript;

        protected override void RefreshProgramImpl()
        {
            Compile();
            base.RefreshProgramImpl();
        }

        protected void Compile()
        {
            if (sourceScript == null) return;

            using (var outputWriter = new StringWriter())
            using (var errorWriter  = new StringWriter())
            {
                var inputStream = CharStreams.fromString(sourceScript.text);
                var lexer       = new TeuchiUdonLexer(inputStream, outputWriter, errorWriter);
                var tokenStream = new CommonTokenStream(lexer);
                var parser      = new TeuchiUdonParser(tokenStream, outputWriter, errorWriter);
                var listener    = new TeuchiUdonListener(parser);

                TeuchiUdonLogicalErrorHandler.Instance.Init(parser);
                TeuchiUdonTables             .Instance.Init();
                TeuchiUdonQualifierStack     .Instance.Init();
                TeuchiUdonAssemblyWriter     .Instance.Init();

                ParseTreeWalker.Default.Walk(listener, parser.target());

                var output = outputWriter.ToString();
                var error  = errorWriter .ToString();
                if (string.IsNullOrEmpty(error))
                {
                    udonAssembly = output;
                }
                else
                {
                    Debug.LogError(error);
                }
            }
        }
    }
}
