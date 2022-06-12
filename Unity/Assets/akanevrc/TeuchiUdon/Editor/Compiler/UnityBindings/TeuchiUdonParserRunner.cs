using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public static class TeuchiUdonParserRunner
    {
        public static (TargetResult result, IEnumerable<TeuchiUdonParserError> errors) ParseFromString
        (
            string input,
            TeuchiUdonListener listener,
            TeuchiUdonTables tables,
            TeuchiUdonParserErrorOps parserErrorOps
        )
        {
            using (var inputReader = new StringReader(input))
            {
                return ParseFromReader(inputReader, listener, tables, parserErrorOps);
            }
        }

        public static (TargetResult result, IEnumerable<TeuchiUdonParserError> errors) ParseFromReader
        (
            TextReader inputReader,
            TeuchiUdonListener listener,
            TeuchiUdonTables tables,
            TeuchiUdonParserErrorOps parserErrorOps
        )
        {
            using (var outputWriter = new StringWriter())
            using (var errorWriter  = new StringWriter())
            {
                var inputStream = CharStreams.fromTextReader(inputReader);
                var lexer       = new TeuchiUdonLexer(inputStream, outputWriter, errorWriter);
                var tokenStream = new CommonTokenStream(lexer);
                var parser      = new TeuchiUdonParser(tokenStream, outputWriter, errorWriter);

                parser.ParserErrorOps = parserErrorOps;

                try
                {
                    ParseTreeWalker.Default.Walk(listener, parser.target());
                }
                catch (Exception ex)
                {
                    return (null, new TeuchiUdonParserError[] { new TeuchiUdonParserError(null, null, ex.ToString()) }.Concat(tables.ParserErrors));
                }

                return (listener.TargetResult, tables.ParserErrors);
            }
        }
    }
}
