using System;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public static class TeuchiUdonParserRunner
    {
        public static (TargetResult result, string error) ParseFromString
        (
            string input,
            TeuchiUdonLogicalErrorHandler logicalErrorHandler,
            TeuchiUdonListener listener
        )
        {
            using (var inputReader = new StringReader(input))
            {
                return ParseFromReader(inputReader, logicalErrorHandler, listener);
            }
        }

        public static (TargetResult result, string error) ParseFromReader
        (
            TextReader inputReader,
            TeuchiUdonLogicalErrorHandler logicalErrorHandler,
            TeuchiUdonListener listener
        )
        {
            using (var outputWriter = new StringWriter())
            using (var errorWriter  = new StringWriter())
            {
                var inputStream = CharStreams.fromTextReader(inputReader);
                var lexer       = new TeuchiUdonLexer(inputStream, outputWriter, errorWriter);
                var tokenStream = new CommonTokenStream(lexer);
                var parser      = new TeuchiUdonParser(tokenStream, outputWriter, errorWriter);

                logicalErrorHandler.SetParser(parser);

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
