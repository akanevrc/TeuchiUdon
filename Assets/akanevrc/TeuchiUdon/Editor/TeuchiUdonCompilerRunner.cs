using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using UnityEditor;
using UnityEngine;

namespace akanevrc.TeuchiUdon.Editor
{
    public static class TeuchiUdonCompilerRunner
    {
        [MenuItem("Tools/Run Compile")]
        public static void RunCompile()
        {
            CompileFromPath(@"Assets\akanevrc\TeuchiUdon\Test\TeuchiUdonTest.teuchi");
        }

        public static void CompileFromPath(string path)
        {
            using (var inputReader = new StreamReader(File.OpenRead(path)))
            {
                CompileFromReader(inputReader);
            }
        }

        public static void CompileFromString(string input)
        {
            using (var inputReader = new StringReader(input))
            {
                CompileFromReader(inputReader);
            }
        }

        public static void CompileFromReader(TextReader inputReader)
        {
            using (var outputWriter = new StringWriter())
            using (var errorWriter  = new StringWriter())
            {
                var inputStream = CharStreams.fromTextReader(inputReader);
                var lexer       = new TeuchiUdonLexer(inputStream, outputWriter, errorWriter);
                var tokenStream = new CommonTokenStream(lexer);
                var parser      = new TeuchiUdonParser(tokenStream, outputWriter, errorWriter);

                var logicalErrorHandler = new TeuchiUdonLogicalErrorHandler(parser);
                var listener            = new TeuchiUdonListener(logicalErrorHandler);
                ParseTreeWalker.Default.Walk(listener, parser.target());
                Debug.Log(outputWriter.ToString());
                Debug.Log(errorWriter .ToString());
            }
        }
    }
}
