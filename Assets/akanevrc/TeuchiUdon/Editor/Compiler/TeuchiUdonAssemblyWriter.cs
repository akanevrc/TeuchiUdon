using System.Collections.Generic;
using System.IO;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonAssemblyWriter
    {
        public static TeuchiUdonAssemblyWriter Instance { get; } = new TeuchiUdonAssemblyWriter();

        private List<TeuchiUdonAssembly> DataBlock { get; set; }
        private List<TeuchiUdonAssembly> CodeBlock { get; set; }

        public void Init()
        {
            DataBlock = new List<TeuchiUdonAssembly>();
            CodeBlock = new List<TeuchiUdonAssembly>();
        }

        public void PushDataBlock(IEnumerable<TeuchiUdonAssembly> assemblies)
        {
            DataBlock.AddRange(assemblies);
        }

        public void PushCodeBlock(IEnumerable<TeuchiUdonAssembly> assemblies)
        {
            CodeBlock.AddRange(assemblies);
        }

        public void WriteAll(TextWriter writer)
        {
            var indent = 0;

            WriteOne(writer, new Assembly_DATA_START() , ref indent);
            WriteOne(writer, new Assembly_INDENT    (1), ref indent);
            foreach (var asm in DataBlock)
            {
                WriteOne(writer, asm, ref indent);
            }
            WriteOne(writer, new Assembly_INDENT  (-1), ref indent);
            WriteOne(writer, new Assembly_DATA_END()  , ref indent);

            WriteOne(writer, new Assembly_CODE_START() , ref indent);
            WriteOne(writer, new Assembly_INDENT    (1), ref indent);
            foreach (var asm in CodeBlock)
            {
                WriteOne(writer, asm, ref indent);
            }
            WriteOne(writer, new Assembly_INDENT  (-1), ref indent);
            WriteOne(writer, new Assembly_CODE_END()  , ref indent);
        }

        private void WriteOne(TextWriter writer, TeuchiUdonAssembly assembly, ref int indent)
        {
            if (assembly is Assembly_NO_CODE) return;

            if (assembly is Assembly_NEW_LINE)
            {
                writer.WriteLine();
                return;
            }

            if (assembly is Assembly_INDENT assemblyIndent)
            {
                indent += assemblyIndent.Level;
                if (indent < 0) indent = 0;
                return;
            }

            WriteIndent(writer, indent);
            writer.WriteLine(assembly);
        }

        private void WriteIndent(TextWriter writer, int indent)
        {
            writer.Write(new string(' ', indent * 4));
        }
    }
}
