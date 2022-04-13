using System;
using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonParserRunner
    {
        public static void Init(IEnumerable<string> dllPaths)
        {
            TeuchiUdonDllLoader.Instance.Init(dllPaths);
        }

        public static (TargetResult result, string error) ParseFromString(string input)
        {
            using (var inputReader = new StringReader(input))
            {
                return ParseFromReader(inputReader);
            }
        }

        public static (TargetResult result, string error) ParseFromReader(TextReader inputReader)
        {
            using (var outputWriter = new StringWriter())
            using (var errorWriter  = new StringWriter())
            {
                var inputStream = CharStreams.fromTextReader(inputReader);
                var lexer       = new TeuchiUdonLexer(inputStream, outputWriter, errorWriter);
                var tokenStream = new CommonTokenStream(lexer);
                var parser      = new TeuchiUdonParser(tokenStream, outputWriter, errorWriter);
                var listener    = new TeuchiUdonListener(parser);

                PrimitiveTypes               .Instance.Init();
                TeuchiUdonLogicalErrorHandler.Instance.Init(parser);
                TeuchiUdonTables             .Instance.Init();
                TeuchiUdonQualifierStack     .Instance.Init();
                TeuchiUdonOutValuePool       .Instance.Init();

                try
                {
                    ParseTreeWalker.Default.Walk(listener, parser.target());
                }
                catch (Exception ex)
                {
                    return (null, $"{ex}\n{errorWriter}");
                }

                return (listener.TargetResult, errorWriter.ToString());
            }
        }
    }
}
