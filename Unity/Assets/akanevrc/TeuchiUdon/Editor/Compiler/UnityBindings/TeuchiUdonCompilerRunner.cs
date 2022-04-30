using System.IO;
using akanevrc.TeuchiUdon.Compiler;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public static class TeuchiUdonCompilerRunner
    {
        public static (string output, string error) CompileFromString
        (
            string input,
            TeuchiUdonLogicalErrorHandler logicalErrorHandler,
            TeuchiUdonListener listener,
            TeuchiUdonCompilerStrategy compilerStrategy,
            TeuchiUdonAssemblyWriter assemblyWriter
        )
        {
            using (var inputReader = new StringReader(input))
            {
                return CompileFromReader(inputReader, logicalErrorHandler, listener, compilerStrategy, assemblyWriter);
            }
        }

        public static (string output, string error) CompileFromReader
        (
            TextReader inputReader,
            TeuchiUdonLogicalErrorHandler logicalErrorHandler,
            TeuchiUdonListener listener,
            TeuchiUdonCompilerStrategy compilerStrategy,
            TeuchiUdonAssemblyWriter assemblyWriter
        )
        {
            var (result, error) = TeuchiUdonParserRunner.ParseFromReader(inputReader, logicalErrorHandler, listener);

            if (string.IsNullOrEmpty(error))
            {
                using (var outputWriter = new StringWriter())
                {
                    Compile(result, outputWriter, compilerStrategy, assemblyWriter);
                    return (outputWriter.ToString(), "");
                }
            }
            else
            {
                return ("", error);
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
