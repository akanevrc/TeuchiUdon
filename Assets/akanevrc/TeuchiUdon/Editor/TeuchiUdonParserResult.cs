using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace akanevrc.TeuchiUdon.Editor
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
        public TopBindResult()
            : base(null)
        {
        }
    }

    public class VarBindResult : TeuchiUdonParserResult
    {
        public IdentifierResult Identifier { get; }
        public ExprResult Expr { get; }

        public VarBindResult(IdentifierResult identifier, ExprResult expr, Dictionary<string, VarBindResult> dic)
            : base(identifier.Token)
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

        public VarDeclResult(TupleDeclResult tupleDecl)
            : base(tupleDecl.Token)
        {
            TupleDecl  = tupleDecl;
            SingleDecl = null;
        }

        public VarDeclResult(SingleDeclResult singleDecl)
            : base(singleDecl.Token)
        {
            TupleDecl  = null;
            SingleDecl = singleDecl;
        }
    }

    public class TupleDeclResult : TeuchiUdonParserResult
    {
        public VarDeclResult[] Decls { get; }

        public TupleDeclResult(IEnumerable<VarDeclResult> decls)
            : base(decls.FirstOrDefault()?.Token)
        {
            Decls = decls.ToArray();
        }
    }

    public class SingleDeclResult : TeuchiUdonParserResult
    {
        public IdentifierResult Identifier { get; }
        public QualifiedResult Type { get; }

        public SingleDeclResult(IdentifierResult identifier, QualifiedResult type)
            : base(identifier.Token)
        {
            Identifier = identifier;
            Type       = type;
        }
    }

    public class QualifiedResult : TeuchiUdonParserResult
    {
        public IdentifierResult[] Identifiers { get; }

        public QualifiedResult(IEnumerable<IdentifierResult> identifiers)
            : base(identifiers.FirstOrDefault()?.Token)
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

        public ExprResult(TeuchiUdonParserResult inner)
            : base(inner.Token)
        {
            Inner = inner;
        }
    }

    public class LiteralResult : TeuchiUdonParserResult
    {
        public uint Address { get; }
        public object Value { get; }

        public LiteralResult(IToken token, object value, List<LiteralResult> list)
            : base(token)
        {
            Address = (uint)list.Count;
            Value   = value;

            list.Add(this);
        }
    }

    public class EvalVarResult : TeuchiUdonParserResult
    {
        public IdentifierResult Identifier { get; }

        public EvalVarResult(IdentifierResult identifier)
            : base(identifier.Token)
        {
            Identifier = identifier;
        }
    }

    public class EvalFuncResult : TeuchiUdonParserResult
    {
        public IdentifierResult Identifier { get; }
        public ExprResult[] Args { get; }

        public EvalFuncResult(IdentifierResult identifier, IEnumerable<ExprResult> args)
            : base(identifier.Token)
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

        public InfixResult(string op, ExprResult expr1, ExprResult expr2)
            : base(expr1.Token)
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

        public FuncResult(VarDeclResult varDecl, ExprResult expr, List<FuncResult> list)
            : base(varDecl.Token)
        {
            Address = (uint)list.Count;
            VarDecl = varDecl;
            Expr    = expr;

            list.Add(this);
        }
    }
}
