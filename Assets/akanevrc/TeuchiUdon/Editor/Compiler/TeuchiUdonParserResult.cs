using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonParserResult
    {
        public IToken Token { get; }

        public TeuchiUdonParserResult(IToken token)
        {
            Token = token;
        }
    }

    public class TopBindResult : TeuchiUdonParserResult
    {
        public TopBindResult(IToken token)
            : base(token)
        {
        }
    }

    public class VarBindResult : TeuchiUdonParserResult
    {
        public IdentifierResult Identifier { get; }
        public ExprResult Expr { get; }

        public VarBindResult(IToken token, IdentifierResult identifier, ExprResult expr, Dictionary<string, VarBindResult> dic)
            : base(token)
        {
            Identifier = identifier;
            Expr       = expr;

            dic.Add(identifier.Name, this);
        }
    }

    public class VarDeclResult : TeuchiUdonParserResult
    {
        public TupleDeclResult TupleDecl { get; }
        public SingleDeclResult SingleDecl { get; }

        public VarDeclResult(IToken token, TupleDeclResult tupleDecl)
            : base(token)
        {
            TupleDecl  = tupleDecl;
            SingleDecl = null;
        }

        public VarDeclResult(IToken token, SingleDeclResult singleDecl)
            : base(token)
        {
            TupleDecl  = null;
            SingleDecl = singleDecl;
        }
    }

    public class TupleDeclResult : TeuchiUdonParserResult
    {
        public VarDeclResult[] Decls { get; }

        public TupleDeclResult(IToken token, IEnumerable<VarDeclResult> decls)
            : base(token)
        {
            Decls = decls.ToArray();
        }
    }

    public class SingleDeclResult : TeuchiUdonParserResult
    {
        public IdentifierResult Identifier { get; }
        public QualifiedResult Type { get; }

        public SingleDeclResult(IToken token, IdentifierResult identifier, QualifiedResult type)
            : base(token)
        {
            Identifier = identifier;
            Type       = type;
        }
    }

    public class QualifiedResult : TeuchiUdonParserResult
    {
        public IdentifierResult[] Identifiers { get; }

        public QualifiedResult(IToken token, IEnumerable<IdentifierResult> identifiers)
            : base(token)
        {
            Identifiers = identifiers.ToArray();
        }
    }

    public class IdentifierResult : TeuchiUdonParserResult
    {
        public string Name { get; }

        public IdentifierResult(IToken token, string name)
            : base(token)
        {
            Name  = name;
        }
    }

    public class ExprResult : TeuchiUdonParserResult
    {
        public TeuchiUdonParserResult Inner { get; }

        public ExprResult(IToken token, TeuchiUdonParserResult inner)
            : base(token)
        {
            Inner = inner;
        }
    }

    public class ParensResult : TeuchiUdonParserResult
    {
        public ExprResult Expr { get; }

        public ParensResult(IToken token, ExprResult expr)
            : base(token)
        {
            Expr = expr;
        }
    }

    public class LiteralResult : TeuchiUdonParserResult
    {
        public uint Address { get; }
        public object Value { get; }
        public string Text { get; }

        public LiteralResult(IToken token, object value, string text, List<LiteralResult> list)
            : base(token)
        {
            Address = (uint)list.Count;
            Value   = value;
            Text    = text;

            list.Add(this);
        }
    }

    public class EvalVarResult : TeuchiUdonParserResult
    {
        public IdentifierResult Identifier { get; }

        public EvalVarResult(IToken token, IdentifierResult identifier)
            : base(token)
        {
            Identifier = identifier;
        }
    }

    public class EvalFuncResult : TeuchiUdonParserResult
    {
        public IdentifierResult Identifier { get; }
        public ExprResult[] Args { get; }

        public EvalFuncResult(IToken token, IdentifierResult identifier, IEnumerable<ExprResult> args)
            : base(token)
        {
            Identifier = identifier;
            Args       = args.ToArray();
        }
    }

    public class InfixResult : TeuchiUdonParserResult
    {
        public string Op { get; }
        public ExprResult Expr1 { get; }
        public ExprResult Expr2 { get; }

        public InfixResult(IToken token, string op, ExprResult expr1, ExprResult expr2)
            : base(token)
        {
            Op    = op;
            Expr1 = expr1;
            Expr2 = expr2;
        }
    }

    public class FuncResult : TeuchiUdonParserResult
    {
        public uint Address { get; }
        public VarDeclResult VarDecl { get; }
        public ExprResult Expr { get; }

        public FuncResult(IToken token, VarDeclResult varDecl, ExprResult expr, List<FuncResult> list)
            : base(token)
        {
            Address = (uint)list.Count;
            VarDecl = varDecl;
            Expr    = expr;

            list.Add(this);
        }
    }
}
