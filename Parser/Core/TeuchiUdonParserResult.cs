using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace akanevrc.TeuchiUdon
{
    public abstract class TeuchiUdonParserResult
    {
        public IToken Start { get; }
        public IToken Stop { get; }
        public bool Valid { get; }

        public TeuchiUdonParserResult(IToken start, IToken stop, bool valid)
        {
            Start = start;
            Stop  = stop;
            Valid = valid;
        }
    }

    public class TargetResult : TeuchiUdonParserResult
    {
        public BodyResult Body { get; }

        public TargetResult(IToken start, IToken stop)
            : base(start, stop, false)
        {
        }

        public TargetResult(IToken start, IToken stop, BodyResult body)
            : base(start, stop, true)
        {
            Body = body;
        }
    }

    public class BodyResult : TeuchiUdonParserResult
    {
        public TopStatementResult[] TopStatements { get; }

        public BodyResult(IToken start, IToken stop)
            : base(start, stop, false)
        {
        }

        public BodyResult(IToken start, IToken stop, IEnumerable<TopStatementResult> topStatements)
            : base(start, stop, true)
        {
            TopStatements = topStatements.ToArray();
        }
    }

    public abstract class TopStatementResult : TeuchiUdonParserResult
    {
        public TopStatementResult(IToken start, IToken stop, bool valid)
            : base(start, stop, valid)
        {
        }
    }

    public class TopBindResult : TopStatementResult
    {
        public VarBindResult VarBind { get; }
        public bool Public { get; }
        public TeuchiUdonSyncMode Sync { get; }

        public TopBindResult(IToken start, IToken stop)
            : base(start, stop, false)
        {
        }

        public TopBindResult(IToken start, IToken stop, VarBindResult varBind, bool pub, TeuchiUdonSyncMode sync)
            : base(start, stop, true)
        {
            VarBind = varBind;
            Public  = pub;
            Sync    = sync;
        }
    }

    public class TopExprResult : TopStatementResult
    {
        public ExprResult Expr { get; }

        public TopExprResult(IToken start, IToken stop)
            : base(start, stop, false)
        {
        }

        public TopExprResult(IToken start, IToken stop, ExprResult expr)
            : base(start, stop, true)
        {
            Expr = expr;
        }
    }

    public abstract class VarAttrResult : TeuchiUdonParserResult
    {
        public VarAttrResult(IToken start, IToken stop, bool valid)
            : base(start, stop, valid)
        {
        }
    }

    public class PublicVarAttrResult : VarAttrResult
    {
        public PublicVarAttrResult(IToken start, IToken stop)
            : base(start, stop, true)
        {
        }
    }

    public class SyncVarAttrResult : VarAttrResult
    {
        public TeuchiUdonSyncMode Mode { get; }

        public SyncVarAttrResult(IToken start, IToken stop)
            : base(start, stop, false)
        {
        }

        public SyncVarAttrResult(IToken start, IToken stop, TeuchiUdonSyncMode mode)
            : base(start, stop, true)
        {
            Mode = mode;
        }
    }

    public abstract class ExprAttrResult : TeuchiUdonParserResult
    {
        public ExprAttrResult(IToken start, IToken stop, bool valid)
            : base(start, stop, valid)
        {
        }
    }

    public class VarBindResult : TeuchiUdonParserResult
    {
        public TeuchiUdonVarBind VarBind { get; }
        public TeuchiUdonVar[] Vars { get; }
        public VarDeclResult VarDecl { get; }
        public ExprResult Expr { get; }

        public VarBindResult(IToken start, IToken stop)
            : base(start, stop, false)
        {
        }

        public VarBindResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonVarBind varBind,
            IEnumerable<TeuchiUdonVar> vars,
            VarDeclResult varDecl,
            ExprResult expr
        ) : base(start, stop, true)
        {
            VarBind = varBind;
            Vars    = vars.ToArray();
            VarDecl = varDecl;
            Expr    = expr;
        }
    }

    public class VarDeclResult : TeuchiUdonParserResult
    {
        public TeuchiUdonVar[] Vars { get; }
        public TeuchiUdonType[] Types { get; }
        public QualifiedVarResult[] QualifiedVars { get; }

        public VarDeclResult(IToken start, IToken stop)
            : base(start, stop, false)
        {
        }

        public VarDeclResult
        (
            IToken start,
            IToken stop,
            IEnumerable<TeuchiUdonVar> vars,
            IEnumerable<TeuchiUdonType> types,
            IEnumerable<QualifiedVarResult> qualifiedVars
        ) : base(start, stop, true)
        {
            Vars          = vars         .ToArray();
            Types         = types        .ToArray();
            QualifiedVars = qualifiedVars.ToArray();
        }
    }

    public class QualifiedVarResult : TeuchiUdonParserResult
    {
        public IdentifierResult Identifier { get; }
        public ExprResult Qualified { get; }

        public QualifiedVarResult(IToken start, IToken stop)
            : base(start, stop, false)
        {
        }

        public QualifiedVarResult(IToken start, IToken stop, IdentifierResult identifier, ExprResult qualified)
            : base(start, stop, true)
        {
            Identifier = identifier;
            Qualified  = qualified;
        }
    }

    public class IdentifierResult : TeuchiUdonParserResult
    {
        public string Name { get; }

        public IdentifierResult(IToken start, IToken stop)
            : base(start, stop, false)
        {
        }

        public IdentifierResult(IToken start, IToken stop, string name)
            : base(start, stop, true)
        {
            Name = name;
        }
    }

    public abstract class StatementResult : TeuchiUdonParserResult
    {
        public StatementResult(IToken start, IToken stop, bool valid)
            : base(start, stop, valid)
        {
        }
    }

    public class JumpResult : StatementResult
    {
        public ExprResult Value { get; }
        public Func<TeuchiUdonBlock> Block { get; }
        public Func<ITeuchiUdonLabel> Label { get; }

        public JumpResult(IToken start, IToken stop)
            : base(start, stop, false)
        {
        }

        public JumpResult(IToken start, IToken stop, ExprResult value, Func<TeuchiUdonBlock> block, Func<ITeuchiUdonLabel> label)
            : base(start, stop, true)
        {
            Value = value;
            Block = block;
            Label = label;
        }
    }

    public class LetBindResult : StatementResult
    {
        public VarBindResult VarBind { get; }

        public LetBindResult(IToken start, IToken stop)
            : base(start, stop, false)
        {
        }

        public LetBindResult(IToken start, IToken stop, VarBindResult varBind)
            : base(start, stop, true)
        {
            VarBind = varBind;
        }
    }

    public class ExprResult : StatementResult
    {
        public TypedResult Inner { get; }
        public bool ReturnsValue { get; set; } = true;

        public ExprResult(IToken start, IToken stop)
            : base(start, stop, false)
        {
        }

        public ExprResult(IToken start, IToken stop, TypedResult inner)
            : base(start, stop, true)
        {
            Inner = inner;
        }
    }

    public abstract class TypedResult : TeuchiUdonParserResult
    {
        public TeuchiUdonType Type { get; set; }

        public TypedResult(IToken start, IToken stop, bool valid, TeuchiUdonType type, bool typeBound)
            : base(start, stop, valid)
        {
            Type      = type;
            TypeBound = typeBound;
        }

        public ExprResult Instance { get; set; } = null;
        public abstract ITeuchiUdonLeftValue[] LeftValues { get; }
        public abstract IEnumerable<TypedResult> Children { get; }
        public abstract IEnumerable<TypedResult> ReleasedChildren { get; }
        public abstract IEnumerable<TeuchiUdonOutValue> ReleasedOutValues { get; }
        
        public bool TypeBound { get; set; }
    }

    public abstract class ExternResult : TypedResult
    {
        public TeuchiUdonQualifier Qualifier { get; }
        public Dictionary<string, TeuchiUdonMethod> Methods { get; set; }
        public Dictionary<string, TeuchiUdonOutValue[]> OutValuess { get; set; }
        public Dictionary<string, TeuchiUdonOutValue> TmpValues { get; set; }
        public Dictionary<string, TeuchiUdonLiteral> Literals { get; set; }
        public Dictionary<string, ICodeLabel> Labels { get; set; }

        public ExternResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public ExternResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonQualifier qualifier)
            : base(start, stop, true, type, typeBound)
        {
            Qualifier = qualifier;
        }

        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => OutValuess.Values.SelectMany(x => x).Concat(TmpValues.Values);
    }

    public class InvalidResult : TypedResult
    {
        public InvalidResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound)
            : base(start, stop, false, type, typeBound)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class UnknownTypeResult : TypedResult
    {
        public UnknownTypeResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound)
            : base(start, stop, true, type, typeBound)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class UnitResult : TypedResult
    {
        public UnitResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound)
            : base(start, stop, true, type, typeBound)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class BlockResult : TypedResult
    {
        public TeuchiUdonBlock Block { get; }
        public StatementResult[] Statements { get; }
        public ExprResult Expr { get; }

        public BlockResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public BlockResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonBlock block,
            IEnumerable<StatementResult> statements,
            ExprResult expr
        ) : base(start, stop, true, type, typeBound)
        {
            Block      = block;
            Statements = statements.ToArray();
            Expr       = expr;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children =>
            new TypedResult[] { Expr.Inner }
            .Concat
            (
                Statements
                .Where(x => x is LetBindResult)
                .Select(x => ((LetBindResult)x).VarBind.Expr.Inner)
            )
            .Concat
            (
                Statements
                .Where(x => x is ExprResult)
                .Select(x => ((ExprResult)x).Inner)
            );
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Expr.Inner.ReleasedOutValues;
    }

    public class ParenResult : TypedResult
    {
        public ExprResult Expr { get; }

        public ParenResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public ParenResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, ExprResult expr)
            : base(start, stop, true, type, typeBound)
        {
            Expr = expr;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Expr.Inner.LeftValues;
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Expr.Inner.ReleasedOutValues;
    }

    public class TupleResult : TypedResult
    {
        public ExprResult[] Exprs { get; }

        public TupleResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public TupleResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, IEnumerable<ExprResult> exprs)
            : base(start, stop, true, type, typeBound)
        {
            Exprs = exprs.ToArray();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Exprs.SelectMany(x => x.Inner.LeftValues).ToArray();
        public override IEnumerable<TypedResult> Children => Exprs.Select(x => x.Inner);
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Exprs.SelectMany(x => x.Inner.ReleasedOutValues);
    }

    public class ArrayCtorResult : ExternResult
    {
        public IterExprResult[] Iters { get; }

        public ArrayCtorResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public ArrayCtorResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonQualifier qualifier, IEnumerable<IterExprResult> iters)
            : base(start, stop, type, typeBound, qualifier)
        {
            Iters = iters.ToArray();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Iters;
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => base.ReleasedOutValues.Concat(Children.SelectMany(x => x.ReleasedOutValues));
    }

    public class LiteralResult : TypedResult
    {
        public TeuchiUdonLiteral Literal { get; }

        public LiteralResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public LiteralResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonLiteral literal)
            : base(start, stop, true, type, typeBound)
        {
            Literal = literal;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class ThisResult : TypedResult
    {
        public TeuchiUdonThis This { get; }

        public ThisResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonThis this_)
            : base(start, stop, true, type, typeBound)
        {
            This = this_;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class InterpolatedStringResult : ExternResult
    {
        public TeuchiUdonLiteral StringLiteral { get; }
        public ExprResult[] Exprs { get; }

        public InterpolatedStringResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public InterpolatedStringResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonLiteral stringLiteral,
            IEnumerable<ExprResult> exprs
        ) : base(start, stop, type, typeBound, qualifier)
        {
            StringLiteral = stringLiteral;
            Exprs         = exprs.ToArray();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Exprs.Select(x => x.Inner);
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => base.ReleasedOutValues.Concat(Children.SelectMany(x => x.ReleasedOutValues));
    }

    public abstract class InterpolatedStringPartResult : TypedResult
    {
        public InterpolatedStringPartResult(IToken start, IToken stop, bool valid, TeuchiUdonType type, bool typeBound)
            : base(start, stop, valid, type, typeBound)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class RegularStringInterpolatedStringPartResult : InterpolatedStringPartResult
    {
        public string RawString { get; }

        public RegularStringInterpolatedStringPartResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public RegularStringInterpolatedStringPartResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, string rawString)
            : base(start, stop, true, type, typeBound)
        {
            RawString = rawString;
        }

        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class ExprInterpolatedStringPartResult : InterpolatedStringPartResult
    {
        public ExprResult Expr { get; }

        public ExprInterpolatedStringPartResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public ExprInterpolatedStringPartResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, ExprResult expr)
            : base(start, stop, true, type, typeBound)
        {
            Expr = expr;
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
    }

    public class EvalVarResult : TypedResult
    {
        public TeuchiUdonVar Var { get; }

        public EvalVarResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public EvalVarResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonVar v)
            : base(start, stop, true, type, typeBound)
        {
            Var = v;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Var.Mut ? new ITeuchiUdonLeftValue[] { Var } : Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class EvalTypeResult : TypedResult
    {
        public TeuchiUdonType InnerType { get; }

        public EvalTypeResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public EvalTypeResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonType innerType)
            : base(start, stop, true, type, typeBound)
        {
            InnerType = innerType;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class EvalQualifierResult : TypedResult
    {
        public TeuchiUdonQualifier Qualifier { get; }

        public EvalQualifierResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public EvalQualifierResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonQualifier qualifier)
            : base(start, stop, true, type, typeBound)
        {
            Qualifier = qualifier;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class EvalGetterResult : ExternResult
    {
        public TeuchiUdonMethod Getter { get; }

        public EvalGetterResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public EvalGetterResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonQualifier qualifier, TeuchiUdonMethod getter)
            : base(start, stop, type, typeBound, qualifier)
        {
            Getter = getter;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class EvalSetterResult : ExternResult
    {
        public TeuchiUdonMethod Setter { get; }

        public EvalSetterResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public EvalSetterResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonQualifier qualifier, TeuchiUdonMethod setter)
            : base(start, stop, type, typeBound, qualifier)
        {
            Setter = setter;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => new ITeuchiUdonLeftValue[] { Setter };
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class EvalGetterSetterResult : ExternResult
    {
        public TeuchiUdonMethod Getter { get; }
        public TeuchiUdonMethod Setter { get; }

        public EvalGetterSetterResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public EvalGetterSetterResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod getter,
            TeuchiUdonMethod setter
        ) : base(start, stop, type, typeBound, qualifier)
        {
            Getter = getter;
            Setter = setter;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => new ITeuchiUdonLeftValue[] { Setter };
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class EvalFuncResult : TypedResult
    {
        public TeuchiUdonEvalFunc EvalFunc { get; }
        public TeuchiUdonOutValue OutValue { get; }
        public ExprResult Expr { get; }
        public ExprResult[] Args { get; }

        public EvalFuncResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public EvalFuncResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonEvalFunc evalFunc,
            TeuchiUdonOutValue outValue,
            ExprResult expr,
            IEnumerable<ExprResult> args
        ) : base(start, stop, true, type, typeBound)
        {
            EvalFunc = evalFunc;
            OutValue = outValue;
            Expr     = expr;
            Args     = args.ToArray();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner }.Concat(Args.Select(x => x.Inner));
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => new TeuchiUdonOutValue[] { OutValue };
    }

    public class EvalSpreadFuncResult : TypedResult
    {
        public TeuchiUdonEvalFunc EvalFunc { get; }
        public TeuchiUdonOutValue OutValue { get; }
        public ExprResult Expr { get; }
        public ExprResult Arg { get; }

        public EvalSpreadFuncResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public EvalSpreadFuncResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonEvalFunc evalFunc,
            TeuchiUdonOutValue outValue,
            ExprResult expr,
            ExprResult arg
        ) : base(start, stop, true, type, typeBound)
        {
            EvalFunc = evalFunc;
            OutValue = outValue;
            Expr     = expr;
            Arg      = arg;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner, Arg.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => new TeuchiUdonOutValue[] { OutValue };
    }

    public class EvalMethodResult : ExternResult
    {
        public ExprResult Expr { get; }
        public ExprResult[] Args { get; }
        public TeuchiUdonMethod Method { get; }

        public EvalMethodResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public EvalMethodResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod method,
            ExprResult expr,
            IEnumerable<ExprResult> args
        ) : base(start, stop, type, typeBound, qualifier)
        {
            Method = method;
            Expr   = expr;
            Args   = args.ToArray();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner }.Concat(Args.Select(x => x.Inner));
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class EvalSpreadMethodResult : ExternResult
    {
        public ExprResult Expr { get; }
        public ExprResult Arg { get; }
        public TeuchiUdonMethod Method { get; }

        public EvalSpreadMethodResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public EvalSpreadMethodResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod method,
            ExprResult expr,
            ExprResult arg
        ) : base(start, stop, type, typeBound, qualifier)
        {
            Method = method;
            Expr   = expr;
            Arg    = arg;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner, Arg.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class EvalCoalescingMethodResult : ExternResult
    {
        public ExprResult Expr1 { get; }
        public ExprResult Expr2 { get; }
        public ExprResult[] Args { get; }
        public TeuchiUdonMethod Method { get; }

        public EvalCoalescingMethodResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public EvalCoalescingMethodResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod method,
            ExprResult expr1,
            ExprResult expr2,
            IEnumerable<ExprResult> args
        ) : base(start, stop, type, typeBound, qualifier)
        {
            Method = method;
            Expr1  = expr1;
            Expr2  = expr2;
            Args   = args.ToArray();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr1.Inner, Expr2.Inner }.Concat(Args.Select(x => x.Inner));
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class EvalCoalescingSpreadMethodResult : ExternResult
    {
        public ExprResult Expr1 { get; }
        public ExprResult Expr2 { get; }
        public ExprResult Arg { get; }
        public TeuchiUdonMethod Method { get; }

        public EvalCoalescingSpreadMethodResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public EvalCoalescingSpreadMethodResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod method,
            ExprResult expr1,
            ExprResult expr2,
            ExprResult arg
        ) : base(start, stop, type, typeBound, qualifier)
        {
            Method = method;
            Expr1  = expr1;
            Expr2  = expr2;
            Arg    = arg;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr1.Inner, Expr2.Inner, Arg.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class EvalCastResult : TypedResult
    {
        public ExprResult Expr { get; }

        public EvalCastResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public EvalCastResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, ExprResult expr)
            : base(start, stop, true, type, typeBound)
        {
            Expr = expr;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class EvalTypeOfResult : TypedResult
    {
        public EvalTypeOfResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound)
            : base(start, stop, true, type, typeBound)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class EvalVarCandidateResult : TypedResult
    {
        public IdentifierResult Identifier { get; }

        public EvalVarCandidateResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public EvalVarCandidateResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, IdentifierResult identifier)
            : base(start, stop, true, type, typeBound)
        {
            Identifier = identifier;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class EvalArrayIndexerResult : ExternResult
    {
        public ExprResult Expr { get; }
        public ExprResult Arg { get; }

        public EvalArrayIndexerResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public EvalArrayIndexerResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            ExprResult expr,
            ExprResult arg
        ) : base(start, stop, type, typeBound, qualifier)
        {
            Expr = expr;
            Arg  = arg;
        }

        public override ITeuchiUdonLeftValue[] LeftValues =>
            new ITeuchiUdonLeftValue[] { new TeuchiUdonArraySetter(Expr, Arg, Methods["setter"]) };
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner, Arg.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class TeuchiUdonArraySetter : ITeuchiUdonLeftValue
    {
        public ExprResult Expr { get; }
        public ExprResult Arg { get; }
        public TeuchiUdonMethod Method { get; }

        public TeuchiUdonArraySetter(ExprResult expr, ExprResult arg, TeuchiUdonMethod method)
        {
            Expr   = expr;
            Arg    = arg;
            Method = method;
        }
    }

    public class TypeCastResult : TypedResult
    {
        public ExprResult Expr { get; }
        public ExprResult Arg { get; }

        public TypeCastResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public TypeCastResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, ExprResult expr, ExprResult arg)
            : base(start, stop, true, type, typeBound)
        {
            Expr = expr;
            Arg  = arg;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner, Arg.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class ConvertCastResult : ExternResult
    {
        public ExprResult Expr { get; }
        public ExprResult Arg { get; }

        public ConvertCastResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public ConvertCastResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonQualifier qualifier, ExprResult expr, ExprResult arg)
            : base(start, stop, type, typeBound, qualifier)
        {
            Expr = expr;
            Arg  = arg;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner, Arg.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class TypeOfResult : TypedResult
    {
        public TeuchiUdonLiteral Literal { get; }

        public TypeOfResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public TypeOfResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonLiteral literal)
            : base(start, stop, true, type, typeBound)
        {
            Literal = literal;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public abstract class OpResult : ExternResult
    {
        public string Op { get; }

        public OpResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public OpResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonQualifier qualifier, string op)
            : base(start, stop, type, typeBound, qualifier)
        {
            Op = op;
        }
    }

    public abstract class UnaryOpResult : OpResult
    {
        public ExprResult Expr { get; }

        public UnaryOpResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public UnaryOpResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonQualifier qualifier, string op, ExprResult expr)
            : base(start, stop, type, typeBound, qualifier, op)
        {
            Expr = expr;
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class PrefixResult : UnaryOpResult
    {
        public PrefixResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public PrefixResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonQualifier qualifier, string op, ExprResult expr)
            : base(start, stop, type, typeBound, qualifier, op, expr)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
    }

    public abstract class BinaryOpResult : OpResult
    {
        public ExprResult Expr1 { get; }
        public ExprResult Expr2 { get; }

        public BinaryOpResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public BinaryOpResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            string op,
            ExprResult expr1,
            ExprResult expr2
        ) : base(start, stop, type, typeBound, qualifier, op)
        {
            Expr1 = expr1;
            Expr2 = expr2;
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr1.Inner, Expr2.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class InfixResult : BinaryOpResult
    {
        public InfixResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public InfixResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            string op,
            ExprResult expr1,
            ExprResult expr2
        ) : base(start, stop, type, typeBound, qualifier, op, expr1, expr2)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Op == "." ? Expr2.Inner.LeftValues : Array.Empty<ITeuchiUdonLeftValue>();
    }

    public class LetInBindResult : TypedResult
    {
        public TeuchiUdonLetIn LetIn { get; }
        public VarBindResult VarBind { get; }
        public ExprResult Expr { get; }

        public LetInBindResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public LetInBindResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonLetIn letIn,
            VarBindResult varBind,
            ExprResult expr
        ) : base(start, stop, true, type, typeBound)
        {
            LetIn   = LetIn;
            VarBind = varBind;
            Expr    = expr;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { VarBind.Expr.Inner, Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Expr.Inner.ReleasedOutValues;
    }

    public class IfResult : TypedResult
    {
        public ExprResult[] Conditions { get; }
        public StatementResult[] Statements { get; }
        public ICodeLabel[] Labels { get; }

        public IfResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public IfResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            IEnumerable<ExprResult> conditions,
            IEnumerable<StatementResult> statements,
            IEnumerable<ICodeLabel> labels
        ) : base(start, stop, true, type, typeBound)
        {
            Conditions = conditions.ToArray();
            Statements = statements.ToArray();
            Labels     = labels    .ToArray();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children =>
            Conditions
            .Select(x => x.Inner)
            .Concat
            (
                Statements
                .Where(x => x is ExprResult)
                .Select(x => ((ExprResult)x).Inner)
            );
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class IfElseResult : TypedResult
    {
        public ExprResult[] Conditions { get; }
        public ExprResult[] ThenParts { get; }
        public ExprResult ElsePart { get; }
        public ICodeLabel[] Labels { get; }

        public IfElseResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public IfElseResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            IEnumerable<ExprResult> conditions,
            IEnumerable<ExprResult> thenParts,
            ExprResult elsePart,
            IEnumerable<ICodeLabel> labels
        ) : base(start, stop, true, type, typeBound)
        {
            Conditions = conditions.ToArray();
            ThenParts  = thenParts .ToArray();
            ElsePart   = elsePart;
            Labels     = labels.ToArray();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children =>
            Conditions
            .Select(x => x.Inner)
            .Concat(ThenParts.Select(x => x.Inner))
            .Concat(new TypedResult[] { ElsePart.Inner });
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Children.SelectMany(x => x.ReleasedOutValues);
    }

    public class WhileResult : TypedResult
    {
        public ExprResult Condition { get; }
        public ExprResult Expr { get; }
        public ICodeLabel[] Labels { get; }

        public WhileResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public WhileResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, ExprResult condition, ExprResult expr, IEnumerable<ICodeLabel> labels)
            : base(start, stop, true, type, typeBound)
        {
            Condition = condition;
            Expr      = expr;
            Labels    = labels.ToArray();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Condition.Inner, Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class ForResult : TypedResult
    {
        public TeuchiUdonFor For { get; }
        public ForBindResult[] ForBinds { get; }
        public ExprResult Expr { get; }
        public ICodeLabel ContinueLabel { get; }

        public ForResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public ForResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            int index,
            IEnumerable<ForBindResult> forBinds,
            ExprResult expr,
            ICodeLabel continueLabel
        ) : base(start, stop, true, type, typeBound)
        {
            For           = new TeuchiUdonFor(index);
            ForBinds      = forBinds.ToArray();
            Expr          = expr;
            ContinueLabel = continueLabel;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children =>
            ForBinds
            .SelectMany(x =>
                x is LetForBindResult    letBind ? new TypedResult[] { letBind.Iter } :
                x is AssignForBindResult assign  ? new TypedResult[] { assign.Expr.Inner, assign.Iter } :
                Enumerable.Empty<TypedResult>()
            )
            .Concat(new TypedResult[] { Expr.Inner });
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class LoopResult : TypedResult
    {
        public ExprResult Expr { get; }
        public ICodeLabel[] Labels { get; }

        public LoopResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public LoopResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, ExprResult expr, IEnumerable<ICodeLabel> labels)
            : base(start, stop, true, type, typeBound)
        {
            Expr   = expr;
            Labels = labels.ToArray();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class FuncResult : TypedResult
    {
        public TeuchiUdonFunc Func { get; }
        public VarDeclResult VarDecl { get; }
        public ExprResult Expr { get; }
        public bool IsDet { get; }

        public FuncResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public FuncResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonFunc func,
            VarDeclResult varDecl,
            ExprResult expr,
            bool deterministic
        ) : base(start, stop, true, type, typeBound)
        {
            Func    = func;
            VarDecl = varDecl;
            Expr    = expr;
            IsDet   = deterministic;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class MethodResult : TypedResult
    {
        public IdentifierResult Identifier { get; }

        public MethodResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, false, type, true)
        {
        }

        public MethodResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, IdentifierResult identifier)
            : base(start, stop, true, type, typeBound)
        {
            Identifier = identifier;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public abstract class IterExprResult : ExternResult
    {
        public abstract ICodeLabel BreakLabel { get; }

        public IterExprResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public IterExprResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonQualifier qualifier)
            : base(start, stop, type, typeBound, qualifier)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
    }

    public class ElementsIterExprResult : IterExprResult
    {
        public ExprResult[] Exprs { get; }
        public override ICodeLabel BreakLabel { get; } = null;

        public ElementsIterExprResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public ElementsIterExprResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            IEnumerable<ExprResult> exprs
        ) : base(start, stop, type, typeBound, qualifier)
        {
            Exprs = exprs.ToArray();
        }

        public override IEnumerable<TypedResult> Children => Exprs.Select(x => x.Inner);
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
    }

    public class RangeIterExprResult : IterExprResult
    {
        public ExprResult First { get; }
        public ExprResult Last { get; }
        public override ICodeLabel BreakLabel => Labels["loop2"];

        public RangeIterExprResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public RangeIterExprResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            ExprResult first,
            ExprResult last
        ) : base(start, stop, type, typeBound, qualifier)
        {
            First = first;
            Last  = last;
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { First.Inner, Last.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class SteppedRangeIterExprResult : IterExprResult
    {
        public ExprResult First { get; }
        public ExprResult Last { get; }
        public ExprResult Step { get; }
        public override ICodeLabel BreakLabel => Labels["loop2"];

        public SteppedRangeIterExprResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public SteppedRangeIterExprResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            ExprResult first,
            ExprResult last,
            ExprResult step
        ) : base(start, stop, type, typeBound, qualifier)
        {
            First = first;
            Last  = last;
            Step  = step;
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { First.Inner, Last.Inner, Step.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class SpreadIterExprResult : IterExprResult
    {
        public ExprResult Expr { get; }
        public override ICodeLabel BreakLabel => Labels["loop2"];

        public SpreadIterExprResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, type)
        {
        }

        public SpreadIterExprResult(IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonQualifier qualifier, ExprResult expr)
            : base(start, stop, type, typeBound, qualifier)
        {
            Expr = expr;
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class IsoExprResult : TeuchiUdonParserResult
    {
        public ExprResult Expr { get; }

        public IsoExprResult(IToken start, IToken stop)
            : base(start, stop, false)
        {
        }

        public IsoExprResult(IToken start, IToken stop, ExprResult expr)
            : base(start, stop, true)
        {
            Expr = expr;
        }
    }

    public class ArgExprResult : TeuchiUdonParserResult
    {
        public ExprResult Expr { get; }
        public bool Ref { get; }

        public ArgExprResult(IToken start, IToken stop)
            : base(start, stop, false)
        {
        }

        public ArgExprResult(IToken start, IToken stop, ExprResult expr, bool rf)
            : base(start, stop, true)
        {
            Expr = expr;
            Ref  = rf;
        }
    }

    public abstract class ForBindResult : TeuchiUdonParserResult
    {
        public ForBindResult(IToken start, IToken stop, bool valid)
            : base(start, stop, valid)
        {
        }
    }

    public class LetForBindResult : ForBindResult
    {
        public TeuchiUdonVarBind VarBind { get; }
        public TeuchiUdonVar[] Vars { get; }
        public VarDeclResult VarDecl { get; }
        public IterExprResult Iter { get; }

        public LetForBindResult(IToken start, IToken stop)
            : base(start, stop, false)
        {
        }

        public LetForBindResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonVarBind varBind,
            IEnumerable<TeuchiUdonVar> vars,
            VarDeclResult varDecl,
            IterExprResult iter
        ) : base(start, stop, true)
        {
            VarBind = varBind;
            Vars    = vars.ToArray();
            VarDecl = varDecl;
            Iter    = iter;
        }
    }

    public class AssignForBindResult : ForBindResult
    {
        public ExprResult Expr { get; }
        public IterExprResult Iter { get; }

        public AssignForBindResult(IToken start, IToken stop)
            : base(start, stop, false)
        {
        }

        public AssignForBindResult(IToken start, IToken stop, ExprResult expr, IterExprResult iter)
            : base(start, stop, true)
        {
            Expr = expr;
            Iter = iter;
        }
    }
}
