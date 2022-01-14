
namespace akanevrc.TeuchiUdon.Editor.Compiler
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
        LABEL
    }

    public abstract class TeuchiUdonAssemblySyncMode
    {
    }

    public class AssemblySyncMode_NONE
    {
        public override string ToString()
        {
            return "none";
        }
    }

    public class AssemblySyncMode_LINEAR
    {
        public override string ToString()
        {
            return "linear";
        }
    }

    public class AssemblySyncMode_SMOOTH
    {
        public override string ToString()
        {
            return "smooth";
        }
    }

    public abstract class TeuchiUdonAssemblyLiteral
    {
    }

    public class AssemblyLiteral_NULL : TeuchiUdonAssemblyLiteral
    {
        public override string ToString()
        {
            return $"null";
        }
    }

    public class AssemblyLiteral_VALUE : TeuchiUdonAssemblyLiteral
    {
        public string Value { get; }

        public AssemblyLiteral_VALUE(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }

    public abstract class TeuchiUdonAssemblyAddress
    {
    }

    public class AssemblyAddress_LABEL : TeuchiUdonAssemblyAddress
    {
        public string Label { get; }

        public AssemblyAddress_LABEL(string label)
        {
            Label = label;
        }

        public override string ToString()
        {
            return Label;
        }
    }

    public class AssemblyAddress_NUMBER : TeuchiUdonAssemblyAddress
    {
        public uint Number { get; }

        public AssemblyAddress_NUMBER(uint number)
        {
            Number = number;
        }

        public override string ToString()
        {
            return $"0x{Number.ToString("X8")}";
        }
    }

    public abstract class TeuchiUdonAssembly
    {
        public TeuchiUdonAssemblyInstruction Instruction { get; }

        public TeuchiUdonAssembly(TeuchiUdonAssemblyInstruction instruction)
        {
            Instruction = instruction;
        }
    }

    public class Assembly_NO_CODE : TeuchiUdonAssembly
    {
        public Assembly_NO_CODE()
            : base(TeuchiUdonAssemblyInstruction.NO_CODE)
        {
        }
    }

    public class Assembly_NEW_LINE : TeuchiUdonAssembly
    {
        public Assembly_NEW_LINE()
            : base(TeuchiUdonAssemblyInstruction.NEW_LINE)
        {
        }
    }

    public class Assembly_INDENT : TeuchiUdonAssembly
    {
        public int Level { get; }

        public Assembly_INDENT(int level)
            : base(TeuchiUdonAssemblyInstruction.INDENT)
        {
            Level = level;
        }
    }

    public class Assembly_COMMENT : TeuchiUdonAssembly
    {
        public string Text { get; }

        public Assembly_COMMENT(string text)
            : base(TeuchiUdonAssemblyInstruction.COMMENT)
        {
            Text = text;
        }
        
        public override string ToString()
        {
            return $"# {Text}";
        }
    }

    public class Assembly_NOP : TeuchiUdonAssembly
    {
        public Assembly_NOP()
            : base(TeuchiUdonAssemblyInstruction.NOP)
        {
        }
        
        public override string ToString()
        {
            return $"NOP";
        }
    }

    public class Assembly_PUSH : TeuchiUdonAssembly
    {
        public TeuchiUdonAssemblyAddress Address { get; }

        public Assembly_PUSH(TeuchiUdonAssemblyAddress address)
            : base(TeuchiUdonAssemblyInstruction.PUSH)
        {
            Address = address;
        }

        public override string ToString()
        {
            return $"PUSH, {Address}";
        }
    }

    public class Assembly_POP : TeuchiUdonAssembly
    {
        public Assembly_POP()
            : base(TeuchiUdonAssemblyInstruction.POP)
        {
        }

        public override string ToString()
        {
            return $"POP";
        }
    }

    public class Assembly_JUMP_IF_FALSE : TeuchiUdonAssembly
    {
        public TeuchiUdonAssemblyAddress Address { get; }

        public Assembly_JUMP_IF_FALSE(TeuchiUdonAssemblyAddress address)
            : base(TeuchiUdonAssemblyInstruction.JUMP_IF_FALSE)
        {
            Address = address;
        }

        public override string ToString()
        {
            return $"JUMP_IF_FALSE, {Address}";
        }
    }

    public class Assembly_JUMP : TeuchiUdonAssembly
    {
        public TeuchiUdonAssemblyAddress Address { get; }

        public Assembly_JUMP(TeuchiUdonAssemblyAddress address)
            : base(TeuchiUdonAssemblyInstruction.JUMP)
        {
            Address = address;
        }

        public override string ToString()
        {
            return $"JUMP, {Address}";
        }
    }

    public class Assembly_EXTERN : TeuchiUdonAssembly
    {
        public TeuchiUdonMethod Method { get; }

        public Assembly_EXTERN(TeuchiUdonMethod method)
            : base(TeuchiUdonAssemblyInstruction.EXTERN)
        {
            Method = method;
        }

        public override string ToString()
        {
            return $"EXTERN, \"{Method.UdonName}\"";
        }
    }

    public class Assembly_ANNOTATION : TeuchiUdonAssembly
    {
        public Assembly_ANNOTATION()
            : base(TeuchiUdonAssemblyInstruction.ANNOTATION)
        {
        }

        public override string ToString()
        {
            return $"ANNOTATION";
        }
    }

    public class Assembly_JUMP_INDIRECT : TeuchiUdonAssembly
    {
        public TeuchiUdonAssemblyAddress Address { get; }

        public Assembly_JUMP_INDIRECT(TeuchiUdonAssemblyAddress address)
            : base(TeuchiUdonAssemblyInstruction.JUMP_INDIRECT)
        {
            Address = address;
        }

        public override string ToString()
        {
            return $"JUMP_INDIRECT, {Address}";
        }
    }

    public class Assembly_COPY : TeuchiUdonAssembly
    {
        public Assembly_COPY()
            : base(TeuchiUdonAssemblyInstruction.COPY)
        {
        }

        public override string ToString()
        {
            return $"COPY";
        }
    }

    public class Assembly_DATA_START : TeuchiUdonAssembly
    {
        public Assembly_DATA_START()
            : base(TeuchiUdonAssemblyInstruction.DATA_START)
        {
        }

        public override string ToString()
        {
            return $".data_start";
        }
    }

    public class Assembly_DATA_END : TeuchiUdonAssembly
    {
        public Assembly_DATA_END()
            : base(TeuchiUdonAssemblyInstruction.DATA_END)
        {
        }

        public override string ToString()
        {
            return $".data_end";
        }
    }

    public class Assembly_CODE_START : TeuchiUdonAssembly
    {
        public Assembly_CODE_START()
            : base(TeuchiUdonAssemblyInstruction.CODE_START)
        {
        }

        public override string ToString()
        {
            return $".code_start";
        }
    }

    public class Assembly_CODE_END : TeuchiUdonAssembly
    {
        public Assembly_CODE_END()
            : base(TeuchiUdonAssemblyInstruction.CODE_END)
        {
        }

        public override string ToString()
        {
            return $".code_end";
        }
    }

    public class Assembly_EXPORT_DATA : TeuchiUdonAssembly
    {
        public string Data { get; }

        public Assembly_EXPORT_DATA(string data)
            : base(TeuchiUdonAssemblyInstruction.EXPORT_DATA)
        {
            Data = data;
        }

        public override string ToString()
        {
            return $".export {Data}";
        }
    }

    public class Assembly_SYNC_DATA : TeuchiUdonAssembly
    {
        public string Data { get; }
        public TeuchiUdonAssemblySyncMode SyncMode { get; }

        public Assembly_SYNC_DATA(string data, TeuchiUdonAssemblySyncMode syncMode)
            : base(TeuchiUdonAssemblyInstruction.SYNC_DATA)
        {
            Data     = data;
            SyncMode = syncMode;
        }

        public override string ToString()
        {
            return $".sync {Data}, {SyncMode}";
        }
    }

    public class Assembly_DECL_DATA : TeuchiUdonAssembly
    {
        public string Data { get; }
        public TeuchiUdonType Type { get; }
        public TeuchiUdonAssemblyLiteral Literal { get; }

        public Assembly_DECL_DATA(string data, TeuchiUdonType type, TeuchiUdonAssemblyLiteral literal)
            : base(TeuchiUdonAssemblyInstruction.DECL_DATA)
        {
            Data    = data;
            Type    = type;
            Literal = literal;
        }

        public override string ToString()
        {
            return $"{Data}: %{Type.UdonName}, {Literal}";
        }
    }

    public class Assembly_EXPORT_CODE : TeuchiUdonAssembly
    {
        public string Code { get; }

        public Assembly_EXPORT_CODE(string code)
            : base(TeuchiUdonAssemblyInstruction.EXPORT_CODE)
        {
            Code = code;
        }

        public override string ToString()
        {
            return $".export {Code}";
        }
    }

    public class Assembly_LABEL : TeuchiUdonAssembly
    {
        public string Label { get; }

        public Assembly_LABEL(string label)
            : base(TeuchiUdonAssemblyInstruction.LABEL)
        {
            Label = label;
        }

        public override string ToString()
        {
            return $"{Label}:";
        }
    }
}
