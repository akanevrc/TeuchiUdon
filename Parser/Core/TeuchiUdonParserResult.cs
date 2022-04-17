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
            int index,
            TeuchiUdonQualifier qualifier,
            IEnumerable<TeuchiUdonVar> vars,
            VarDeclResult varDecl,
            ExprResult expr
        ) : base(start, stop, true)
        {
            VarBind = new TeuchiUdonVarBind(index, qualifier, vars.Select(x => x.Name));
            Vars    = vars.ToArray();
            VarDecl = varDecl;
            Expr    = expr;

            foreach (var v in Vars)
            {
                TeuchiUdonTables.Instance.Vars[v] = v;
            }
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

        public VarDeclResult(IToken start, IToken stop, TeuchiUdonQualifier qualifier, IEnumerable<QualifiedVarResult> qualifiedVars)
            : base(start, stop, true)
        {
            Types = qualifiedVars
                .Select(x =>
                    x.Qualified.Inner.Type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Type) ?
                        x.Qualified.Inner.Type.GetArgAsType() :
                        PrimitiveTypes.Instance.Unknown
                ).ToArray();
            var indices = qualifiedVars.Select(_ => TeuchiUdonTables.Instance.GetVarIndex()).ToArray();
            Vars =
                qualifiedVars
                .Zip(Types  , (q, t) => (q, t))
                .Zip(indices, (x, n) => (x.q, x.t, n))
                .Select(x => new TeuchiUdonVar(x.n, qualifier, x.q.Identifier.Name, x.t, false, false)).ToArray();
            QualifiedVars = qualifiedVars.ToArray();

            foreach (var v in Vars)
            {
                if (!TeuchiUdonTables.IsValidVarName(v.Name))
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"'{v.Name}' is invalid variable name");
                }
                else if (TeuchiUdonTables.Instance.Vars.ContainsKey(v))
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"'{v.Name}' conflicts with another variable");
                }
                else
                {
                    TeuchiUdonTables.Instance.Vars.Add(v, v);
                }
            }
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
        public TeuchiUdonType Type { get; protected set; }

        public TypedResult(IToken start, IToken stop, bool valid, TeuchiUdonType type)
            : base(start, stop, valid)
        {
            Type      = type;
            TypeBound = !type.ContainsUnknown();
        }

        public ExprResult Instance { get; set; } = null;
        public abstract ITeuchiUdonLeftValue[] LeftValues { get; }
        public abstract IEnumerable<TypedResult> Children { get; }
        public abstract IEnumerable<TypedResult> ReleasedChildren { get; }
        public abstract IEnumerable<TeuchiUdonOutValue> ReleasedOutValues { get; }
        public abstract bool Deterministic { get; }
        
        protected bool TypeBound { get; set; }
        public abstract void BindType(TeuchiUdonType type);
    }

    public abstract class ExternResult : TypedResult
    {
        public TeuchiUdonQualifier Qualifier { get; }
        public Dictionary<string, TeuchiUdonMethod> Methods { get; protected set; }
        public Dictionary<string, TeuchiUdonOutValue[]> OutValuess { get; protected set; }
        public Dictionary<string, TeuchiUdonOutValue> TmpValues { get; protected set; }
        public Dictionary<string, TeuchiUdonLiteral> Literals { get; protected set; }
        public Dictionary<string, ICodeLabel> Labels { get; protected set; }

        public ExternResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public ExternResult(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier)
            : base(start, stop, true, type)
        {
            Qualifier = qualifier;
        }

        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => OutValuess.Values.SelectMany(x => x).Concat(TmpValues.Values);

        public override bool Deterministic => Children.All(x => x.Deterministic);

        protected abstract IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods();
        protected abstract bool CreateOutValuesForMethods { get; }
        protected abstract IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues();
        protected abstract IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals();
        protected abstract IEnumerable<(string key, ICodeLabel value)> GetLabels();

        protected void Init()
        {
            Methods    = GetMethods().ToDictionary(x => x.key, x => x.value);
            OutValuess =
                CreateOutValuesForMethods ?
                    Methods
                    .Select(x =>
                        (
                            key  : x.Key,
                            value: x.Value == null ?
                                Array.Empty<TeuchiUdonOutValue>() :
                                x.Value.OutTypes.Select(y =>
                                    TeuchiUdonOutValuePool.Instance.RetainOutValue(Qualifier.GetFuncQualifier(), y)
                                )
                                .ToArray()
                        )
                    )
                    .ToDictionary(x => x.key, x => x.value)
                    :
                    new Dictionary<string, TeuchiUdonOutValue[]>();
            TmpValues =
                GetTmpValues()
                .Store(out var outValues)
                .Select(x => x.key)
                .Zip
                (
                    outValues.Select(x =>
                        TeuchiUdonOutValuePool.Instance.RetainOutValue(Qualifier.GetFuncQualifier(), x.type)
                    ),
                    (k, v) => (k, v)
                )
                .ToDictionary(x => x.k, x => x.v);
            Literals = GetLiterals().ToDictionary(x => x.key, x => x.value);
            Labels   = GetLabels  ().ToDictionary(x => x.key, x => x.value);
        }

        protected TeuchiUdonMethod GetMethodFromName
        (
            IEnumerable<TeuchiUdonType> types,
            bool isTypeType,
            IEnumerable<string> methodNames,
            IEnumerable<TeuchiUdonType> inTypes
        )
        {
            foreach (var t in types)
            {
                var type = isTypeType ? PrimitiveTypes.Instance.Type.ApplyArgAsType(t) : t;

                foreach (var name in methodNames)
                {
                    var qm = new TeuchiUdonMethod(type, name, inTypes);
                    var m  = TeuchiUdonTables.Instance.GetMostCompatibleMethods(qm).ToArray();
                    if (m.Length == 0)
                    {
                    }
                    else if (m.Length == 1)
                    {
                        return m[0];
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(Start, $"method '{name}' has multiple overloads");
                        return null;
                    }
                }
            }

            TeuchiUdonLogicalErrorHandler.Instance.ReportError(Start, $"method is not defined");
            return null;
        }
    }

    public class InvalidResult : TypedResult
    {
        public InvalidResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class UnknownTypeResult : TypedResult
    {
        public UnknownTypeResult(IToken start, IToken stop)
            : base(start, stop, true, PrimitiveTypes.Instance.Type.ApplyArgAsType(PrimitiveTypes.Instance.Unknown))
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class UnitResult : TypedResult
    {
        public UnitResult(IToken start, IToken stop)
            : base(start, stop, true, PrimitiveTypes.Instance.Unit)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class BlockResult : TypedResult
    {
        public TeuchiUdonBlock Block { get; }
        public StatementResult[] Statements { get; }
        public ExprResult Expr { get; }

        public BlockResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public BlockResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            int index,
            TeuchiUdonQualifier qualifier,
            IEnumerable<StatementResult> statements,
            ExprResult expr
        ) : base(start, stop, true, type)
        {
            Block      = new TeuchiUdonBlock(index, qualifier, type);
            Statements = statements.ToArray();
            Expr       = expr;

            if (!TeuchiUdonTables.Instance.Blocks.ContainsKey(Block))
            {
                TeuchiUdonTables.Instance.Blocks.Add(Block, Block);
            }
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
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class ParenResult : TypedResult
    {
        public ExprResult Expr { get; }

        public ParenResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public ParenResult(IToken start, IToken stop, TeuchiUdonType type, ExprResult expr)
            : base(start, stop, true, type)
        {
            Expr = expr;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Expr.Inner.LeftValues;
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Expr.Inner.ReleasedOutValues;
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
            if (TypeBound || type.ContainsUnknown()) return;
            Expr.Inner.BindType(type);
            Type      = Type.Fix(type);
            TypeBound = true;
        }
    }

    public class TupleResult : TypedResult
    {
        public ExprResult[] Exprs { get; }

        public TupleResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public TupleResult(IToken start, IToken stop, TeuchiUdonType type, IEnumerable<ExprResult> exprs)
            : base(start, stop, true, type)
        {
            Exprs = exprs.ToArray();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Exprs.SelectMany(x => x.Inner.LeftValues).ToArray();
        public override IEnumerable<TypedResult> Children => Exprs.Select(x => x.Inner);
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Exprs.SelectMany(x => x.Inner.ReleasedOutValues);
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
            if (TypeBound || type.ContainsUnknown()) return;
            if (!type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Tuple)) return;
            foreach (var x in Exprs.Zip(type.GetArgsAsTuple(), (e, t) => (e, t))) x.e.Inner.BindType(x.t);
            Type      = Type.Fix(type);
            TypeBound = true;
        }
    }

    public class ArrayCtorResult : ExternResult
    {
        public IterExprResult[] Iters { get; }

        public ArrayCtorResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public ArrayCtorResult(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier, IEnumerable<IterExprResult> iters)
            : base(start, stop, type, qualifier)
        {
            Iters = iters.ToArray();

            foreach (var o in Children.SelectMany(x => x.ReleasedOutValues))
            {
                TeuchiUdonOutValuePool.Instance.RetainReleasedOutValue(o);
            }
            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Iters;
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => base.ReleasedOutValues.Concat(Children.SelectMany(x => x.ReleasedOutValues));

        public override void BindType(TeuchiUdonType type)
        {
            if (TypeBound || type.ContainsUnknown()) return;
            Type = Type.Fix(type);
            Init();
            TypeBound = true;
        }

        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            if (TeuchiUdonTables.Instance.Types.ContainsKey(Type))
            {
                return new (string, TeuchiUdonMethod)[]
                {
                    (
                        "ctor",
                        GetMethodFromName
                        (
                            new TeuchiUdonType[] { Type },
                            true,
                            new string[] { "ctor" },
                            new TeuchiUdonType[] { PrimitiveTypes.Instance.Int }
                        )
                    ),
                    (
                        "setter",
                        GetMethodFromName
                        (
                            new TeuchiUdonType[] { Type },
                            false,
                            new string[] { "Set" },
                            new TeuchiUdonType[] { Type, PrimitiveTypes.Instance.Int, Type.GetArgAsArray() }
                        )
                    ),
                    (
                        "lessThanOrEqual",
                        GetMethodFromName
                        (
                            new TeuchiUdonType[] { PrimitiveTypes.Instance.Int },
                            true,
                            new string[] { "op_LessThanOrEqual" },
                            new TeuchiUdonType[] { PrimitiveTypes.Instance.Int, PrimitiveTypes.Instance.Int }
                        )
                    ),
                    (
                        "addition",
                        GetMethodFromName
                        (
                            new TeuchiUdonType[] { PrimitiveTypes.Instance.Int },
                            true,
                            new string[] { "op_Addition" },
                            new TeuchiUdonType[] { PrimitiveTypes.Instance.Int, PrimitiveTypes.Instance.Int }
                        )
                    )
                };
            }
            else
            {
                return new (string, TeuchiUdonMethod)[]
                {
                    (
                        "ctor",
                        GetMethodFromName
                        (
                            new TeuchiUdonType[] { PrimitiveTypes.Instance.AnyArray },
                            true,
                            new string[] { "ctor" },
                            new TeuchiUdonType[] { PrimitiveTypes.Instance.Int }
                        )
                    ),
                    (
                        "setter",
                        GetMethodFromName
                        (
                            new TeuchiUdonType[] { PrimitiveTypes.Instance.AnyArray },
                            false,
                            new string[] { "Set" },
                            new TeuchiUdonType[] { PrimitiveTypes.Instance.AnyArray, PrimitiveTypes.Instance.Int, PrimitiveTypes.Instance.Object }
                        )
                    ),
                    (
                        "lessThanOrEqual",
                        GetMethodFromName
                        (
                            new TeuchiUdonType[] { PrimitiveTypes.Instance.Int },
                            true,
                            new string[] { "op_LessThanOrEqual" },
                            new TeuchiUdonType[] { PrimitiveTypes.Instance.Int, PrimitiveTypes.Instance.Int }
                        )
                    ),
                    (
                        "addition",
                        GetMethodFromName
                        (
                            new TeuchiUdonType[] { PrimitiveTypes.Instance.Int },
                            true,
                            new string[] { "op_Addition" },
                            new TeuchiUdonType[] { PrimitiveTypes.Instance.Int, PrimitiveTypes.Instance.Int }
                        )
                    )
                };
            }
        }

        protected override bool CreateOutValuesForMethods => false;

        protected override IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues()
        {
            return new (string, TeuchiUdonType)[]
            {
                ("array", Type),
                ("key"  , PrimitiveTypes.Instance.Int),
                ("limit", PrimitiveTypes.Instance.Int)
            };
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return new (string, TeuchiUdonLiteral)[]
            {
                ("0", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "0", PrimitiveTypes.Instance.Int)),
                ("1", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "1", PrimitiveTypes.Instance.Int))
            };
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            return Enumerable.Empty<(string, ICodeLabel)>();
        }
    }

    public class LiteralResult : TypedResult
    {
        public TeuchiUdonLiteral Literal { get; }

        public LiteralResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public LiteralResult(IToken start, IToken stop, TeuchiUdonType type, int index, string text, object value)
            : base(start, stop, true, type)
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

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class ThisResult : TypedResult
    {
        public TeuchiUdonThis This { get; }

        public ThisResult(IToken start, IToken stop)
            : base(start, stop, true, PrimitiveTypes.Instance.GameObject)
        {
            This = new TeuchiUdonThis();

            if (TeuchiUdonTables.Instance.This.ContainsKey(This))
            {
                This = TeuchiUdonTables.Instance.This[This];
            }
            else
            {
                TeuchiUdonTables.Instance.This.Add(This, This);
            }
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class InterpolatedStringResult : ExternResult
    {
        public TeuchiUdonLiteral StringLiteral { get; }
        public ExprResult[] Exprs { get; }

        public InterpolatedStringResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public InterpolatedStringResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonLiteral stringLiteral,
            IEnumerable<ExprResult> exprs
        ) : base(start, stop, PrimitiveTypes.Instance.String, qualifier)
        {
            StringLiteral = stringLiteral;
            Exprs         = exprs.ToArray();

            foreach (var o in Children.SelectMany(x => x.ReleasedOutValues))
            {
                TeuchiUdonOutValuePool.Instance.RetainReleasedOutValue(o);
            }
            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Exprs.Select(x => x.Inner);
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => base.ReleasedOutValues.Concat(Children.SelectMany(x => x.ReleasedOutValues));

        public override void BindType(TeuchiUdonType type)
        {
        }

        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            var arrayType = PrimitiveTypes.Instance.Object.ToArrayType();
            return new (string, TeuchiUdonMethod)[]
            {
                (
                    "format",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { PrimitiveTypes.Instance.String },
                        true,
                        new string[] { "Format" },
                        new TeuchiUdonType[] { PrimitiveTypes.Instance.String, PrimitiveTypes.Instance.Array.ApplyArgAsArray(PrimitiveTypes.Instance.Object) }
                    )
                ),
                (
                    "arrayCtor",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { arrayType },
                        true,
                        new string[] { "ctor" },
                        new TeuchiUdonType[] { PrimitiveTypes.Instance.Int }
                    )
                ),
                (
                    "arraySetter",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { arrayType },
                        false,
                        new string[] { "Set" },
                        new TeuchiUdonType[] { arrayType, PrimitiveTypes.Instance.Int, PrimitiveTypes.Instance.Object }
                    )
                ),
                (
                    "keyAddition",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { PrimitiveTypes.Instance.Int },
                        true,
                        new string[] { "op_Addition" },
                        new TeuchiUdonType[] { PrimitiveTypes.Instance.Int, PrimitiveTypes.Instance.Int }
                    )
                )
            };
        }

        protected override bool CreateOutValuesForMethods => false;

        protected override IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues()
        {
            return new (string, TeuchiUdonType)[]
            {
                ("array", PrimitiveTypes.Instance.Object.ToArrayType()),
                ("key"  , PrimitiveTypes.Instance.Int),
                ("out"  , PrimitiveTypes.Instance.String)
            };
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return new (string, TeuchiUdonLiteral)[]
            {
                ("0"     , TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "0"                    , PrimitiveTypes.Instance.Int)),
                ("1"     , TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "1"                    , PrimitiveTypes.Instance.Int)),
                ("length", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), Exprs.Length.ToString(), PrimitiveTypes.Instance.Int))
            };
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            return Enumerable.Empty<(string, ICodeLabel)>();
        }
    }

    public abstract class InterpolatedStringPartResult : TypedResult
    {
        public InterpolatedStringPartResult(IToken start, IToken stop, bool valid)
            : base(start, stop, valid, PrimitiveTypes.Instance.String)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class RegularStringInterpolatedStringPartResult : InterpolatedStringPartResult
    {
        public string RawString { get; }

        public RegularStringInterpolatedStringPartResult(IToken start, IToken stop)
            : base(start, stop, false)
        {
        }

        public RegularStringInterpolatedStringPartResult(IToken start, IToken stop, string rawString)
            : base(start, stop, true)
        {
            RawString = rawString;
        }

        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
    }

    public class ExprInterpolatedStringPartResult : InterpolatedStringPartResult
    {
        public ExprResult Expr { get; }

        public ExprInterpolatedStringPartResult(IToken start, IToken stop)
            : base(start, stop, false)
        {
        }

        public ExprInterpolatedStringPartResult(IToken start, IToken stop, ExprResult expr)
            : base(start, stop, true)
        {
            Expr = expr;
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
    }

    public class EvalVarResult : TypedResult
    {
        public TeuchiUdonVar Var { get; }

        public EvalVarResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public EvalVarResult(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonVar v)
            : base(start, stop, true, type)
        {
            Var = v;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Var.Mut ? new ITeuchiUdonLeftValue[] { Var } : Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class EvalTypeResult : TypedResult
    {
        public TeuchiUdonType InnerType { get; }

        public EvalTypeResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public EvalTypeResult(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonType innerType)
            : base(start, stop, true, type)
        {
            InnerType = innerType;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class EvalQualifierResult : TypedResult
    {
        public TeuchiUdonQualifier Qualifier { get; }

        public EvalQualifierResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public EvalQualifierResult(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier)
            : base(start, stop, true, type)
        {
            Qualifier = qualifier;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class EvalGetterResult : ExternResult
    {
        private TeuchiUdonMethod Getter { get; }

        public EvalGetterResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public EvalGetterResult(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier, TeuchiUdonMethod getter)
            : base(start, stop, type, qualifier)
        {
            Getter = getter;
            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;

        public override void BindType(TeuchiUdonType type)
        {
        }

        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            return new (string, TeuchiUdonMethod)[]
            {
                ("getter", Getter)
            };
        }

        protected override bool CreateOutValuesForMethods => true;

        protected override IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues()
        {
            return Enumerable.Empty<(string, TeuchiUdonType)>();
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return Enumerable.Empty<(string, TeuchiUdonLiteral)>();
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            return Enumerable.Empty<(string, ICodeLabel)>();
        }
    }

    public class EvalSetterResult : ExternResult
    {
        private TeuchiUdonMethod Setter { get; }

        public EvalSetterResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public EvalSetterResult(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier, TeuchiUdonMethod setter)
            : base(start, stop, type, qualifier)
        {
            Setter = setter;
            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => new ITeuchiUdonLeftValue[] { Setter };
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;

        public override void BindType(TeuchiUdonType type)
        {
        }

        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            return new (string, TeuchiUdonMethod)[]
            {
                ("setter", Setter)
            };
        }

        protected override bool CreateOutValuesForMethods => true;

        protected override IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues()
        {
            return Enumerable.Empty<(string, TeuchiUdonType)>();
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return Enumerable.Empty<(string, TeuchiUdonLiteral)>();
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            return Enumerable.Empty<(string, ICodeLabel)>();
        }
    }

    public class EvalGetterSetterResult : ExternResult
    {
        private TeuchiUdonMethod Getter { get; }
        private TeuchiUdonMethod Setter { get; }

        public EvalGetterSetterResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public EvalGetterSetterResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod getter,
            TeuchiUdonMethod setter
        ) : base(start, stop, type, qualifier)
        {
            Getter = getter;
            Setter = setter;
            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => new ITeuchiUdonLeftValue[] { Setter };
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;

        public override void BindType(TeuchiUdonType type)
        {
        }

        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            return new (string, TeuchiUdonMethod)[]
            {
                ("getter", Getter),
                ("setter", Setter)
            };
        }

        protected override bool CreateOutValuesForMethods => true;

        protected override IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues()
        {
            return Enumerable.Empty<(string, TeuchiUdonType)>();
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return Enumerable.Empty<(string, TeuchiUdonLiteral)>();
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            return Enumerable.Empty<(string, ICodeLabel)>();
        }
    }

    public class EvalFuncResult : TypedResult
    {
        public TeuchiUdonEvalFunc EvalFunc { get; }
        public TeuchiUdonOutValue OutValue { get; }
        public ExprResult Expr { get; }
        public ExprResult[] Args { get; }

        public EvalFuncResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public EvalFuncResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            int index,
            TeuchiUdonQualifier qualifier,
            ExprResult expr,
            IEnumerable<ExprResult> args
        ) : base(start, stop, true, type)
        {
            EvalFunc = new TeuchiUdonEvalFunc(index, qualifier);
            OutValue = TeuchiUdonOutValuePool.Instance.RetainOutValue(qualifier.GetFuncQualifier(), PrimitiveTypes.Instance.UInt);
            Expr     = expr;
            Args     = args.ToArray();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner }.Concat(Args.Select(x => x.Inner));
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => new TeuchiUdonOutValue[] { OutValue };
        public override bool Deterministic => !Expr.Inner.Type.ContainsNonDetFunc() && Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class EvalSpreadFuncResult : TypedResult
    {
        public TeuchiUdonEvalFunc EvalFunc { get; }
        public TeuchiUdonOutValue OutValue { get; }
        public ExprResult Expr { get; }
        public ExprResult Arg { get; }

        public EvalSpreadFuncResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public EvalSpreadFuncResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            int index,
            TeuchiUdonQualifier qualifier,
            ExprResult expr,
            ExprResult arg
        ) : base(start, stop, true, type)
        {
            EvalFunc = new TeuchiUdonEvalFunc(index, qualifier);
            OutValue = TeuchiUdonOutValuePool.Instance.RetainOutValue(qualifier.GetFuncQualifier(), PrimitiveTypes.Instance.UInt);
            Expr     = expr;
            Arg      = arg;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner, Arg.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => new TeuchiUdonOutValue[] { OutValue };
        public override bool Deterministic => !Expr.Inner.Type.ContainsNonDetFunc() && Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class EvalMethodResult : ExternResult
    {
        public ExprResult Expr { get; }
        public ExprResult[] Args { get; }
        private TeuchiUdonMethod Method { get; }

        public EvalMethodResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public EvalMethodResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod method,
            ExprResult expr,
            IEnumerable<ExprResult> args
        ) : base(start, stop, type, qualifier)
        {
            Method = method;
            Expr   = expr;
            Args   = args.ToArray();

            Instance = expr.Inner.Instance;

            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner }.Concat(Args.Select(x => x.Inner));
        public override IEnumerable<TypedResult> ReleasedChildren => Children;

        public override void BindType(TeuchiUdonType type)
        {
        }
        
        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            return new (string, TeuchiUdonMethod)[]
            {
                ("method", Method)
            };
        }

        protected override bool CreateOutValuesForMethods => true;

        protected override IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues()
        {
            return Enumerable.Empty<(string, TeuchiUdonType)>();
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return Enumerable.Empty<(string, TeuchiUdonLiteral)>();
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            return Enumerable.Empty<(string, ICodeLabel)>();
        }
    }

    public class EvalSpreadMethodResult : ExternResult
    {
        public ExprResult Expr { get; }
        public ExprResult Arg { get; }
        private TeuchiUdonMethod Method { get; }

        public EvalSpreadMethodResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public EvalSpreadMethodResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod method,
            ExprResult expr,
            ExprResult arg
        ) : base(start, stop, type, qualifier)
        {
            Method = method;
            Expr   = expr;
            Arg    = arg;

            Instance = expr.Inner.Instance;

            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner, Arg.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;

        public override void BindType(TeuchiUdonType type)
        {
        }
        
        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            return new (string, TeuchiUdonMethod)[]
            {
                ("method", Method)
            };
        }

        protected override bool CreateOutValuesForMethods => true;

        protected override IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues()
        {
            return Enumerable.Empty<(string, TeuchiUdonType)>();
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return Enumerable.Empty<(string, TeuchiUdonLiteral)>();
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            return Enumerable.Empty<(string, ICodeLabel)>();
        }
    }

    public class EvalCoalescingMethodResult : ExternResult
    {
        public ExprResult Expr1 { get; }
        public ExprResult Expr2 { get; }
        public ExprResult[] Args { get; }
        private TeuchiUdonMethod Method { get; }

        public EvalCoalescingMethodResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public EvalCoalescingMethodResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod method,
            ExprResult expr1,
            ExprResult expr2,
            IEnumerable<ExprResult> args
        ) : base(start, stop, type, qualifier)
        {
            Method = method;
            Expr1  = expr1;
            Expr2  = expr2;
            Args   = args.ToArray();

            Instance = expr2.Inner.Instance;

            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr1.Inner, Expr2.Inner }.Concat(Args.Select(x => x.Inner));
        public override IEnumerable<TypedResult> ReleasedChildren => Children;

        public override void BindType(TeuchiUdonType type)
        {
        }
        
        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            if (Expr1.Inner.Type.LogicalTypeEquals(PrimitiveTypes.Instance.NullType))
            {
                return new (string, TeuchiUdonMethod)[]
                {
                    ("method", Method)
                };
            }
            else
            {
                return new (string, TeuchiUdonMethod)[]
                {
                    ("method", Method),
                    (
                        "==",
                        GetMethodFromName
                        (
                            new TeuchiUdonType[] { Expr1.Inner.Type, PrimitiveTypes.Instance.Object },
                            true,
                            new string[] { "op_Equality" },
                            new TeuchiUdonType[] { Expr1.Inner.Type, Expr1.Inner.Type }
                        )
                    )
                };
            };
        }

        protected override bool CreateOutValuesForMethods => true;

        protected override IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues()
        {
            return new (string, TeuchiUdonType)[] { ("tmp", Expr1.Inner.Type) };
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return new (string, TeuchiUdonLiteral)[]
            {
                ("null", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "null", PrimitiveTypes.Instance.NullType))
            };
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            return new (string, ICodeLabel)[]
            {
                ("1", new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex())),
                ("2", new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex()))
            };
        }
    }

    public class EvalCoalescingSpreadMethodResult : ExternResult
    {
        public ExprResult Expr1 { get; }
        public ExprResult Expr2 { get; }
        public ExprResult Arg { get; }
        private TeuchiUdonMethod Method { get; }

        public EvalCoalescingSpreadMethodResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public EvalCoalescingSpreadMethodResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod method,
            ExprResult expr1,
            ExprResult expr2,
            ExprResult arg
        ) : base(start, stop, type, qualifier)
        {
            Method = method;
            Expr1  = expr1;
            Expr2  = expr2;
            Arg    = arg;

            Instance = expr2.Inner.Instance;

            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr1.Inner, Expr2.Inner, Arg.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;

        public override void BindType(TeuchiUdonType type)
        {
        }
        
        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            if (Expr1.Inner.Type.LogicalTypeEquals(PrimitiveTypes.Instance.NullType))
            {
                return new (string, TeuchiUdonMethod)[]
                {
                    ("method", Method)
                };
            }
            else
            {
                return new (string, TeuchiUdonMethod)[]
                {
                    ("method", Method),
                    (
                        "==",
                        GetMethodFromName
                        (
                            new TeuchiUdonType[] { Expr1.Inner.Type, PrimitiveTypes.Instance.Object },
                            true,
                            new string[] { "op_Equality" },
                            new TeuchiUdonType[] { Expr1.Inner.Type, Expr1.Inner.Type }
                        )
                    )
                };
            };
        }

        protected override bool CreateOutValuesForMethods => true;

        protected override IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues()
        {
            return new (string, TeuchiUdonType)[] { ("tmp", Expr1.Inner.Type) };
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return new (string, TeuchiUdonLiteral)[]
            {
                ("null", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "null", PrimitiveTypes.Instance.NullType))
            };
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            return new (string, ICodeLabel)[]
            {
                ("1", new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex())),
                ("2", new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex()))
            };
        }
    }

    public class EvalCastResult : TypedResult
    {
        public ExprResult Expr { get; }

        public EvalCastResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public EvalCastResult(IToken start, IToken stop, TeuchiUdonType type, ExprResult expr)
            : base(start, stop, true, type)
        {
            Expr = expr;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class EvalTypeOfResult : TypedResult
    {
        public EvalTypeOfResult(IToken start, IToken stop)
            : base(start, stop, true, PrimitiveTypes.Instance.TypeOf)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class EvalVarCandidateResult : TypedResult
    {
        public IdentifierResult Identifier { get; }

        public EvalVarCandidateResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public EvalVarCandidateResult(IToken start, IToken stop, IdentifierResult identifier)
            : base(start, stop, true, PrimitiveTypes.Instance.Unknown)
        {
            Identifier = identifier;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class EvalArrayIndexerResult : ExternResult
    {
        public ExprResult Expr { get; }
        public ExprResult Arg { get; }

        public EvalArrayIndexerResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public EvalArrayIndexerResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            TeuchiUdonQualifier qualifier,
            ExprResult expr,
            ExprResult arg
        ) : base(start, stop, type, qualifier)
        {
            Expr = expr;
            Arg  = arg;
            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues =>
            new ITeuchiUdonLeftValue[] { new TeuchiUdonArraySetter(Expr, Arg, Methods["setter"]) };
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner, Arg.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;

        public override void BindType(TeuchiUdonType type)
        {
        }

        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            return new (string, TeuchiUdonMethod)[]
            {
                (
                    "getter",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { Expr.Inner.Type },
                        false,
                        new string[] { "Get" },
                        new TeuchiUdonType[] { Expr.Inner.Type, PrimitiveTypes.Instance.Int }
                    )
                ),
                (
                    "setter",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { Expr.Inner.Type },
                        false,
                        new string[] { "Set" },
                        new TeuchiUdonType[] { Expr.Inner.Type, PrimitiveTypes.Instance.Int, Expr.Inner.Type.GetArgAsType() }
                    )
                )
            };
        }

        protected override bool CreateOutValuesForMethods => true;

        protected override IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues()
        {
            return Enumerable.Empty<(string, TeuchiUdonType)>();
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return Enumerable.Empty<(string, TeuchiUdonLiteral)>();
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            return Enumerable.Empty<(string, ICodeLabel)>();
        }
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

        public TypeCastResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public TypeCastResult(IToken start, IToken stop, TeuchiUdonType type, ExprResult expr, ExprResult arg)
            : base(start, stop, true, type)
        {
            Expr = expr;
            Arg  = arg;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner, Arg.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class ConvertCastResult : ExternResult
    {
        public ExprResult Expr { get; }
        public ExprResult Arg { get; }

        public ConvertCastResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public ConvertCastResult(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier, ExprResult expr, ExprResult arg)
            : base(start, stop, type, qualifier)
        {
            Expr = expr;
            Arg  = arg;
            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner, Arg.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;

        public override void BindType(TeuchiUdonType type)
        {
        }

        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            var methodName = TeuchiUdonMethod.GetConvertMethodName(Expr.Inner.Type.GetArgAsType());
            return new (string, TeuchiUdonMethod)[]
            {
                (
                    "convert",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { new TeuchiUdonType("SystemConvert") },
                        true,
                        new string[] { methodName },
                        new TeuchiUdonType[] { Arg.Inner.Type }
                    )
                )
            };
        }

        protected override bool CreateOutValuesForMethods => true;

        protected override IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues()
        {
            return Enumerable.Empty<(string, TeuchiUdonType)>();
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return Enumerable.Empty<(string, TeuchiUdonLiteral)>();
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            return Enumerable.Empty<(string, ICodeLabel)>();
        }
    }

    public class TypeOfResult : TypedResult
    {
        public TeuchiUdonLiteral Literal { get; }

        public TypeOfResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public TypeOfResult(IToken start, IToken stop, TeuchiUdonType type)
            : base(start, stop, true, PrimitiveTypes.Instance.DotNetType)
        {
            Literal = TeuchiUdonLiteral.CreateDotNetType(TeuchiUdonTables.Instance.GetLiteralIndex(), type);
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public abstract class OpResult : ExternResult
    {
        public string Op { get; }

        public OpResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public OpResult(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier, string op)
            : base(start, stop, type, qualifier)
        {
            Op = op;
        }
    }

    public abstract class UnaryOpResult : OpResult
    {
        public ExprResult Expr { get; }

        public UnaryOpResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public UnaryOpResult(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier, string op, ExprResult expr)
            : base(start, stop, type, qualifier, op)
        {
            Expr = expr;
            Init();
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class PrefixResult : UnaryOpResult
    {
        public PrefixResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public PrefixResult(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier, string op, ExprResult expr)
            : base(start, stop, type, qualifier, op, expr)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();

        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            var exprType = Expr.Inner.Type;

            switch (Op)
            {
                case "+":
                    return Enumerable.Empty<(string, TeuchiUdonMethod)>();
                case "-":
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "op",
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { exprType },
                                true,
                                new string[] { "op_UnaryMinus" },
                                new TeuchiUdonType[] { exprType }
                            )
                        )
                    };
                case "!":
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "op",
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { exprType },
                                true,
                                new string[] { "op_UnaryNegation" },
                                new TeuchiUdonType[] { exprType }
                            )
                        )
                    };
                case "~":
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "op",
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { exprType },
                                true,
                                new string[] { "op_LogicalXor" },
                                new TeuchiUdonType[] { exprType, exprType }
                            )
                        )
                    };
                default:
                    return Enumerable.Empty<(string, TeuchiUdonMethod)>();
            }
        }

        protected override bool CreateOutValuesForMethods => true;

        protected override IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues()
        {
            switch (Op)
            {
                default:
                    return Enumerable.Empty<(string, TeuchiUdonType)>();
            }
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            switch (Op)
            {
                case "~":
                    return new (string, TeuchiUdonLiteral)[]
                    {
                        ("mask", TeuchiUdonLiteral.CreateMask(TeuchiUdonTables.Instance.GetLiteralIndex(), Expr.Inner.Type))
                    };
                default:
                    return Enumerable.Empty<(string, TeuchiUdonLiteral)>();
            }
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            return Enumerable.Empty<(string, ICodeLabel)>();
        }
    }

    public abstract class BinaryOpResult : OpResult
    {
        public ExprResult Expr1 { get; }
        public ExprResult Expr2 { get; }

        public BinaryOpResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public BinaryOpResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            TeuchiUdonQualifier qualifier,
            string op,
            ExprResult expr1,
            ExprResult expr2
        ) : base(start, stop, type, qualifier, op)
        {
            Expr1 = expr1;
            Expr2 = expr2;
            Init();
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr1.Inner, Expr2.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class InfixResult : BinaryOpResult
    {
        public InfixResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public InfixResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            TeuchiUdonQualifier qualifier,
            string op,
            ExprResult expr1,
            ExprResult expr2
        ) : base(start, stop, type, qualifier, op, expr1, expr2)
        {
            if (op == "." || op == "?.")
            {
                Instance             = expr1;
                expr2.Inner.Instance = expr1;
            }
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Op == "." ? Expr2.Inner.LeftValues : Array.Empty<ITeuchiUdonLeftValue>();

        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            var expr1Type  = Expr1.Inner.Type;
            var expr2Type  = Expr2.Inner.Type;
            var objectType = PrimitiveTypes.Instance.Object;

            switch (Op)
            {
                case ".":
                    return Enumerable.Empty<(string, TeuchiUdonMethod)>();
                case "?.":
                    if (Expr1.Inner.Type.LogicalTypeEquals(PrimitiveTypes.Instance.NullType))
                    {
                        return Enumerable.Empty<(string, TeuchiUdonMethod)>();
                    }
                    else
                    {
                        return new (string, TeuchiUdonMethod)[]
                        {
                            (
                                "==",
                                GetMethodFromName
                                (
                                    new TeuchiUdonType[] { expr1Type, objectType },
                                    true,
                                    new string[] { "op_Equality" },
                                    new TeuchiUdonType[] { expr1Type, expr1Type }
                                )
                            )
                        };
                    }
                case "+":
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "op",
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { expr1Type, expr2Type },
                                true,
                                new string[] { "op_Addition" },
                                new TeuchiUdonType[] { expr1Type, expr2Type }
                            )
                        )
                    };
                case "-":
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "op",
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { expr1Type, expr2Type },
                                true,
                                new string[] { "op_Subtraction" },
                                new TeuchiUdonType[] { expr1Type, expr2Type }
                            )
                        )
                    };
                case "*":
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "op",
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { expr1Type, expr2Type },
                                true,
                                new string[] { "op_Multiplication", "op_Multiply" },
                                new TeuchiUdonType[] { expr1Type, expr2Type }
                            )
                        )
                    };
                case "/":
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "op",
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { expr1Type, expr2Type },
                                true,
                                new string[] { "op_Division" },
                                new TeuchiUdonType[] { expr1Type, expr2Type }
                            )
                        )
                    };
                case "%":
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "op",
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { expr1Type, expr2Type },
                                true,
                                new string[] { "op_Modulus", "op_Remainder" },
                                new TeuchiUdonType[] { expr1Type, expr2Type }
                            )
                        )
                    };
                case "<<":
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "op",
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { expr1Type, expr2Type },
                                true,
                                new string[] { "op_LeftShift" },
                                new TeuchiUdonType[] { expr1Type, expr2Type }
                            )
                        )
                    };
                case ">>":
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "op",
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { expr1Type, expr2Type },
                                true,
                                new string[] { "op_RightShift" },
                                new TeuchiUdonType[] { expr1Type, expr2Type }
                            )
                        )
                    };
                case "<":
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "op",
                            Expr1.Inner.Type.IsAssignableFrom(Expr2.Inner.Type) ?
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { expr1Type },
                                true,
                                new string[] { "op_LessThan" },
                                new TeuchiUdonType[] { expr1Type, expr1Type }
                            ) :
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { expr2Type },
                                true,
                                new string[] { "op_LessThan" },
                                new TeuchiUdonType[] { expr2Type, expr2Type }
                            )
                        )
                    };
                case ">":
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "op",
                            Expr1.Inner.Type.IsAssignableFrom(Expr2.Inner.Type) ?
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { expr1Type },
                                true,
                                new string[] { "op_GreaterThan" },
                                new TeuchiUdonType[] { expr1Type, expr1Type }
                            ) :
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { expr2Type },
                                true,
                                new string[] { "op_GreaterThan" },
                                new TeuchiUdonType[] { expr2Type, expr2Type }
                            )
                        )
                    };
                case "<=":
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "op",
                            Expr1.Inner.Type.IsAssignableFrom(Expr2.Inner.Type) ?
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { expr1Type },
                                true,
                                new string[] { "op_LessThanOrEqual" },
                                new TeuchiUdonType[] { expr1Type, expr1Type }
                            ) :
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { expr2Type },
                                true,
                                new string[] { "op_LessThanOrEqual" },
                                new TeuchiUdonType[] { expr2Type, expr2Type }
                            )
                        )
                    };
                case ">=":
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "op",
                            Expr1.Inner.Type.IsAssignableFrom(Expr2.Inner.Type) ?
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { expr1Type },
                                true,
                                new string[] { "op_GreaterThanOrEqual" },
                                new TeuchiUdonType[] { expr1Type, expr1Type }
                            ) :
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { expr2Type },
                                true,
                                new string[] { "op_GreaterThanOrEqual" },
                                new TeuchiUdonType[] { expr2Type, expr2Type }
                            )
                        )
                    };
                case "==":
                    if (Expr1.Inner.Type.LogicalTypeEquals(PrimitiveTypes.Instance.NullType) && Expr2.Inner.Type.LogicalTypeEquals(PrimitiveTypes.Instance.NullType))
                    {
                        return Enumerable.Empty<(string, TeuchiUdonMethod)>();
                    }
                    else
                    {
                        return new (string, TeuchiUdonMethod)[]
                        {
                            (
                                "op",
                                Expr1.Inner.Type.IsAssignableFrom(Expr2.Inner.Type) ?
                                GetMethodFromName
                                (
                                    new TeuchiUdonType[] { expr1Type, objectType },
                                    true,
                                    new string[] { "op_Equality" },
                                    new TeuchiUdonType[] { expr1Type, expr1Type }
                                ) :
                                GetMethodFromName
                                (
                                    new TeuchiUdonType[] { expr2Type, objectType },
                                    true,
                                    new string[] { "op_Equality" },
                                    new TeuchiUdonType[] { expr2Type, expr2Type }
                                )
                            )
                        };
                    }
                case "!=":
                    if (Expr1.Inner.Type.LogicalTypeEquals(PrimitiveTypes.Instance.NullType) && Expr2.Inner.Type.LogicalTypeEquals(PrimitiveTypes.Instance.NullType))
                    {
                        return Enumerable.Empty<(string, TeuchiUdonMethod)>();
                    }
                    else
                    {
                        return new (string, TeuchiUdonMethod)[]
                        {
                            (
                                "op",
                                Expr1.Inner.Type.IsAssignableFrom(Expr2.Inner.Type) ?
                                GetMethodFromName
                                (
                                    new TeuchiUdonType[] { expr1Type, objectType },
                                    true,
                                    new string[] { "op_Inequality" },
                                    new TeuchiUdonType[] { expr1Type, expr1Type }
                                ) :
                                GetMethodFromName
                                (
                                    new TeuchiUdonType[] { expr2Type, objectType },
                                    true,
                                    new string[] { "op_Inequality" },
                                    new TeuchiUdonType[] { expr2Type, expr2Type }
                                )
                            )
                        };
                    }
                case "&":
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "op",
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { expr1Type, expr2Type },
                                true,
                                new string[] { "op_LogicalAnd" },
                                new TeuchiUdonType[] { expr1Type, expr2Type }
                            )
                        )
                    };
                case "^":
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "op",
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { expr1Type, expr2Type },
                                true,
                                new string[] { "op_LogicalXor" },
                                new TeuchiUdonType[] { expr1Type, expr2Type }
                            )
                        )
                    };
                case "|":
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "op",
                            GetMethodFromName
                            (
                                new TeuchiUdonType[] { expr1Type, expr2Type },
                                true,
                                new string[] { "op_LogicalOr"  },
                                new TeuchiUdonType[] { expr1Type, expr2Type }
                            )
                        )
                    };
                case "&&":
                    return Enumerable.Empty<(string, TeuchiUdonMethod)>();
                case "||":
                    return Enumerable.Empty<(string, TeuchiUdonMethod)>();
                case "??":
                    if (Expr1.Inner.Type.LogicalTypeEquals(PrimitiveTypes.Instance.NullType))
                    {
                        return Enumerable.Empty<(string, TeuchiUdonMethod)>();
                    }
                    else
                    {
                        return new (string, TeuchiUdonMethod)[]
                        {
                            (
                                "==",
                                GetMethodFromName
                                (
                                    new TeuchiUdonType[] { expr1Type, objectType },
                                    true,
                                    new string[] { "op_Equality" },
                                    new TeuchiUdonType[] { expr1Type, expr1Type }
                                )
                            )
                        };
                    }
                case "<-":
                    return Enumerable.Empty<(string, TeuchiUdonMethod)>();
                default:
                    return Enumerable.Empty<(string, TeuchiUdonMethod)>();
            }
        }

        protected override bool CreateOutValuesForMethods => true;

        protected override IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues()
        {
            switch (Op)
            {
                case "?.":
                case "??":
                    return new (string, TeuchiUdonType)[] { ("tmp", Type) };
                default:
                    return Enumerable.Empty<(string, TeuchiUdonType)>();
            }
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            switch (Op)
            {
                case "==":
                    return new (string, TeuchiUdonLiteral)[]
                    {
                        ("true", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "true", PrimitiveTypes.Instance.Bool))
                    };
                case "!=":
                    return new (string, TeuchiUdonLiteral)[]
                    {
                        ("false", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "false", PrimitiveTypes.Instance.Bool))
                    };
                case "&&":
                    return new (string, TeuchiUdonLiteral)[]
                    {
                        ("false", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "false", PrimitiveTypes.Instance.Bool))
                    };
                case "||":
                    return new (string, TeuchiUdonLiteral)[]
                    {
                        ("true", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "true", PrimitiveTypes.Instance.Bool))
                    };
                case "?.":
                case "??":
                    return new (string, TeuchiUdonLiteral)[]
                    {
                        ("null", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "null", PrimitiveTypes.Instance.NullType))
                    };
                default:
                    return Enumerable.Empty<(string, TeuchiUdonLiteral)>();
            }
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            switch (Op)
            {
                case "?.":
                case "&&":
                case "||":
                case "??":
                    return new (string, ICodeLabel)[]
                    {
                        ("1", new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex())),
                        ("2", new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex()))
                    };
                default:
                    return Enumerable.Empty<(string, ICodeLabel)>();
            }
        }
    }

    public class LetInBindResult : TypedResult
    {
        public TeuchiUdonLetIn LetIn { get; }
        public VarBindResult VarBind { get; }
        public ExprResult Expr { get; }

        public LetInBindResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public LetInBindResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            int index,
            TeuchiUdonQualifier qualifier,
            VarBindResult varBind,
            ExprResult expr
        ) : base(start, stop, true, type)
        {
            LetIn   = new TeuchiUdonLetIn(index, qualifier);
            VarBind = varBind;
            Expr    = expr;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { VarBind.Expr.Inner, Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Expr.Inner.ReleasedOutValues;
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
            if (TypeBound || type.ContainsUnknown()) return;
            Expr.Inner.BindType(type);
            Type      = Type.Fix(type);
            TypeBound = true;
        }
    }

    public class IfResult : TypedResult
    {
        public ExprResult[] Conditions { get; }
        public StatementResult[] Statements { get; }
        public ICodeLabel[] Labels { get; }

        public IfResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public IfResult(IToken start, IToken stop, TeuchiUdonType type, IEnumerable<ExprResult> conditions, IEnumerable<StatementResult> statements)
            : base(start, stop, true, type)
        {
            Conditions = conditions.ToArray();
            Statements = statements.ToArray();
            Labels     =
                Enumerable
                .Range(0, Conditions.Length)
                .Select(_ => new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex()))
                .ToArray();
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
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class IfElseResult : TypedResult
    {
        public ExprResult[] Conditions { get; }
        public ExprResult[] ThenParts { get; }
        public ExprResult ElsePart { get; }
        public ICodeLabel[] Labels { get; }

        public IfElseResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public IfElseResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            IEnumerable<ExprResult> conditions,
            IEnumerable<ExprResult> thenParts,
            ExprResult elsePart
        ) : base(start, stop, true, type)
        {
            Conditions = conditions.ToArray();
            ThenParts  = thenParts .ToArray();
            ElsePart   = elsePart;
            Labels     =
                Enumerable
                .Range(0, Conditions.Length + 1)
                .Select(_ => new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex()))
                .ToArray();

            foreach (var o in Children.SelectMany(x => x.ReleasedOutValues))
            {
                TeuchiUdonOutValuePool.Instance.RetainReleasedOutValue(o);
            }
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children =>
            Conditions
            .Select(x => x.Inner)
            .Concat(ThenParts.Select(x => x.Inner))
            .Concat(new TypedResult[] { ElsePart.Inner });
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Children.SelectMany(x => x.ReleasedOutValues);
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class WhileResult : TypedResult
    {
        public ExprResult Condition { get; }
        public ExprResult Expr { get; }
        public ICodeLabel[] Labels { get; }

        public WhileResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public WhileResult(IToken start, IToken stop, TeuchiUdonType type, ExprResult condition, ExprResult expr)
            : base(start, stop, true, type)
        {
            Condition = condition;
            Expr      = expr;
            Labels    = new ICodeLabel[]
            {
                new TeuchiUdonLoop(TeuchiUdonTables.Instance.GetLoopIndex()),
                new TeuchiUdonLoop(TeuchiUdonTables.Instance.GetLoopIndex())
            };

            if (Expr.Inner is BlockResult block)
            {
                block.Block.Continue = Labels[0];
                block.Block.Break    = Labels[1];
            }
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Condition.Inner, Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class ForResult : TypedResult
    {
        public TeuchiUdonFor For { get; }
        public ForBindResult[] ForBinds { get; }
        public ExprResult Expr { get; }
        public ICodeLabel ContinueLabel { get; }

        public ForResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public ForResult(IToken start, IToken stop, TeuchiUdonType type, int index, IEnumerable<ForBindResult> forBinds, ExprResult expr)
            : base(start, stop, true, type)
        {
            For           = new TeuchiUdonFor(index);
            ForBinds      = forBinds.ToArray();
            Expr          = expr;
            ContinueLabel = new TeuchiUdonLoop(TeuchiUdonTables.Instance.GetLoopIndex());

            if (Expr.Inner is BlockResult block)
            {
                var forBind = forBinds.FirstOrDefault();
                block.Block.Continue = ContinueLabel;
                block.Block.Break    =
                    forBind == null ? null :
                    forBind is LetForBindResult    letBind ? letBind.Iter.BreakLabel :
                    forBind is AssignForBindResult assign  ? assign .Iter.BreakLabel :
                        null;
            }
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
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class LoopResult : TypedResult
    {
        public ExprResult Expr { get; }
        public ICodeLabel[] Labels { get; }

        public LoopResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public LoopResult(IToken start, IToken stop, TeuchiUdonType type, ExprResult expr)
            : base(start, stop, true, type)
        {
            Expr   = expr;
            Labels = new ICodeLabel[]
            {
                new TeuchiUdonLoop(TeuchiUdonTables.Instance.GetLoopIndex()),
                new TeuchiUdonLoop(TeuchiUdonTables.Instance.GetLoopIndex())
            };

            if (Expr.Inner is BlockResult block)
            {
                block.Block.Continue = Labels[0];
                block.Block.Break    = Labels[1];
            }
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class FuncResult : TypedResult
    {
        public TeuchiUdonFunc Func { get; }
        public VarDeclResult VarDecl { get; }
        public ExprResult Expr { get; }
        private bool IsDet { get; }

        public FuncResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public FuncResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            int index,
            TeuchiUdonQualifier qualifier,
            IEnumerable<TeuchiUdonVar> vars,
            VarDeclResult varDecl,
            ExprResult expr,
            bool deterministic
        ) : base(start, stop, true, type)
        {
            Func    = new TeuchiUdonFunc(index, qualifier, type, vars, expr, deterministic);
            VarDecl = varDecl;
            Expr    = expr;
            IsDet   = deterministic;

            if (TeuchiUdonTables.Instance.Funcs.ContainsKey(Func))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"{Func} conflicts with another function");
            }
            else
            {
                TeuchiUdonTables.Instance.Funcs.Add(Func, Func);
            }

            if (Expr.Inner is BlockResult block)
            {
                block.Block.Return = Func.Return;
            }
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
        public override bool Deterministic => IsDet;

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class MethodResult : TypedResult
    {
        public IdentifierResult Identifier { get; }

        public MethodResult(IToken start, IToken stop)
            : base(start, stop, false, PrimitiveTypes.Instance.Invalid)
        {
        }

        public MethodResult(IToken start, IToken stop, TeuchiUdonType type, IdentifierResult identifier)
            : base(start, stop, true, type)
        {
            Identifier = identifier;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();
        public override IEnumerable<TypedResult> ReleasedChildren => Children;
        public override IEnumerable<TeuchiUdonOutValue> ReleasedOutValues => Enumerable.Empty<TeuchiUdonOutValue>();
        public override bool Deterministic => Children.All(x => x.Deterministic);

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public abstract class IterExprResult : ExternResult
    {
        public abstract ICodeLabel BreakLabel { get; }

        public IterExprResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public IterExprResult(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier)
            : base(start, stop, type, qualifier)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
    }

    public class ElementsIterExprResult : IterExprResult
    {
        public ExprResult[] Exprs { get; }
        public override ICodeLabel BreakLabel { get; } = null;

        public ElementsIterExprResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public ElementsIterExprResult(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier, IEnumerable<ExprResult> exprs)
            : base(start, stop, type, qualifier)
        {
            Exprs = exprs.ToArray();
            Init();
        }

        public override IEnumerable<TypedResult> Children => Exprs.Select(x => x.Inner);
        public override IEnumerable<TypedResult> ReleasedChildren => Enumerable.Empty<TypedResult>();

        public override void BindType(TeuchiUdonType type)
        {
        }

        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            return Enumerable.Empty<(string, TeuchiUdonMethod)>();
        }

        protected override bool CreateOutValuesForMethods => false;

        protected override IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues()
        {
            return Enumerable.Empty<(string, TeuchiUdonType)>();
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return new (string, TeuchiUdonLiteral)[]
            {
                ("length", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), Exprs.Length.ToString(), PrimitiveTypes.Instance.Int))
            };
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            return Enumerable.Empty<(string, ICodeLabel)>();
        }
    }

    public class RangeIterExprResult : IterExprResult
    {
        public ExprResult First { get; }
        public ExprResult Last { get; }
        public override ICodeLabel BreakLabel => Labels["loop2"];

        public RangeIterExprResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public RangeIterExprResult(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier, ExprResult first, ExprResult last)
            : base(start, stop, type, qualifier)
        {
            First = first;
            Last  = last;
            Init();
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { First.Inner, Last.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;

        public override void BindType(TeuchiUdonType type)
        {
        }

        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            var convertMethodName = TeuchiUdonMethod.GetConvertMethodName(PrimitiveTypes.Instance.Int);
            return new (string, TeuchiUdonMethod)[]
            {
                (
                    "lessThanOrEqual",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { Type },
                        true,
                        new string[] { "op_LessThanOrEqual" },
                        new TeuchiUdonType[] { Type, Type }
                    )
                ),
                (
                    "greaterThan",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { Type },
                        true,
                        new string[] { "op_GreaterThan" },
                        new TeuchiUdonType[] { Type, Type }
                    )
                ),
                (
                    "convert",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { new TeuchiUdonType("SystemConvert") },
                        true,
                        new string[] { convertMethodName },
                        new TeuchiUdonType[] { Type }
                    )
                ),
                (
                    "addition",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { Type },
                        true,
                        new string[] { "op_Addition" },
                        new TeuchiUdonType[] { Type, Type }
                    )
                ),
                (
                    "subtraction",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { Type },
                        true,
                        new string[] { "op_Subtraction" },
                        new TeuchiUdonType[] { Type, Type }
                    )
                )
            };
        }

        protected override bool CreateOutValuesForMethods => false;

        protected override IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues()
        {
            return new (string, TeuchiUdonType)[]
            {
                ("value"      , Type),
                ("limit"      , Type),
                ("condition"  , PrimitiveTypes.Instance.Bool),
                ("length"     , PrimitiveTypes.Instance.Int),
                ("valueLength", Type),
            };
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return new (string, TeuchiUdonLiteral)[]
            {
                ("step", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "1", Type))
            };
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            return new (string, ICodeLabel)[]
            {
                ("branch1", new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex())),
                ("branch2", new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex())),
                ("loop1"  , new TeuchiUdonLoop  (TeuchiUdonTables.Instance.GetLoopIndex())),
                ("loop2"  , new TeuchiUdonLoop  (TeuchiUdonTables.Instance.GetLoopIndex()))
            };
        }
    }

    public class SteppedRangeIterExprResult : IterExprResult
    {
        public ExprResult First { get; }
        public ExprResult Last { get; }
        public ExprResult Step { get; }
        public override ICodeLabel BreakLabel => Labels["loop2"];

        public SteppedRangeIterExprResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public SteppedRangeIterExprResult
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            TeuchiUdonQualifier qualifier,
            ExprResult first,
            ExprResult last,
            ExprResult step
        ) : base(start, stop, type, qualifier)
        {
            First = first;
            Last  = last;
            Step  = step;
            Init();
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { First.Inner, Last.Inner, Step.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;

        public override void BindType(TeuchiUdonType type)
        {
        }

        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            var convertMethodName = TeuchiUdonMethod.GetConvertMethodName(PrimitiveTypes.Instance.Int);
            return new (string, TeuchiUdonMethod)[]
            {
                (
                    "keyGreaterThan",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { PrimitiveTypes.Instance.Int },
                        true,
                        new string[] { "op_GreaterThan" },
                        new TeuchiUdonType[] { PrimitiveTypes.Instance.Int, PrimitiveTypes.Instance.Int }
                    )
                ),
                (
                    "equality",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { Type },
                        true,
                        new string[] { "op_Equality" },
                        new TeuchiUdonType[] { Type, Type }
                    )
                ),
                (
                    "lessThanOrEqual",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { Type },
                        true,
                        new string[] { "op_LessThanOrEqual" },
                        new TeuchiUdonType[] { Type, Type }
                    )
                ),
                (
                    "greaterThan",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { Type },
                        true,
                        new string[] { "op_GreaterThan" },
                        new TeuchiUdonType[] { Type, Type }
                    )
                ),
                (
                    "convert",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { new TeuchiUdonType("SystemConvert") },
                        true,
                        new string[] { convertMethodName },
                        new TeuchiUdonType[] { Type }
                    )
                ),
                (
                    "addition",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { Type },
                        true,
                        new string[] { "op_Addition" },
                        new TeuchiUdonType[] { Type, Type }
                    )
                ),
                (
                    "subtraction",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { Type },
                        true,
                        new string[] { "op_Subtraction" },
                        new TeuchiUdonType[] { Type, Type }
                    )
                ),
                (
                    "division",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { Type },
                        true,
                        new string[] { "op_Division" },
                        new TeuchiUdonType[] { Type, Type }
                    )
                )
            };
        }

        protected override bool CreateOutValuesForMethods => false;

        protected override IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues()
        {
            return new (string, TeuchiUdonType)[]
            {
                ("value"      , Type),
                ("limit"      , Type),
                ("step"       , Type),
                ("isUpTo"     , PrimitiveTypes.Instance.Bool),
                ("condition"  , PrimitiveTypes.Instance.Bool),
                ("length"     , PrimitiveTypes.Instance.Int),
                ("valueLength", Type),
            };
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return new (string, TeuchiUdonLiteral)[]
            {
                ("0", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "0", Type))
            };
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            return new (string, ICodeLabel)[]
            {
                ("branch1", new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex())),
                ("branch2", new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex())),
                ("branch3", new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex())),
                ("branch4", new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex())),
                ("branch5", new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex())),
                ("branch6", new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex())),
                ("branch7", new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex())),
                ("branch8", new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex())),
                ("loop1"  , new TeuchiUdonLoop  (TeuchiUdonTables.Instance.GetLoopIndex())),
                ("loop2"  , new TeuchiUdonLoop  (TeuchiUdonTables.Instance.GetLoopIndex()))
            };
        }
    }

    public class SpreadIterExprResult : IterExprResult
    {
        public ExprResult Expr { get; }
        public override ICodeLabel BreakLabel => Labels["loop2"];

        public SpreadIterExprResult(IToken start, IToken stop)
            : base(start, stop)
        {
        }

        public SpreadIterExprResult(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier, ExprResult expr)
            : base(start, stop, type, qualifier)
        {
            Expr = expr;
            Init();
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner };
        public override IEnumerable<TypedResult> ReleasedChildren => Children;

        public override void BindType(TeuchiUdonType type)
        {
        }

        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            var arrayType = Type.ToArrayType();
            return new (string, TeuchiUdonMethod)[]
            {
                (
                    "clone",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { arrayType },
                        false,
                        new string[] { "Clone" },
                        new TeuchiUdonType[] { arrayType }
                    )
                ),
                (
                    "getter",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { arrayType },
                        false,
                        new string[] { "Get" },
                        new TeuchiUdonType[] { arrayType, PrimitiveTypes.Instance.Int }
                    )
                ),
                (
                    "getLength",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { arrayType },
                        false,
                        new string[] { "get_Length" },
                        new TeuchiUdonType[] { arrayType }
                    )
                ),
                (
                    "lessThan",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { PrimitiveTypes.Instance.Int },
                        true,
                        new string[] { "op_LessThan" },
                        new TeuchiUdonType[] { PrimitiveTypes.Instance.Int, PrimitiveTypes.Instance.Int }
                    )
                ),
                (
                    "greaterThanOrEqual",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { PrimitiveTypes.Instance.Int },
                        true,
                        new string[] { "op_GreaterThanOrEqual" },
                        new TeuchiUdonType[] { PrimitiveTypes.Instance.Int, PrimitiveTypes.Instance.Int }
                    )
                ),
                (
                    "addition",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { PrimitiveTypes.Instance.Int },
                        true,
                        new string[] { "op_Addition" },
                        new TeuchiUdonType[] { PrimitiveTypes.Instance.Int, PrimitiveTypes.Instance.Int }
                    )
                )
            };
        }

        protected override bool CreateOutValuesForMethods => false;

        protected override IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues()
        {
            return new (string, TeuchiUdonType)[]
            {
                ("array"      , Type.ToArrayType()),
                ("key"        , PrimitiveTypes.Instance.Int),
                ("value"      , Type),
                ("length"     , PrimitiveTypes.Instance.Int),
                ("condition"  , PrimitiveTypes.Instance.Bool)
            };
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return new (string, TeuchiUdonLiteral)[]
            {
                ("0", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "0", Type)),
                ("1", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "1", Type))
            };
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            return new (string, ICodeLabel)[]
            {
                ("loop1", new TeuchiUdonLoop(TeuchiUdonTables.Instance.GetLoopIndex())),
                ("loop2", new TeuchiUdonLoop(TeuchiUdonTables.Instance.GetLoopIndex()))
            };
        }
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
            int index,
            TeuchiUdonQualifier qualifier,
            IEnumerable<TeuchiUdonVar> vars,
            VarDeclResult varDecl,
            IterExprResult iter
        ) : base(start, stop, true)
        {
            VarBind = new TeuchiUdonVarBind(index, qualifier, vars.Select(x => x.Name));
            Vars    = vars.ToArray();
            VarDecl = varDecl;
            Iter    = iter;

            foreach (var v in Vars)
            {
                TeuchiUdonTables.Instance.Vars[v] = v;
            }
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
