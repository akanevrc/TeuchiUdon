using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public abstract class TeuchiUdonParserResult
    {
        public IToken Token { get; }

        public TeuchiUdonParserResult(IToken token)
        {
            Token = token;
        }

        public virtual IEnumerable<TeuchiUdonAssembly> GetAssemblyDataPart()
        {
            return new TeuchiUdonAssembly[0];
        }

        public virtual IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return new TeuchiUdonAssembly[0];
        }
    }

    public class BodyResult : TeuchiUdonParserResult
    {
        public TopStatementResult[] TopStatements { get; }

        public BodyResult(IToken token, IEnumerable<TopStatementResult> topStatements)
            : base(token)
        {
            TopStatements = topStatements.ToArray();
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyDataPart()
        {
            return TopStatements.SelectMany(x => x.GetAssemblyDataPart());
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return TopStatements.SelectMany(x => x.GetAssemblyCodePart());
        }
    }

    public class TopStatementResult : TeuchiUdonParserResult
    {
        public TeuchiUdonParserResult Statement { get; }

        public TopStatementResult(IToken token, TeuchiUdonParserResult statement)
            : base(token)
        {
            Statement = statement;
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyDataPart()
        {
            return Statement.GetAssemblyDataPart();
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return Statement.GetAssemblyCodePart();
        }
    }

    public class VarBindResult : TeuchiUdonParserResult
    {
        public TeuchiUdonVar Var { get; }
        public VarDeclResult VarDecl { get; }
        public ExprResult Expr { get; }

        public VarBindResult(IToken token, VarDeclResult varDecl, TeuchiUdonType type, ExprResult expr)
            : base(token)
        {
            Var     = new TeuchiUdonVar(varDecl.Vars[0].Qualifier, varDecl.Vars[0].Name, type, expr);
            VarDecl = varDecl;
            Expr    = expr;

            TeuchiUdonTables.Instance.Vars[Var] = Var;
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyDataPart()
        {
            return VarDecl.GetAssemblyDataPart().Concat(Expr.GetAssemblyDataPart());
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return Expr.GetAssemblyCodePart();
        }
    }

    public class VarDeclResult : TeuchiUdonParserResult
    {
        public TeuchiUdonVar[] Vars { get; }
        public TeuchiUdonType[] Types { get; }
        public IdentifierResult[] Identifiers { get; }
        public QualifiedResult[] Qualifieds { get; }

        public VarDeclResult(IToken token, TeuchiUdonQualifier qualifier, IEnumerable<IdentifierResult> identifiers, IEnumerable<QualifiedResult> qualifieds)
            : base(token)
        {
            Types       = qualifieds .Select(x => x.Type).ToArray();
            Vars        = identifiers.Zip(Types, (i, t) => (i, t)).Select(x => new TeuchiUdonVar(qualifier, x.i.Name, x.t)).ToArray();
            Identifiers = identifiers.ToArray();
            Qualifieds  = qualifieds .ToArray();

            foreach (var v in Vars)
            {
                TeuchiUdonTables.Instance.Vars.Add(v, v);
            }
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyDataPart()
        {
            return Vars.SelectMany(x => x.Type.TypeNameEquals(TeuchiUdonType.Func) ?
            new TeuchiUdonAssembly[0] :
            new TeuchiUdonAssembly[]
            {
                new Assembly_EXPORT_DATA(x.GetUdonName()),
                new Assembly_DECL_DATA
                (
                    x.GetUdonName(),
                    x.Type,
                    x.Expr.Inner is LiteralResult literal ?
                        (TeuchiUdonAssemblyLiteral)new AssemblyLiteral_VALUE(literal.Literal.Text) :
                        (TeuchiUdonAssemblyLiteral)new AssemblyLiteral_NULL ()
                )
            });
        }
    }

    public class QualifiedResult : TeuchiUdonParserResult
    {
        public IdentifierResult[] Identifiers { get; }
        public TeuchiUdonType Type { get; }

        public QualifiedResult(IToken token, IEnumerable<IdentifierResult> identifiers, TeuchiUdonType type)
            : base(token)
        {
            Identifiers = identifiers.ToArray();
            Type        = type;
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
        public TypedResult Inner { get; }

        public ExprResult(IToken token, TypedResult inner)
            : base(token)
        {
            Inner = inner;
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyDataPart()
        {
            return Inner.GetAssemblyDataPart();
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return Inner.GetAssemblyCodePart();
        }
    }

    public abstract class TypedResult : TeuchiUdonParserResult
    {
        public TeuchiUdonType Type { get; }

        public TypedResult(IToken token, TeuchiUdonType type)
            : base(token)
        {
            Type = type;
        }
    }

    public class BottomResult : TypedResult
    {
        public BottomResult(IToken token)
            : base(token, TeuchiUdonType.Bottom)
        {
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyDataPart()
        {
            throw new InvalidOperationException("bottom detected");
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            throw new InvalidOperationException("bottom detected");
        }
    }

    public class ParensResult : TypedResult
    {
        public ExprResult Expr { get; }

        public ParensResult(IToken token, ExprResult expr)
            : base(token, expr.Inner.Type)
        {
            Expr = expr;
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyDataPart()
        {
            return Expr.GetAssemblyDataPart();
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return Expr.GetAssemblyCodePart();
        }
    }

    public class LiteralResult : TypedResult
    {
        public TeuchiUdonLiteral Literal { get; }

        public LiteralResult(IToken token, TeuchiUdonType type, int index, string text, object value)
            : base(token, type)
        {
            Literal = new TeuchiUdonLiteral(index, text, type, value);

            TeuchiUdonTables.Instance.Literals.Add(Literal, Literal);
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyDataPart()
        {
            return new TeuchiUdonAssembly[]
            {
                new Assembly_DECL_DATA(Literal.GetUdonName(), Type, new AssemblyLiteral_VALUE(Literal.Text))
            };
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return new TeuchiUdonAssembly[]
            {
                new Assembly_PUSH(new AssemblyAddress_LABEL(Literal.GetUdonName()))
            };
        }
    }

    public class EvalVarResult : TypedResult
    {
        public TeuchiUdonVar Var { get; }
        public IdentifierResult Identifier { get; }

        public EvalVarResult(IToken token, TeuchiUdonType type, TeuchiUdonVar v, IdentifierResult identifier)
            : base(token, type)
        {
            Var        = v;
            Identifier = identifier;
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return new TeuchiUdonAssembly[]
            {
                new Assembly_PUSH(new AssemblyAddress_LABEL(Var.GetUdonName()))
            };
        }
    }

    public class EvalTypeResult : TypedResult
    {
        public TeuchiUdonType InnerType { get; }
        public IdentifierResult Identifier { get; }

        public EvalTypeResult(IToken token, TeuchiUdonType type, TeuchiUdonType innerType, IdentifierResult identifier)
            : base(token, type)
        {
            InnerType  = innerType;
            Identifier = identifier;
        }
    }

    public class EvalQualifierResult : TypedResult
    {
        public TeuchiUdonQualifier Qualifier { get; }
        public IdentifierResult Identifier { get; }

        public EvalQualifierResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, IdentifierResult identifier)
            : base(token, type)
        {
            Qualifier  = qualifier;
            Identifier = identifier;
        }
    }

    public class EvalFuncResult : TypedResult
    {
        public IdentifierResult Identifier { get; }
        public ExprResult[] Args { get; }

        public EvalFuncResult(IToken token, TeuchiUdonType type, IdentifierResult identifier, IEnumerable<ExprResult> args)
            : base(token, type)
        {
            Identifier = identifier;
            Args       = args.ToArray();
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyDataPart()
        {
            return Args.SelectMany(x => x.GetAssemblyDataPart());
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            throw new NotImplementedException();
        }
    }

    public class EvalMethodResult : TypedResult
    {
        public TeuchiUdonMethod Method { get; }
        public IdentifierResult Identifier { get; }
        public ExprResult[] Args { get; }

        public EvalMethodResult(IToken token, TeuchiUdonType type, TeuchiUdonMethod method, IdentifierResult identifier, IEnumerable<ExprResult> args)
            : base(token, type)
        {
            Method     = method;
            Identifier = identifier;
            Args       = args.ToArray();
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyDataPart()
        {
            return Args.SelectMany(x => x.GetAssemblyDataPart());
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return Args.SelectMany(x => x.GetAssemblyCodePart()).Concat(new TeuchiUdonAssembly[]
            {
                new Assembly_EXTERN(Method)
            });
        }
    }

    public abstract class EvalCandidateResult : TypedResult
    {
        public IdentifierResult Identifier { get; }

        public EvalCandidateResult(IToken token, IdentifierResult identifier)
            : base(token, TeuchiUdonType.Bottom)
        {
            Identifier = identifier;
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyDataPart()
        {
            throw new InvalidOperationException("candidate detected");
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            throw new InvalidOperationException("candidate detected");
        }
    }

    public class EvalQualifierCandidateResult : EvalCandidateResult
    {
        public EvalQualifierCandidateResult(IToken token, IdentifierResult identifier)
            : base(token, identifier)
        {
        }
    }

    public class EvalMethodCandidateResult : EvalCandidateResult
    {
        public ExprResult[] Args { get; }

        public EvalMethodCandidateResult(IToken token, IdentifierResult identifier, IEnumerable<ExprResult> args)
            : base(token, identifier)
        {
            Args = args.ToArray();
        }
    }

    public class InfixResult : TypedResult
    {
        public string Op { get; }
        public ExprResult Expr1 { get; }
        public ExprResult Expr2 { get; }

        public InfixResult(IToken token, TeuchiUdonType type, string op, ExprResult expr1, ExprResult expr2)
            : base(token, type)
        {
            Op    = op;
            Expr1 = expr1;
            Expr2 = expr2;
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyDataPart()
        {
            return Expr1.GetAssemblyDataPart().Concat(Expr2.GetAssemblyDataPart());
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return Expr1.GetAssemblyCodePart().Concat(Expr2.GetAssemblyCodePart());
        }
    }

    public class FuncResult : TypedResult
    {
        public TeuchiUdonFunc Func { get; }
        public VarDeclResult VarDecl { get; }
        public ExprResult Expr { get; }

        public FuncResult(IToken token, TeuchiUdonType type, int index, TeuchiUdonQualifier qualifier, string name, VarDeclResult varDecl, ExprResult expr)
            : base(token, type)
        {
            Func    = new TeuchiUdonFunc(index, qualifier, name, varDecl.Vars, expr);
            VarDecl = varDecl;
            Expr    = expr;

            TeuchiUdonTables.Instance.Funcs.Add(index, this);
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyDataPart()
        {
            return VarDecl.GetAssemblyDataPart().Concat(Expr.GetAssemblyDataPart());
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return new TeuchiUdonAssembly[]
            {
                new Assembly_EXPORT_CODE(Func.Name),
                new Assembly_LABEL      (Func.Name),
                new Assembly_INDENT     (1)
            }
            .Concat(Expr.GetAssemblyCodePart())
            .Concat(new TeuchiUdonAssembly[] { new Assembly_INDENT(-1) });
        }
    }
}
