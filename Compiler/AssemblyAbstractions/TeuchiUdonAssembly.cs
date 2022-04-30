
namespace akanevrc.TeuchiUdon.Compiler
{
    public enum TeuchiUdonAssemblyInstruction
    {
        NO_CODE,
        NEW_LINE,
        INDENT,
        COMMENT,
        NOP,
        PUSH,
        POP,
        JUMP_IF_FALSE,
        JUMP,
        EXTERN,
        ANNOTATION,
        JUMP_INDIRECT,
        COPY,
        DATA_START,
        DATA_END,
        CODE_START,
        CODE_END,
        EXPORT_DATA,
        SYNC_DATA,
        DECL_DATA,
        EXPORT_CODE,
        LABEL,
        DUMMY,
        RETAIN_TMP,
        RELEASE_TMP
    }

    public abstract class TeuchiUdonAssemblySyncMode
    {
        public static TeuchiUdonAssemblySyncMode Create(TeuchiUdonSyncMode syncMode)
        {
            switch (syncMode)
            {
                case TeuchiUdonSyncMode.Disable:
                    return null;
                case TeuchiUdonSyncMode.Sync:
                    return new AssemblySyncMode_NONE();
                case TeuchiUdonSyncMode.Linear:
                    return new AssemblySyncMode_LINEAR();
                case TeuchiUdonSyncMode.Smooth:
                    return new AssemblySyncMode_SMOOTH();
                default:
                    return null;
            }
        }
    }

    public class AssemblySyncMode_NONE : TeuchiUdonAssemblySyncMode
    {
    }

    public class AssemblySyncMode_LINEAR : TeuchiUdonAssemblySyncMode
    {
    }

    public class AssemblySyncMode_SMOOTH : TeuchiUdonAssemblySyncMode
    {
    }

    public abstract class TeuchiUdonAssemblyLiteral
    {
    }

    public class AssemblyLiteral_NULL : TeuchiUdonAssemblyLiteral
    {
    }

    public class AssemblyLiteral_THIS : TeuchiUdonAssemblyLiteral
    {
    }

    public class AssemblyLiteral_ADDRESS : TeuchiUdonAssemblyLiteral
    {
        public uint Address { get; }

        public AssemblyLiteral_ADDRESS(uint address)
        {
            Address = address;
        }
    }

    public class AssemblyLiteral_RAW : TeuchiUdonAssemblyLiteral
    {
        public string Value { get; }

        public AssemblyLiteral_RAW(string value)
        {
            Value = value;
        }
    }

    public abstract class TeuchiUdonAssemblyDataAddress
    {
        public abstract IDataLabel GetLabel();
    }

    public class AssemblyAddress_DATA_LABEL : TeuchiUdonAssemblyDataAddress
    {
        public IDataLabel Label { get; }

        public AssemblyAddress_DATA_LABEL(IDataLabel label)
        {
            Label = label;
        }

        public override IDataLabel GetLabel()
        {
            return Label;
        }
    }

    public class AssemblyAddress_INDIRECT_LABEL : TeuchiUdonAssemblyDataAddress
    {
        public TeuchiUdonIndirect Indirect { get; }

        public AssemblyAddress_INDIRECT_LABEL(TeuchiUdonIndirect indirect)
        {
            Indirect = indirect;
        }

        public override IDataLabel GetLabel()
        {
            return Indirect;
        }
    }

    public abstract class TeuchiUdonAssemblyCodeAddress
    {
    }

    public class AssemblyAddress_CODE_LABEL : TeuchiUdonAssemblyCodeAddress
    {
        public ICodeLabel Label { get; }

        public AssemblyAddress_CODE_LABEL(ICodeLabel label)
        {
            Label = label;
        }
    }

    public class AssemblyAddress_NUMBER : TeuchiUdonAssemblyCodeAddress
    {
        public uint Number { get; }

        public AssemblyAddress_NUMBER(uint number)
        {
            Number = number;
        }
    }

    public abstract class TeuchiUdonAssembly
    {
        public TeuchiUdonAssemblyInstruction Instruction { get; }
        public uint Size { get; }

        public TeuchiUdonAssembly(TeuchiUdonAssemblyInstruction instruction, uint size)
        {
            Instruction = instruction;
            Size        = size;
        }
    }

    public abstract class Assembly_UnaryDataAddress : TeuchiUdonAssembly
    {
        public TeuchiUdonAssemblyDataAddress Address { get; }

        public Assembly_UnaryDataAddress(TeuchiUdonAssemblyInstruction instruction, uint size, TeuchiUdonAssemblyDataAddress address)
            : base(instruction, size)
        {
            Address = address;
        }
    }

    public abstract class Assembly_UnaryCodeAddress : TeuchiUdonAssembly
    {
        public TeuchiUdonAssemblyCodeAddress Address { get; }

        public Assembly_UnaryCodeAddress(TeuchiUdonAssemblyInstruction instruction, uint size, TeuchiUdonAssemblyCodeAddress address)
            : base(instruction, size)
        {
            Address = address;
        }
    }

    public class Assembly_NO_CODE : TeuchiUdonAssembly
    {
        public Assembly_NO_CODE()
            : base(TeuchiUdonAssemblyInstruction.NO_CODE, 0)
        {
        }
    }

    public class Assembly_NEW_LINE : TeuchiUdonAssembly
    {
        public Assembly_NEW_LINE()
            : base(TeuchiUdonAssemblyInstruction.NEW_LINE, 0)
        {
        }
    }

    public class Assembly_INDENT : TeuchiUdonAssembly
    {
        public int Level { get; }

        public Assembly_INDENT(int level)
            : base(TeuchiUdonAssemblyInstruction.INDENT, 0)
        {
            Level = level;
        }
    }

    public class Assembly_COMMENT : TeuchiUdonAssembly
    {
        public string Text { get; }

        public Assembly_COMMENT(string text)
            : base(TeuchiUdonAssemblyInstruction.COMMENT, 0)
        {
            Text = text;
        }
    }

    public class Assembly_NOP : TeuchiUdonAssembly
    {
        public Assembly_NOP()
            : base(TeuchiUdonAssemblyInstruction.NOP, 4)
        {
        }
    }

    public class Assembly_PUSH : Assembly_UnaryDataAddress
    {
        public Assembly_PUSH(TeuchiUdonAssemblyDataAddress address)
            : base(TeuchiUdonAssemblyInstruction.PUSH, 8, address)
        {
        }
    }

    public class Assembly_POP : TeuchiUdonAssembly
    {
        public Assembly_POP()
            : base(TeuchiUdonAssemblyInstruction.POP, 4)
        {
        }
    }

    public class Assembly_JUMP_IF_FALSE : Assembly_UnaryCodeAddress
    {
        public Assembly_JUMP_IF_FALSE(TeuchiUdonAssemblyCodeAddress address)
            : base(TeuchiUdonAssemblyInstruction.JUMP_IF_FALSE, 8, address)
        {
        }
    }

    public class Assembly_JUMP : Assembly_UnaryCodeAddress
    {
        public Assembly_JUMP(TeuchiUdonAssemblyCodeAddress address)
            : base(TeuchiUdonAssemblyInstruction.JUMP, 8, address)
        {
        }
    }

    public class Assembly_EXTERN : TeuchiUdonAssembly
    {
        public TeuchiUdonMethod Method { get; }

        public Assembly_EXTERN(TeuchiUdonMethod method)
            : base(TeuchiUdonAssemblyInstruction.EXTERN, 8)
        {
            Method = method;
        }
    }

    public class Assembly_ANNOTATION : TeuchiUdonAssembly
    {
        public Assembly_ANNOTATION()
            : base(TeuchiUdonAssemblyInstruction.ANNOTATION, 4)
        {
        }
    }

    public class Assembly_JUMP_INDIRECT : Assembly_UnaryDataAddress
    {
        public Assembly_JUMP_INDIRECT(TeuchiUdonAssemblyDataAddress address)
            : base(TeuchiUdonAssemblyInstruction.JUMP_INDIRECT, 8, address)
        {
        }
    }

    public class Assembly_COPY : TeuchiUdonAssembly
    {
        public Assembly_COPY()
            : base(TeuchiUdonAssemblyInstruction.COPY, 4)
        {
        }
    }

    public class Assembly_DATA_START : TeuchiUdonAssembly
    {
        public Assembly_DATA_START()
            : base(TeuchiUdonAssemblyInstruction.DATA_START, 0)
        {
        }
    }

    public class Assembly_DATA_END : TeuchiUdonAssembly
    {
        public Assembly_DATA_END()
            : base(TeuchiUdonAssemblyInstruction.DATA_END, 0)
        {
        }
    }

    public class Assembly_CODE_START : TeuchiUdonAssembly
    {
        public Assembly_CODE_START()
            : base(TeuchiUdonAssemblyInstruction.CODE_START, 0)
        {
        }
    }

    public class Assembly_CODE_END : TeuchiUdonAssembly
    {
        public Assembly_CODE_END()
            : base(TeuchiUdonAssemblyInstruction.CODE_END, 0)
        {
        }
    }

    public class Assembly_EXPORT_DATA : TeuchiUdonAssembly
    {
        public IDataLabel Data { get; }

        public Assembly_EXPORT_DATA(IDataLabel data)
            : base(TeuchiUdonAssemblyInstruction.EXPORT_DATA, 0)
        {
            Data = data;
        }
    }

    public class Assembly_SYNC_DATA : TeuchiUdonAssembly
    {
        public IDataLabel Data { get; }
        public TeuchiUdonAssemblySyncMode SyncMode { get; }

        public Assembly_SYNC_DATA(IDataLabel data, TeuchiUdonAssemblySyncMode syncMode)
            : base(TeuchiUdonAssemblyInstruction.SYNC_DATA, 0)
        {
            Data     = data;
            SyncMode = syncMode;
        }
    }

    public class Assembly_DECL_DATA : TeuchiUdonAssembly
    {
        public IDataLabel Data { get; }
        public TeuchiUdonType Type { get; }
        public TeuchiUdonAssemblyLiteral Literal { get; }

        public Assembly_DECL_DATA(IDataLabel data, TeuchiUdonType type, TeuchiUdonAssemblyLiteral literal)
            : base(TeuchiUdonAssemblyInstruction.DECL_DATA, 1)
        {
            Data    = data;
            Type    = type;
            Literal = literal;
        }
    }

    public class Assembly_EXPORT_CODE : TeuchiUdonAssembly
    {
        public ICodeLabel Code { get; }

        public Assembly_EXPORT_CODE(ICodeLabel code)
            : base(TeuchiUdonAssemblyInstruction.EXPORT_CODE, 0)
        {
            Code = code;
        }
    }

    public class Assembly_LABEL : TeuchiUdonAssembly
    {
        public ICodeLabel Label { get; }

        public Assembly_LABEL(ICodeLabel label)
            : base(TeuchiUdonAssemblyInstruction.LABEL, 0)
        {
            Label = label;
        }
    }
}
