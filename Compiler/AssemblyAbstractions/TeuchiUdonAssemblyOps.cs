using System;

namespace akanevrc.TeuchiUdon.Compiler
{
    public class TeuchiUdonAssemblyOps
    {
        private TeuchiUdonTypeOps TypeOps { get; }
        private TeuchiUdonLabelOps LabelOps { get; }

        public TeuchiUdonAssemblyOps
        (
            TeuchiUdonTypeOps typeOps,
            TeuchiUdonLabelOps labelOps
        )
        {
            TypeOps  = typeOps;
            LabelOps = labelOps;
        }

        public string ToString(TeuchiUdonAssemblySyncMode obj)
        {
            switch (obj)
            {
                case AssemblySyncMode_NONE _:
                    return "none";
                case AssemblySyncMode_LINEAR _:
                    return "linear";
                case AssemblySyncMode_SMOOTH _:
                    return "smooth";
                default:
                    throw new NotSupportedException("unsupported type");
            }
        }

        public string ToString(TeuchiUdonAssemblyLiteral obj)
        {
            switch (obj)
            {
                case AssemblyLiteral_NULL _:
                    return $"null";
                case AssemblyLiteral_THIS _:
                    return $"this";
                case AssemblyLiteral_ADDRESS address:
                    return $"0x{address.Address.ToString("X8")}";
                case AssemblyLiteral_RAW raw:
                    return raw.Value;
                default:
                    throw new NotSupportedException("unsupported type");
            }
        }

        public string ToString(TeuchiUdonAssemblyDataAddress obj)
        {
            switch (obj)
            {
                case AssemblyAddress_DATA_LABEL dataLabel:
                    return LabelOps.GetFullLabel(dataLabel.Label);
                case AssemblyAddress_INDIRECT_LABEL indirectLabel:
                    return LabelOps.GetFullLabel(indirectLabel.Indirect);
                default:
                    throw new NotSupportedException("unsupported type");
            }
        }

        public string ToString(TeuchiUdonAssemblyCodeAddress obj)
        {
            switch (obj)
            {
                case AssemblyAddress_CODE_LABEL codeLabel:
                    return LabelOps.GetFullLabel(codeLabel.Label);
                case AssemblyAddress_NUMBER number:
                    return $"0x{number.Number.ToString("X8")}";
                default:
                    throw new NotSupportedException("unsupported type");
            }
        }

        public string ToString(TeuchiUdonAssembly obj)
        {
            switch (obj)
            {
                case Assembly_COMMENT comment:
                    return $"# {comment.Text}";
                case Assembly_NOP _:
                    return $"NOP";
                case Assembly_PUSH push:
                    return $"PUSH, {ToString(push.Address)}";
                case Assembly_POP _:
                    return $"POP";
                case Assembly_JUMP_IF_FALSE jumpIfFalse:
                    return $"JUMP_IF_FALSE, {ToString(jumpIfFalse.Address)}";
                case Assembly_JUMP jump:
                    return $"JUMP, {ToString(jump.Address)}";
                case Assembly_EXTERN extern_:
                    return $"EXTERN, \"{extern_.Method.UdonName}\"";
                case Assembly_ANNOTATION _:
                    return $"ANNOTATION";
                case Assembly_JUMP_INDIRECT jumpIndirect:
                    return $"JUMP_INDIRECT, {ToString(jumpIndirect.Address)}";
                case Assembly_COPY _:
                    return $"COPY";
                case Assembly_DATA_START _:
                    return $".data_start";
                case Assembly_DATA_END _:
                    return $".data_end";
                case Assembly_CODE_START _:
                    return $".code_start";
                case Assembly_CODE_END _:
                    return $".code_end";
                case Assembly_EXPORT_DATA exportData:
                    return $".export {LabelOps.GetFullLabel(exportData.Data)}";
                case Assembly_SYNC_DATA syncData:
                    return $".sync {LabelOps.GetFullLabel(syncData.Data)}, {ToString(syncData.SyncMode)}";
                case Assembly_DECL_DATA declData:
                    return $"{LabelOps.GetFullLabel(declData.Data)}: %{TypeOps.GetRealName(declData.Type)}, {ToString(declData.Literal)}";
                case Assembly_EXPORT_CODE exportCode:
                    return $".export {LabelOps.GetFullLabel(exportCode.Code)}";
                case Assembly_LABEL label:
                    return $"{LabelOps.GetFullLabel(label.Label)}:";
                default:
                    throw new NotSupportedException("unsupported type");
            }
        }
    }
}
