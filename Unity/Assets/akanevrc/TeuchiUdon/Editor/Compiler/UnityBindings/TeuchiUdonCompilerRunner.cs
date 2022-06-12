using System.IO;
using System.Linq;
using akanevrc.TeuchiUdon.Compiler;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public static class TeuchiUdonCompilerRunner
    {
        public static (string output, string error) CompileFromString
        (
            string input,
            TeuchiUdonListener listener,
            TeuchiUdonTables tables,
            TeuchiUdonParserErrorOps parserErrorOps,
            TeuchiUdonCompilerStrategy compilerStrategy,
            TeuchiUdonAssemblyWriter assemblyWriter
        )
        {
            using (var inputReader = new StringReader(input))
            {
                return CompileFromReader(inputReader, listener, tables, parserErrorOps, compilerStrategy, assemblyWriter);
            }
        }

        public static (string output, string error) CompileFromReader
        (
            TextReader inputReader,
            TeuchiUdonListener listener,
            TeuchiUdonTables tables,
            TeuchiUdonParserErrorOps parserErrorOps,
            TeuchiUdonCompilerStrategy compilerStrategy,
            TeuchiUdonAssemblyWriter assemblyWriter
        )
        {
            var (result, errors) = TeuchiUdonParserRunner.ParseFromReader(inputReader, listener, tables, parserErrorOps);

            if (!errors.Any())
            {
                using (var outputWriter = new StringWriter())
                {
                    Compile(result, outputWriter, compilerStrategy, assemblyWriter);
                    return (outputWriter.ToString(), "");
                }
            }
            else
            {
                return ("", string.Join("\n", errors));
            }
        }

        private static void Compile
        (
            TargetResult result,
            TextWriter outputWriter,
            TeuchiUdonCompilerStrategy compilerStrategy,
            TeuchiUdonAssemblyWriter assemblyWriter
        )
        {
            assemblyWriter.PushDataPart
            (
                compilerStrategy.GetDataPartFromTables(),
                compilerStrategy.GetDataPartFromOutValuePool()
            );
            if (result != null && result.Valid)
            {
                assemblyWriter.PushCodePart
                (
                    compilerStrategy.GetCodePartFromTables(),
                    compilerStrategy.GetCodePartFromResult(result)
                );
            }
            else
            {
                assemblyWriter.PushCodePart
                (
                    compilerStrategy.GetCodePartFromTables()
                );
            }
            assemblyWriter.Prepare();
            assemblyWriter.WriteAll(outputWriter);
        }
    }
}
