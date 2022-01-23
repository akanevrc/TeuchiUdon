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

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return Statement.GetAssemblyCodePart();
        }
    }

    public class VarBindResult : TeuchiUdonParserResult
    {
        public TeuchiUdonVar[] Vars { get; }
        public VarDeclResult VarDecl { get; }
        public ExprResult Expr { get; }

        public VarBindResult(IToken token, IEnumerable<TeuchiUdonVar> vars, VarDeclResult varDecl, ExprResult expr)
            : base(token)
        {
            Vars    = vars.ToArray();
            VarDecl = varDecl;
            Expr    = expr;

            foreach (var v in Vars)
            {
                TeuchiUdonTables.Instance.Vars[v] = v;
            }
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
                if (TeuchiUdonTables.Instance.Vars.ContainsKey(v))
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"{v.Name} conflicts with another variable");
                }
                else
                {
                    TeuchiUdonTables.Instance.Vars.Add(v, v);
                }
            }
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
            Name = name;
        }
    }

    public abstract class StatementResult : TeuchiUdonParserResult
    {
        public StatementResult(IToken token)
            : base(token)
        {
        }
    }

    public class JumpResult : StatementResult
    {
        public ExprResult Value { get; }
        public ITeuchiUdonLabel Label { get; }

        public JumpResult(IToken token, ExprResult value, ITeuchiUdonLabel label)
            : base(token)
        {
            Value = value;
            Label = label;
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return
                Value.GetAssemblyCodePart()
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_JUMP_INDIRECT(new AssemblyAddress_DATA_LABEL(Label))
                });
        }
    }

    public class LetBindResult : StatementResult
    {
        public VarBindResult VarBind { get; }

        public LetBindResult(IToken token, VarBindResult varBind)
            : base(token)
        {
            VarBind = varBind;
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return
                VarBind.Expr.GetAssemblyCodePart()
                .Concat(VarBind.Vars.SelectMany(x => new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)),
                    new Assembly_COPY()
                }));
        }
    }

    public class ExprResult : StatementResult
    {
        public TypedResult Inner { get; }

        public ExprResult(IToken token, TypedResult inner)
            : base(token)
        {
            Inner = inner;
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

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            TeuchiUdonLogicalErrorHandler.Instance.ReportError(Token, $"bottom detected");
            return new TeuchiUdonAssembly[0];
        }
    }

    public class UnitResult : TypedResult
    {
        public UnitResult(IToken token)
            : base(token, TeuchiUdonType.Unit)
        {
        }
    }

    public class BlockResult : TypedResult
    {
        public TeuchiUdonBlock Block { get; }
        public StatementResult[] Statements { get; }
        public ExprResult Expr { get; }

        public BlockResult(IToken token, TeuchiUdonType type, int index, TeuchiUdonQualifier qualifier, IEnumerable<StatementResult> statements, ExprResult expr)
            : base(token, type)
        {
            Block      = new TeuchiUdonBlock(index, qualifier);
            Statements = statements.ToArray();
            Expr       = expr;
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return Statements.SelectMany(x => x.GetAssemblyCodePart()).Concat(Expr.GetAssemblyCodePart());
        }
    }

    public class ParenResult : TypedResult
    {
        public ExprResult Expr { get; }

        public ParenResult(IToken token, TeuchiUdonType type, ExprResult expr)
            : base(token, type)
        {
            Expr = expr;
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

            if (TeuchiUdonTables.Instance.Literals.ContainsKey(Literal))
            {
                Literal = TeuchiUdonTables.Instance.Literals[Literal];
            }
            else
            {
                TeuchiUdonTables.Instance.Literals.Add(Literal, Literal);
            }
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return new TeuchiUdonAssembly[]
            {
                new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Literal))
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
                new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Var))
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
        public TeuchiUdonEvalFunc EvalFunc { get; }
        public TeuchiUdonVar Var { get; }
        public IdentifierResult Identifier { get; }
        public ExprResult[] Args { get; }

        public EvalFuncResult
        (
            IToken token,
            TeuchiUdonType type,
            int index,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonVar v,
            IdentifierResult identifier,
            IEnumerable<ExprResult> args
        ) : base(token, type)
        {
            EvalFunc   = new TeuchiUdonEvalFunc(index, qualifier);
            Var        = v;
            Identifier = identifier;
            Args       = args.ToArray();
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return
                Args.SelectMany(x => x.GetAssemblyCodePart())
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_INDIRECT_LABEL(EvalFunc)),
                    new Assembly_JUMP_INDIRECT(new AssemblyAddress_DATA_LABEL(Var)),
                    new Assembly_LABEL(EvalFunc)
                });
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
            : base(token, TeuchiUdonType.Unknown)
        {
            Identifier = identifier;
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
            Func    = new TeuchiUdonFunc(index, qualifier, name, type, varDecl.Vars, expr);
            VarDecl = varDecl;
            Expr    = expr;

            if (TeuchiUdonTables.Instance.Funcs.ContainsKey(Func))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"{Func.Name} conflicts with another function");
            }
            else
            {
                TeuchiUdonTables.Instance.Funcs.Add(Func, Func);
            }
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return new TeuchiUdonAssembly[]
            {
                new Assembly_PUSH(new AssemblyAddress_INDIRECT_LABEL(Func))
            };
        }
    }

    public class LetInBindResult : TypedResult
    {
        public TeuchiUdonLet Let { get; }
        public VarBindResult VarBind { get; }
        public ExprResult Expr { get; }

        public LetInBindResult(IToken token, TeuchiUdonType type, int index, TeuchiUdonQualifier qualifier, VarBindResult varBind, ExprResult expr)
            : base(token, type)
        {
            Let     = new TeuchiUdonLet(index, qualifier);
            VarBind = varBind;
            Expr    = expr;
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return
                VarBind.Expr.GetAssemblyCodePart()
                .Concat(VarBind.Vars.SelectMany(x => new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)),
                    new Assembly_COPY()
                }))
                .Concat(Expr.GetAssemblyCodePart());
        }
    }
}
