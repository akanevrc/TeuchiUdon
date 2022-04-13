using System.Collections.Generic;
using System.IO;

namespace akanevrc.TeuchiUdon.Compiler
{
    public class TeuchiUdonCompilerRunner
    {
        public static void Init(IEnumerable<string> dllPaths)
        {
            TeuchiUdonParserRunner.Init(dllPaths);
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
            var (result, error) = TeuchiUdonParserRunner.ParseFromReader(inputReader);

            TeuchiUdonCompilerStrategy.Instance.Init();
            TeuchiUdonAssemblyWriter  .Instance.Init();

            if (string.IsNullOrEmpty(error))
            {
                using (var outputWriter = new StringWriter())
                {
                    Compile(result, outputWriter);
                    return (outputWriter.ToString(), "");
                }
            }
            else
            {
                return ("", error);
            }
        }

        private static void Compile(TargetResult result, TextWriter outputWriter)
        {
            TeuchiUdonAssemblyWriter.Instance.PushDataPart
            (
                TeuchiUdonCompilerStrategy.Instance.GetDataPartFromTables(),
                TeuchiUdonCompilerStrategy.Instance.GetDataPartFromOutValuePool()
            );
            if (result == null)
            {
                TeuchiUdonAssemblyWriter.Instance.PushCodePart
                (
                    TeuchiUdonCompilerStrategy.Instance.GetCodePartFromTables()
                );
            }
            else
            {
                TeuchiUdonAssemblyWriter.Instance.PushCodePart
                (
                    TeuchiUdonCompilerStrategy.Instance.GetCodePartFromTables(),
                    TeuchiUdonCompilerStrategy.Instance.GetCodePartFromResult(result)
                );
            }
            TeuchiUdonAssemblyWriter.Instance.Prepare();
            TeuchiUdonAssemblyWriter.Instance.WriteAll(outputWriter);
        }
    }
}
