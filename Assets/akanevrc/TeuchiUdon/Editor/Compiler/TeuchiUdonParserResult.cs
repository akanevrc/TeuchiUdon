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
                    new Assembly_JUMP_INDIRECT(new AssemblyAddress_LABEL(Label))
                });
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
            throw new InvalidOperationException("bottom detected");
        }
    }

    public class UnitResult : TypedResult
    {
        public TeuchiUdonLiteral Literal { get; }

        public UnitResult(IToken token)
            : base(token, TeuchiUdonType.Unit)
        {
            var literal = new TeuchiUdonLiteral("0");

            if (TeuchiUdonTables.Instance.Literals.ContainsKey(literal))
            {
                Literal = TeuchiUdonTables.Instance.Literals[literal];
            }
            else
            {
                Literal = new TeuchiUdonLiteral(TeuchiUdonTables.Instance.GetLiteralIndex(), "0", Type, 0);
                TeuchiUdonTables.Instance.Literals.Add(Literal, Literal);
            }
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return new TeuchiUdonAssembly[]
            {
                new Assembly_PUSH(new AssemblyAddress_LABEL(Literal))
            };
        }
    }

    public class BlockResult : TypedResult
    {
        public TeuchiUdonBlock Block { get; }
        public StatementResult[] Statements { get; }

        public BlockResult(IToken token, TeuchiUdonType type, int index, IEnumerable<StatementResult> statements)
            : base(token, type)
        {
            Block      = new TeuchiUdonBlock(index);
            Statements = statements.ToArray();
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return Statements.SelectMany(x => x.GetAssemblyCodePart());
        }
    }

    public class LetInBindResult : TypedResult
    {
        public VarBindResult VarBind { get; }
        public ExprResult Expr { get; }

        public LetInBindResult(IToken token, TeuchiUdonType type, VarBindResult varBind, ExprResult expr)
            : base(token, type)
        {
            VarBind = varBind;
            Expr    = expr;
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return
                VarBind.Expr.GetAssemblyCodePart()
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_LABEL(VarBind.Var)),
                    new Assembly_COPY()
                })
                .Concat(Expr.GetAssemblyCodePart());
        }
    }

    public class ParensResult : TypedResult
    {
        public ExprResult Expr { get; }

        public ParensResult(IToken token, TeuchiUdonType type, ExprResult expr)
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
                new Assembly_PUSH(new AssemblyAddress_LABEL(Literal))
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
                new Assembly_PUSH(new AssemblyAddress_LABEL(Var))
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
            Func    = new TeuchiUdonFunc(index, qualifier, name, varDecl.Vars, expr);
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
            return
                new TeuchiUdonAssembly[]
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
