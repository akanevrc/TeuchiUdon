using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace akanevrc.TeuchiUdon.Compiler
{
    public class TeuchiUdonAssemblyWriter
    {
        public static TeuchiUdonAssemblyWriter Instance { get; } = new TeuchiUdonAssemblyWriter();

        private List<TeuchiUdonAssembly> DataPart { get; set; }
        private List<TeuchiUdonAssembly> CodePart { get; set; }
        private Dictionary<IDataLabel, uint> DataAddresses { get; set; }
        private Dictionary<ICodeLabel, uint> CodeAddresses { get; set; }
        private uint DataAddress { get; set; }
        private uint CodeAddress { get; set; }

        protected TeuchiUdonAssemblyWriter()
        {
        }
        
        public void Init()
        {
            DataPart      = new List<TeuchiUdonAssembly>();
            CodePart      = new List<TeuchiUdonAssembly>();
            DataAddresses = new Dictionary<IDataLabel, uint>();
            CodeAddresses = new Dictionary<ICodeLabel, uint>();
            DataAddress   = 0;
            CodeAddress   = 0;
        }

        public void PushDataPart(params IEnumerable<TeuchiUdonAssembly>[] assembliess)
        {
            foreach (var asms in assembliess)
            {
                DataPart.AddRange(asms);
            }
        }

        public void PushCodePart(params IEnumerable<TeuchiUdonAssembly>[] assembliess)
        {
            if (assembliess.Length >= 1) CodePart.AddRange(assembliess[0]);
            for (var i = 1; i < assembliess.Length; i++)
            {
                if (assembliess[i - 1].Any()) CodePart.Add(new Assembly_NEW_LINE());
                CodePart.AddRange(assembliess[i]);
            }
        }

        public void Prepare()
        {
            DataAddresses = new Dictionary<IDataLabel, uint>();
            CodeAddresses = new Dictionary<ICodeLabel, uint>();
            DataAddress   = 0;
            CodeAddress   = 0;

            for (var i = 0; i < CodePart.Count - 1; i++)
            {
                if (CodePart[i] is Assembly_LABEL && CodePart[i + 1] is Assembly_LABEL)
                {
                    CodePart.Insert(i + 1, new Assembly_NOP());
                }
            }

            foreach (var asm in CodePart)
            {
                if (asm is Assembly_LABEL label && !CodeAddresses.ContainsKey(label.Label))
                {
                    CodeAddresses.Add(label.Label, CodeAddress);
                }
                CodeAddress += asm.Size;
            }

            foreach (var asm in CodePart)
            {
                if (asm is Assembly_UnaryDataAddress dataAddress && dataAddress.Address is AssemblyAddress_INDIRECT_LABEL label)
                {
                    var indirect = label.Indirect;
                    var address  = CodeAddresses[label.Indirect.Label];
                    if (TeuchiUdonTables.Instance.Indirects.ContainsKey(indirect))
                    {
                        TeuchiUdonTables.Instance.Indirects[indirect] = address;
                    }
                }
            }
            PushDataPart(TeuchiUdonCompilerStrategy.Instance.DeclIndirectAddresses(TeuchiUdonTables.Instance.Indirects.Select(x => (x.Key, x.Value))));

            foreach (var asm in CodePart)
            {
                if (asm is Assembly_UnaryDataAddress dasm)
                {
                    TeuchiUdonTables.Instance.UsedData.Add(dasm.Address.GetLabel());
                }
            }

            foreach (var asm in DataPart)
            {
                if (asm is Assembly_DECL_DATA declData && TeuchiUdonTables.Instance.UsedData.Contains(declData.Data))
                {
                    if (!DataAddresses.ContainsKey(declData.Data))
                    {
                        DataAddresses.Add(declData.Data, DataAddress);
                    }
                    DataAddress += asm.Size;
                }
            }
        }

        public void WriteAll(TextWriter writer)
        {
            var indent = 0;

            WriteOne(writer, new Assembly_DATA_START() , ref indent);
            WriteOne(writer, new Assembly_INDENT    (1), ref indent);
            foreach (var asm in DataPart)
            {
                WriteOne(writer, asm, ref indent);
            }
            WriteOne(writer, new Assembly_INDENT  (-1), ref indent);
            WriteOne(writer, new Assembly_DATA_END()  , ref indent);

            WriteOne(writer, new Assembly_NEW_LINE()  , ref indent);

            WriteOne(writer, new Assembly_CODE_START() , ref indent);
            WriteOne(writer, new Assembly_INDENT    (1), ref indent);
            foreach (var asm in CodePart)
            {
                WriteOne(writer, asm, ref indent);
            }
            WriteOne(writer, new Assembly_INDENT  (-1), ref indent);
            WriteOne(writer, new Assembly_CODE_END()  , ref indent);
        }

        private void WriteOne(TextWriter writer, TeuchiUdonAssembly assembly, ref int indent)
        {
            if
            (
                assembly is Assembly_EXPORT_DATA assemblyExportData && !TeuchiUdonTables.Instance.UsedData.Contains(assemblyExportData.Data) ||
                assembly is Assembly_SYNC_DATA   assemblySyncData   && !TeuchiUdonTables.Instance.UsedData.Contains(assemblySyncData  .Data) ||
                assembly is Assembly_DECL_DATA   assemblyDeclData   && !TeuchiUdonTables.Instance.UsedData.Contains(assemblyDeclData  .Data)
            )
            {
                return;
            }

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
