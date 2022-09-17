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

        public TeuchiUdonParserResult(TeuchiUdonTables tables, IToken start, IToken stop, bool valid)
        {
            Start = start;
            Stop  = stop;
            Valid = valid;

            tables.ParserResults.Add(this);
        }
    }

    public class TargetResult : TeuchiUdonParserResult
    {
        public BodyResult Body { get; }

        public TargetResult(TeuchiUdonTables tables, IToken start, IToken stop)
            : base(tables, start, stop, false)
        {
        }

        public TargetResult(TeuchiUdonTables tables, IToken start, IToken stop, BodyResult body)
            : base(tables, start, stop, true)
        {
            Body = body;
        }
    }

    public class BodyResult : TeuchiUdonParserResult
    {
        public TopStatementResult[] TopStatements { get; }

        public BodyResult(TeuchiUdonTables tables, IToken start, IToken stop)
            : base(tables, start, stop, false)
        {
        }

        public BodyResult(TeuchiUdonTables tables, IToken start, IToken stop, IEnumerable<TopStatementResult> topStatements)
            : base(tables, start, stop, true)
        {
            TopStatements = topStatements.ToArray();
        }
    }

    public abstract class TopStatementResult : TeuchiUdonParserResult
    {
        public TopStatementResult(TeuchiUdonTables tables, IToken start, IToken stop, bool valid)
            : base(tables, start, stop, valid)
        {
        }
    }

    public class TopBindResult : TopStatementResult
    {
        public VarBindResult VarBind { get; }
        public bool Public { get; }
        public TeuchiUdonSyncMode Sync { get; }

        public TopBindResult(TeuchiUdonTables tables, IToken start, IToken stop)
            : base(tables, start, stop, false)
        {
        }

        public TopBindResult(TeuchiUdonTables tables, IToken start, IToken stop, VarBindResult varBind, bool pub, TeuchiUdonSyncMode sync)
            : base(tables, start, stop, true)
        {
            VarBind = varBind;
            Public  = pub;
            Sync    = sync;
        }
    }

    public class TopExprResult : TopStatementResult
    {
        public ExprResult Expr { get; }

        public TopExprResult(TeuchiUdonTables tables, IToken start, IToken stop)
            : base(tables, start, stop, false)
        {
        }

        public TopExprResult(TeuchiUdonTables tables, IToken start, IToken stop, ExprResult expr)
            : base(tables, start, stop, true)
        {
            Expr = expr;
        }
    }

    public abstract class VarAttrResult : TeuchiUdonParserResult
    {
        public VarAttrResult(TeuchiUdonTables tables, IToken start, IToken stop, bool valid)
            : base(tables, start, stop, valid)
        {
        }
    }

    public class PublicVarAttrResult : VarAttrResult
    {
        public KeywordResult Keyword { get; }

        public PublicVarAttrResult(TeuchiUdonTables tables, IToken start, IToken stop)
            : base(tables, start, stop, false)
        {
        }

        public PublicVarAttrResult(TeuchiUdonTables tables, IToken start, IToken stop, KeywordResult keyword)
            : base(tables, start, stop, true)
        {
            Keyword = keyword;
        }
    }

    public class SyncVarAttrResult : VarAttrResult
    {
        public KeywordResult Keyword { get; }
        public TeuchiUdonSyncMode Mode { get; }

        public SyncVarAttrResult(TeuchiUdonTables tables, IToken start, IToken stop)
            : base(tables, start, stop, false)
        {
        }

        public SyncVarAttrResult(TeuchiUdonTables tables, IToken start, IToken stop, KeywordResult keyword, TeuchiUdonSyncMode mode)
            : base(tables, start, stop, true)
        {
            Keyword = keyword;
            Mode    = mode;
        }
    }

    public abstract class ExprAttrResult : TeuchiUdonParserResult
    {
        public ExprAttrResult(TeuchiUdonTables tables, IToken start, IToken stop, bool valid)
            : base(tables, start, stop, valid)
        {
        }
    }

    public class KeywordResult : TeuchiUdonParserResult
    {
        public string Name { get; }

        public KeywordResult(TeuchiUdonTables tables, IToken start, IToken stop)
            : base(tables, start, stop, false)
        {
        }

        public KeywordResult(TeuchiUdonTables tables, IToken start, IToken stop, string name)
            : base(tables, start, stop, true)
        {
            Name = name;
        }
    }

    public class VarBindResult : TeuchiUdonParserResult
    {
        public TeuchiUdonVarBind VarBind { get; }
        public TeuchiUdonVar[] Vars { get; }
        public VarDeclResult VarDecl { get; }
        public ExprResult Expr { get; }

        public VarBindResult(TeuchiUdonTables tables, IToken start, IToken stop)
            : base(tables, start, stop, false)
        {
        }

        public VarBindResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonVarBind varBind,
            IEnumerable<TeuchiUdonVar> vars,
            VarDeclResult varDecl,
            ExprResult expr
        ) : base(tables, start, stop, true)
        {
            VarBind    = varBind;
            Vars       = vars.ToArray();
            VarDecl    = varDecl;
            Expr       = expr;
        }
    }

    public class VarDeclResult : TeuchiUdonParserResult
    {
        public TeuchiUdonVar[] Vars { get; }
        public TeuchiUdonType[] Types { get; }
        public QualifiedVarResult[] QualifiedVars { get; }

        public VarDeclResult(TeuchiUdonTables tables, IToken start, IToken stop)
            : base(tables, start, stop, false)
        {
        }

        public VarDeclResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            IEnumerable<TeuchiUdonVar> vars,
            IEnumerable<TeuchiUdonType> types,
            IEnumerable<QualifiedVarResult> qualifiedVars
        ) : base(tables, start, stop, true)
        {
            Vars          = vars         .ToArray();
            Types         = types        .ToArray();
            QualifiedVars = qualifiedVars.ToArray();
        }
    }

    public class QualifiedVarResult : TeuchiUdonParserResult
    {
        public KeywordResult MutKeyword { get; }
        public IdentifierResult Identifier { get; }
        public ExprResult Qualified { get; }

        public QualifiedVarResult(TeuchiUdonTables tables, IToken start, IToken stop)
            : base(tables, start, stop, false)
        {
        }

        public QualifiedVarResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            KeywordResult mutKeyword,
            IdentifierResult identifier,
            ExprResult qualified
        ) : base(tables, start, stop, true)
        {
            MutKeyword = mutKeyword;
            Identifier = identifier;
            Qualified  = qualified;
        }
    }

    public class IdentifierResult : TeuchiUdonParserResult
    {
        public string Name { get; }

        public IdentifierResult(TeuchiUdonTables tables, IToken start, IToken stop)
            : base(tables, start, stop, false)
        {
        }

        public IdentifierResult(TeuchiUdonTables tables, IToken start, IToken stop, string name)
            : base(tables, start, stop, true)
        {
            Name = name;
        }
    }

    public abstract class StatementResult : TeuchiUdonParserResult
    {
        public StatementResult(TeuchiUdonTables tables, IToken start, IToken stop, bool valid)
            : base(tables, start, stop, valid)
        {
        }
    }

    public class JumpResult : StatementResult
    {
        public KeywordResult JumpKeyword { get; }
        public ExprResult Value { get; }
        public Func<TeuchiUdonBlock> Block { get; }
        public Func<ITeuchiUdonLabel> Label { get; }

        public JumpResult(TeuchiUdonTables tables, IToken start, IToken stop)
            : base(tables, start, stop, false)
        {
        }

        public JumpResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            KeywordResult jumpKeyword,
            ExprResult value,
            Func<TeuchiUdonBlock> block,
            Func<ITeuchiUdonLabel> label
        ) : base(tables, start, stop, true)
        {
            JumpKeyword = jumpKeyword;
            Value       = value;
            Block       = block;
            Label       = label;
        }
    }

    public class LetBindResult : StatementResult
    {
        public KeywordResult LetKeyword { get; }
        public VarBindResult VarBind { get; }

        public LetBindResult(TeuchiUdonTables tables, IToken start, IToken stop)
            : base(tables, start, stop, false)
        {
        }

        public LetBindResult(TeuchiUdonTables tables, IToken start, IToken stop, KeywordResult letKeyword, VarBindResult varBind)
            : base(tables, start, stop, true)
        {
            LetKeyword = letKeyword;
            VarBind    = varBind;
        }
    }

    public class ExprResult : StatementResult
    {
        public TypedResult Inner { get; }
        public bool ReturnsValue { get; set; } = true;

        public ExprResult(TeuchiUdonTables tables, IToken start, IToken stop)
            : base(tables, start, stop, false)
        {
        }

        public ExprResult(TeuchiUdonTables tables, IToken start, IToken stop, TypedResult inner)
            : base(tables, start, stop, true)
        {
            Inner = inner;
        }
    }

    public abstract class TypedResult : TeuchiUdonParserResult
    {
        public TeuchiUdonType Type { get; set; }

        public TypedResult(TeuchiUdonTables tables, IToken start, IToken stop, bool valid, TeuchiUdonType type, bool typeBound)
            : base(tables, start, stop, valid)
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

        public ExternResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public ExternResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonQualifier qualifier)
            : base(tables, start, stop, true, type, typeBound)
        {
            Qualifier = qualifier;
        }

        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => OutValuess.Values.SelectMany(x => x).Concat(TmpValues.Values);
    }

    public class InvalidResult : TypedResult
    {
        public InvalidResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound)
            : base(tables, start, stop, false, type, typeBound)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class UnknownTypeResult : TypedResult
    {
        public UnknownTypeResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound)
            : base(tables, start, stop, true, type, typeBound)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class UnitResult : TypedResult
    {
        public UnitResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound)
            : base(tables, start, stop, true, type, typeBound)
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

        public BlockResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public BlockResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonBlock block,
            IEnumerable<StatementResult> statements,
            ExprResult expr
        ) : base(tables, start, stop, true, type, typeBound)
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

        public ParenResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public ParenResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound, ExprResult expr)
            : base(tables, start, stop, true, type, typeBound)
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

        public TupleResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public TupleResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound, IEnumerable<ExprResult> exprs)
            : base(tables, start, stop, true, type, typeBound)
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

        public ArrayCtorResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public ArrayCtorResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            IEnumerable<IterExprResult> iters
        ) : base(tables, start, stop, type, typeBound, qualifier)
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

        public LiteralResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public LiteralResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonLiteral literal)
            : base(tables, start, stop, true, type, typeBound)
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

        public ThisResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonThis this_)
            : base(tables, start, stop, true, type, typeBound)
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

        public InterpolatedStringResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public InterpolatedStringResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonLiteral stringLiteral,
            IEnumerable<ExprResult> exprs
        ) : base(tables, start, stop, type, typeBound, qualifier)
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
        public InterpolatedStringPartResult(TeuchiUdonTables tables, IToken start, IToken stop, bool valid, TeuchiUdonType type, bool typeBound)
            : base(tables, start, stop, valid, type, typeBound)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class RegularStringInterpolatedStringPartResult : InterpolatedStringPartResult
    {
        public string RawString { get; }

        public RegularStringInterpolatedStringPartResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public RegularStringInterpolatedStringPartResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound, string rawString)
            : base(tables, start, stop, true, type, typeBound)
        {
            RawString = rawString;
        }

        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class ExprInterpolatedStringPartResult : InterpolatedStringPartResult
    {
        public ExprResult Expr { get; }

        public ExprInterpolatedStringPartResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public ExprInterpolatedStringPartResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound, ExprResult expr)
            : base(tables, start, stop, true, type, typeBound)
        {
            Expr = expr;
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
    }

    public class EvalVarResult : TypedResult
    {
        public TeuchiUdonVar Var { get; }

        public EvalVarResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public EvalVarResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonVar v)
            : base(tables, start, stop, true, type, typeBound)
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

        public EvalTypeResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public EvalTypeResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonType innerType)
            : base(tables, start, stop, true, type, typeBound)
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

        public EvalQualifierResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public EvalQualifierResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonQualifier qualifier)
            : base(tables, start, stop, true, type, typeBound)
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

        public EvalGetterResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public EvalGetterResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod getter
        ) : base(tables, start, stop, type, typeBound, qualifier)
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

        public EvalSetterResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public EvalSetterResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod setter
        ) : base(tables, start, stop, type, typeBound, qualifier)
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

        public EvalGetterSetterResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public EvalGetterSetterResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod getter,
            TeuchiUdonMethod setter
        ) : base(tables, start, stop, type, typeBound, qualifier)
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

        public EvalFuncResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public EvalFuncResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonEvalFunc evalFunc,
            TeuchiUdonOutValue outValue,
            ExprResult expr,
            IEnumerable<ExprResult> args
        ) : base(tables, start, stop, true, type, typeBound)
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

        public EvalSpreadFuncResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public EvalSpreadFuncResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonEvalFunc evalFunc,
            TeuchiUdonOutValue outValue,
            ExprResult expr,
            ExprResult arg
        ) : base(tables, start, stop, true, type, typeBound)
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

        public EvalMethodResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public EvalMethodResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod method,
            ExprResult expr,
            IEnumerable<ExprResult> args
        ) : base(tables, start, stop, type, typeBound, qualifier)
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

        public EvalSpreadMethodResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public EvalSpreadMethodResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod method,
            ExprResult expr,
            ExprResult arg
        ) : base(tables, start, stop, type, typeBound, qualifier)
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

        public EvalCoalescingMethodResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public EvalCoalescingMethodResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod method,
            ExprResult expr1,
            ExprResult expr2,
            IEnumerable<ExprResult> args
        ) : base(tables, start, stop, type, typeBound, qualifier)
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

        public EvalCoalescingSpreadMethodResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public EvalCoalescingSpreadMethodResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod method,
            ExprResult expr1,
            ExprResult expr2,
            ExprResult arg
        ) : base(tables, start, stop, type, typeBound, qualifier)
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
        public KeywordResult CastKeyword { get; }
        public ExprResult Expr { get; }

        public EvalCastResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public EvalCastResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            KeywordResult castKeyword,
            ExprResult expr
        ) : base(tables, start, stop, true, type, typeBound)
        {
            CastKeyword = castKeyword;
            Expr        = expr;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class EvalTypeOfResult : TypedResult
    {
        public EvalTypeOfResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound)
            : base(tables, start, stop, true, type, typeBound)
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

        public EvalVarCandidateResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public EvalVarCandidateResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound, IdentifierResult identifier)
            : base(tables, start, stop, true, type, typeBound)
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

        public EvalArrayIndexerResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public EvalArrayIndexerResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            ExprResult expr,
            ExprResult arg
        ) : base(tables, start, stop, type, typeBound, qualifier)
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

        public TypeCastResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public TypeCastResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound, ExprResult expr, ExprResult arg)
            : base(tables, start, stop, true, type, typeBound)
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

        public ConvertCastResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public ConvertCastResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            ExprResult expr,
            ExprResult arg
        ) : base(tables, start, stop, type, typeBound, qualifier)
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

        public TypeOfResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public TypeOfResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonLiteral literal)
            : base(tables, start, stop, true, type, typeBound)
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

        public OpResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public OpResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonQualifier qualifier, string op)
            : base(tables, start, stop, type, typeBound, qualifier)
        {
            Op = op;
        }
    }

    public abstract class UnaryOpResult : OpResult
    {
        public ExprResult Expr { get; }

        public UnaryOpResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public UnaryOpResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            string op,
            ExprResult expr
        ) : base(tables, start, stop, type, typeBound, qualifier, op)
        {
            Expr = expr;
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class PrefixResult : UnaryOpResult
    {
        public PrefixResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public PrefixResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            string op,
            ExprResult expr
        ) : base(tables, start, stop, type, typeBound, qualifier, op, expr)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
    }

    public abstract class BinaryOpResult : OpResult
    {
        public ExprResult Expr1 { get; }
        public ExprResult Expr2 { get; }

        public BinaryOpResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public BinaryOpResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            string op,
            ExprResult expr1,
            ExprResult expr2
        ) : base(tables, start, stop, type, typeBound, qualifier, op)
        {
            Expr1 = expr1;
            Expr2 = expr2;
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr1.Inner, Expr2.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class InfixResult : BinaryOpResult
    {
        public InfixResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public InfixResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            string op,
            ExprResult expr1,
            ExprResult expr2
        ) : base(tables, start, stop, type, typeBound, qualifier, op, expr1, expr2)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Op == "." ? Expr2.Inner.LeftValues : Array.Empty<ITeuchiUdonLeftValue>();
    }

    public class LetInBindResult : TypedResult
    {
        public KeywordResult LetKeyword { get; }
        public KeywordResult InKeyword { get; }
        public TeuchiUdonLetIn LetIn { get; }
        public VarBindResult VarBind { get; }
        public ExprResult Expr { get; }

        public LetInBindResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public LetInBindResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            KeywordResult letKeyword,
            KeywordResult inKeyword,
            TeuchiUdonLetIn letIn,
            VarBindResult varBind,
            ExprResult expr
        ) : base(tables, start, stop, true, type, typeBound)
        {
            LetKeyword = letKeyword;
            InKeyword  = inKeyword;
            LetIn      = LetIn;
            VarBind    = varBind;
            Expr       = expr;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { VarBind.Expr.Inner, Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Expr.Inner.ReleasedOutValues;
    }

    public class IfResult : TypedResult
    {
        public KeywordResult IfKeyword { get; }
        public KeywordResult[] ThenKeywords { get; }
        public KeywordResult[] ElifKeywords { get; }
        public ExprResult[] Conditions { get; }
        public StatementResult[] Statements { get; }
        public ICodeLabel[] Labels { get; }

        public IfResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public IfResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            KeywordResult ifKeyword,
            IEnumerable<KeywordResult> thenKeywords,
            IEnumerable<KeywordResult> elifKeywords,
            IEnumerable<ExprResult> conditions,
            IEnumerable<StatementResult> statements,
            IEnumerable<ICodeLabel> labels
        ) : base(tables, start, stop, true, type, typeBound)
        {
            IfKeyword    = ifKeyword;
            ThenKeywords = thenKeywords.ToArray();
            ElifKeywords = elifKeywords.ToArray();
            Conditions   = conditions  .ToArray();
            Statements   = statements  .ToArray();
            Labels       = labels      .ToArray();
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
        public KeywordResult IfKeyword { get; }
        public KeywordResult ElseKeyword { get; }
        public KeywordResult[] ThenKeywords { get; }
        public KeywordResult[] ElifKeywords { get; }
        public ExprResult[] Conditions { get; }
        public ExprResult[] ThenParts { get; }
        public ExprResult ElsePart { get; }
        public ICodeLabel[] Labels { get; }

        public IfElseResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public IfElseResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            KeywordResult ifKeyword,
            KeywordResult elseKeyword,
            IEnumerable<KeywordResult> thenKeywords,
            IEnumerable<KeywordResult> elifKeywords,
            IEnumerable<ExprResult> conditions,
            IEnumerable<ExprResult> thenParts,
            ExprResult elsePart,
            IEnumerable<ICodeLabel> labels
        ) : base(tables, start, stop, true, type, typeBound)
        {
            IfKeyword    = ifKeyword;
            ElseKeyword  = elseKeyword;
            ThenKeywords = thenKeywords.ToArray();
            ElifKeywords = elifKeywords.ToArray();
            Conditions   = conditions  .ToArray();
            ThenParts    = thenParts   .ToArray();
            ElsePart     = elsePart;
            Labels       = labels.ToArray();
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
        public KeywordResult WhileKeyword { get; }
        public KeywordResult DoKeyword { get; }
        public ExprResult Condition { get; }
        public ExprResult Expr { get; }
        public ICodeLabel[] Labels { get; }

        public WhileResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public WhileResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            KeywordResult whileKeyword,
            KeywordResult doKeyword,
            ExprResult condition,
            ExprResult expr,
            IEnumerable<ICodeLabel> labels
        ) : base(tables, start, stop, true, type, typeBound)
        {
            WhileKeyword = whileKeyword;
            DoKeyword    = doKeyword;
            Condition    = condition;
            Expr         = expr;
            Labels       = labels.ToArray();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Condition.Inner, Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
    }

    public class ForResult : TypedResult
    {
        public KeywordResult[] ForKeywords { get; }
        public KeywordResult DoKeyword { get; }
        public TeuchiUdonFor For { get; }
        public ForBindResult[] ForBinds { get; }
        public ExprResult Expr { get; }
        public ICodeLabel ContinueLabel { get; }

        public ForResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public ForResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            int index,
            IEnumerable<KeywordResult> forKeywords,
            KeywordResult doKeyword,
            IEnumerable<ForBindResult> forBinds,
            ExprResult expr,
            ICodeLabel continueLabel
        ) : base(tables, start, stop, true, type, typeBound)
        {
            ForKeywords   = forKeywords.ToArray();
            DoKeyword     = doKeyword;
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
        public KeywordResult LoopKeyword { get; }
        public ExprResult Expr { get; }
        public ICodeLabel[] Labels { get; }

        public LoopResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public LoopResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            KeywordResult loopKeyword,
            ExprResult expr,
            IEnumerable<ICodeLabel> labels
        ) : base(tables, start, stop, true, type, typeBound)
        {
            LoopKeyword = loopKeyword;
            Expr        = expr;
            Labels      = labels.ToArray();
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

        public FuncResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public FuncResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonFunc func,
            VarDeclResult varDecl,
            ExprResult expr,
            bool deterministic
        ) : base(tables, start, stop, true, type, typeBound)
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

        public MethodResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, false, type, true)
        {
        }

        public MethodResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound, IdentifierResult identifier)
            : base(tables, start, stop, true, type, typeBound)
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

        public IterExprResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public IterExprResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type, bool typeBound, TeuchiUdonQualifier qualifier)
            : base(tables, start, stop, type, typeBound, qualifier)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
    }

    public class ElementsIterExprResult : IterExprResult
    {
        public ExprResult[] Exprs { get; }
        public override ICodeLabel BreakLabel { get; } = null;

        public ElementsIterExprResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public ElementsIterExprResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            IEnumerable<ExprResult> exprs
        ) : base(tables, start, stop, type, typeBound, qualifier)
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

        public RangeIterExprResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public RangeIterExprResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            ExprResult first,
            ExprResult last
        ) : base(tables, start, stop, type, typeBound, qualifier)
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

        public SteppedRangeIterExprResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public SteppedRangeIterExprResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            ExprResult first,
            ExprResult last,
            ExprResult step
        ) : base(tables, start, stop, type, typeBound, qualifier)
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

        public SpreadIterExprResult(TeuchiUdonTables tables, IToken start, IToken stop, TeuchiUdonType type)
            : base(tables, start, stop, type)
        {
        }

        public SpreadIterExprResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            bool typeBound,
            TeuchiUdonQualifier qualifier,
            ExprResult expr
        ) : base(tables, start, stop, type, typeBound, qualifier)
        {
            Expr = expr;
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class IsoExprResult : TeuchiUdonParserResult
    {
        public ExprResult Expr { get; }

        public IsoExprResult(TeuchiUdonTables tables, IToken start, IToken stop)
            : base(tables, start, stop, false)
        {
        }

        public IsoExprResult(TeuchiUdonTables tables, IToken start, IToken stop, ExprResult expr)
            : base(tables, start, stop, true)
        {
            Expr = expr;
        }
    }

    public class ArgExprResult : TeuchiUdonParserResult
    {
        public KeywordResult RefKeyword { get; }
        public ExprResult Expr { get; }
        public bool Ref { get; }

        public ArgExprResult(TeuchiUdonTables tables, IToken start, IToken stop)
            : base(tables, start, stop, false)
        {
        }

        public ArgExprResult(TeuchiUdonTables tables, IToken start, IToken stop, KeywordResult refKeyword, ExprResult expr, bool rf)
            : base(tables, start, stop, true)
        {
            RefKeyword = refKeyword;
            Expr       = expr;
            Ref        = rf;
        }
    }

    public abstract class ForBindResult : TeuchiUdonParserResult
    {
        public ForBindResult(TeuchiUdonTables tables, IToken start, IToken stop, bool valid)
            : base(tables, start, stop, valid)
        {
        }
    }

    public class LetForBindResult : ForBindResult
    {
        public KeywordResult LetKeyword { get; }
        public TeuchiUdonVarBind VarBind { get; }
        public TeuchiUdonVar[] Vars { get; }
        public VarDeclResult VarDecl { get; }
        public IterExprResult Iter { get; }

        public LetForBindResult(TeuchiUdonTables tables, IToken start, IToken stop)
            : base(tables, start, stop, false)
        {
        }

        public LetForBindResult
        (
            TeuchiUdonTables tables,
            IToken start,
            IToken stop,
            KeywordResult letKeyword,
            TeuchiUdonVarBind varBind,
            IEnumerable<TeuchiUdonVar> vars,
            VarDeclResult varDecl,
            IterExprResult iter
        ) : base(tables, start, stop, true)
        {
            LetKeyword = letKeyword;
            VarBind    = varBind;
            Vars       = vars.ToArray();
            VarDecl    = varDecl;
            Iter       = iter;
        }
    }

    public class AssignForBindResult : ForBindResult
    {
        public ExprResult Expr { get; }
        public IterExprResult Iter { get; }

        public AssignForBindResult(TeuchiUdonTables tables, IToken start, IToken stop)
            : base(tables, start, stop, false)
        {
        }

        public AssignForBindResult(TeuchiUdonTables tables, IToken start, IToken stop, ExprResult expr, IterExprResult iter)
            : base(tables, start, stop, true)
        {
            Expr = expr;
            Iter = iter;
        }
    }
}
