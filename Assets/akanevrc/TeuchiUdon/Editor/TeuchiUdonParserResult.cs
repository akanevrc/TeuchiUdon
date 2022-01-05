using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace akanevrc.TeuchiUdon.Editor
{
    public interface ITeuchiUdonParserResult
    {
    }

    public class TopBindResult : ITeuchiUdonParserResult
    {
    }

    public class VarBindResult : ITeuchiUdonParserResult
    {
        public IdentifierResult Identifier { get; }
        public ExprResult Expr { get; }

        public VarBindResult(IdentifierResult identifier, ExprResult expr, Dictionary<string, VarBindResult> dic)
        {
            Identifier = identifier;
            Expr       = expr;

            dic.Add(identifier.Identifier, this);
        }
    }

    public class VarDeclResult : ITeuchiUdonParserResult
    {
        public IdentifierResult[] Identifiers { get; }

        public VarDeclResult(IEnumerable<IdentifierResult> identifiers)
        {
            Identifiers = identifiers.ToArray();
        }
    }

    public class IdentifierResult : ITeuchiUdonParserResult
    {
        public IToken Token { get; }
        public string Identifier { get; }

        public IdentifierResult(IToken token, string identifier)
        {
            Token      = token;
            Identifier = identifier;
        }
    }

    public class ExprResult : ITeuchiUdonParserResult
    {
        public ITeuchiUdonParserResult Inner { get; }

        public ExprResult(ITeuchiUdonParserResult inner)
        {
            Inner = inner;
        }
    }

    public class EvalVarResult : ITeuchiUdonParserResult
    {
        public IdentifierResult Identifier { get; }

        public EvalVarResult(IdentifierResult identifier)
        {
            Identifier = identifier;
        }
    }

    public class EvalFuncResult : ITeuchiUdonParserResult
    {
        public IdentifierResult Identifier { get; }
        public ExprResult Expr { get; }

        public EvalFuncResult(IdentifierResult identifier, ExprResult expr)
        {
            Identifier = identifier;
            Expr       = expr;
        }
    }

    public class FuncResult : ITeuchiUdonParserResult
    {
        public uint Address { get; }
        public VarDeclResult VarDecl { get; }
        public ExprResult Expr { get; }

        public FuncResult(VarDeclResult varDecl, ExprResult expr, List<FuncResult> list)
        {
            Address = (uint)list.Count;
            VarDecl = varDecl;
            Expr    = expr;

            list.Add(this);
        }
    }

    public class LiteralResult : ITeuchiUdonParserResult
    {
        public uint Address { get; }
        public IToken Token { get; }
        public object Literal { get; }

        public LiteralResult(IToken token, object literal, List<LiteralResult> list)
        {
            Address = (uint)list.Count;
            Token   = token;
            Literal = literal;

            list.Add(this);
        }
    }
}
