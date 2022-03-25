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
    }

    public class BodyResult : TeuchiUdonParserResult
    {
        public TopStatementResult[] TopStatements { get; }

        public BodyResult(IToken token, IEnumerable<TopStatementResult> topStatements)
            : base(token)
        {
            TopStatements = topStatements.ToArray();
        }
    }

    public abstract class TopStatementResult : TeuchiUdonParserResult
    {
        public TopStatementResult(IToken token)
            : base(token)
        {
        }
    }

    public class TopBindResult : TopStatementResult
    {
        public VarBindResult VarBind { get; }
        public bool Public { get; }
        public TeuchiUdonSyncMode Sync { get; }

        public TopBindResult(IToken token, VarBindResult varBind, bool pub, TeuchiUdonSyncMode sync)
            : base(token)
        {
            VarBind = varBind;
            Public  = pub;
            Sync    = sync;
        }
    }

    public class TopExprResult : TopStatementResult
    {
        public ExprResult Expr { get; }

        public TopExprResult(IToken token, ExprResult expr)
            : base(token)
        {
            Expr = expr;
        }
    }

    public abstract class VarAttrResult : TeuchiUdonParserResult
    {
        public VarAttrResult(IToken token)
            : base(token)
        {
        }
    }

    public class PublicVarAttrResult : VarAttrResult
    {
        public PublicVarAttrResult(IToken token)
            : base(token)
        {
        }
    }

    public class SyncVarAttrResult : VarAttrResult
    {
        public TeuchiUdonSyncMode Mode { get; }

        public SyncVarAttrResult(IToken token, TeuchiUdonSyncMode mode)
            : base(token)
        {
            Mode = mode;
        }
    }

    public abstract class ExprAttrResult : TeuchiUdonParserResult
    {
        public ExprAttrResult(IToken token)
            : base(token)
        {
        }
    }

    public class VarBindResult : TeuchiUdonParserResult
    {
        public TeuchiUdonVarBind VarBind { get; }
        public TeuchiUdonVar[] Vars { get; }
        public VarDeclResult VarDecl { get; }
        public ExprResult Expr { get; }

        public VarBindResult(IToken token, int index, TeuchiUdonQualifier qualifier, IEnumerable<TeuchiUdonVar> vars, VarDeclResult varDecl, ExprResult expr)
            : base(token)
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

        public VarDeclResult(IToken token, TeuchiUdonQualifier qualifier, IEnumerable<QualifiedVarResult> qualifiedVars)
            : base(token)
        {
            Types       = qualifiedVars.Select(x => x.Qualified.Inner.Type.LogicalTypeNameEquals(TeuchiUdonType.Type) ? x.Qualified.Inner.Type.GetArgAsType() : TeuchiUdonType.Unknown).ToArray();
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
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"'{v.Name}' is invalid variable name");
                }
                else if (TeuchiUdonTables.Instance.Vars.ContainsKey(v))
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"'{v.Name}' conflicts with another variable");
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

        public QualifiedVarResult(IToken token, IdentifierResult identifier, ExprResult qualified)
            : base(token)
        {
            Identifier = identifier;
            Qualified  = qualified;
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
        public IDataLabel Label { get; }

        public JumpResult(IToken token, ExprResult value, IDataLabel label)
            : base(token)
        {
            Value = value;
            Label = label;
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
    }

    public class ExprResult : StatementResult
    {
        public TypedResult Inner { get; }
        public bool ReturnsValue { get; set; } = true;

        public ExprResult(IToken token, TypedResult inner)
            : base(token)
        {
            Inner = inner;
        }
    }

    public abstract class TypedResult : TeuchiUdonParserResult
    {
        public TeuchiUdonType Type { get; protected set; }

        public TypedResult(IToken token, TeuchiUdonType type)
            : base(token)
        {
            Type       = type;
            TypeBinded = !type.IsUnknown();
        }

        public ExprResult Instance { get; set; } = null;
        public abstract ITeuchiUdonLeftValue[] LeftValues { get; }
        public abstract IEnumerable<TypedResult> Children { get; }

        protected bool TypeBinded { get; set; }
        public abstract void BindType(TeuchiUdonType type);

        public abstract void ReleaseOutValues();
    }

    public abstract class ExternResult : TypedResult
    {
        public TeuchiUdonQualifier Qualifier { get; }
        public Dictionary<string, TeuchiUdonMethod> Methods { get; protected set; }
        public Dictionary<string, TeuchiUdonOutValue[]> OutValuess { get; protected set; }
        public Dictionary<string, TeuchiUdonOutValue> TmpValues { get; protected set; }
        public Dictionary<string, TeuchiUdonLiteral> Literals { get; protected set; }
        public Dictionary<string, ICodeLabel> Labels { get; protected set; }

        public ExternResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier)
            : base(token, type)
        {
            Qualifier = qualifier;
        }

        public override void ReleaseOutValues()
        {
            foreach (var x in OutValuess.Values.SelectMany(x => x)) TeuchiUdonOutValuePool.Instance.ReleaseOutValue(x);
            foreach (var x in TmpValues .Values                   ) TeuchiUdonOutValuePool.Instance.ReleaseOutValue(x);
        }

        protected abstract IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods();
        protected abstract bool CreateOutValuesForMethods { get; }
        protected abstract IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues();
        protected abstract IEnumerable<TeuchiUdonOutValue> GetInvalidOutValues();
        protected abstract IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals();
        protected abstract IEnumerable<(string key, ICodeLabel value)> GetLabels();

        protected void Init()
        {
            var invalids = GetInvalidOutValues();

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
                                    TeuchiUdonOutValuePool.Instance.RetainOutValueWithoutInvalids(Qualifier.GetFuncQualifier(), y, invalids)
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
                        TeuchiUdonOutValuePool.Instance.RetainOutValueWithoutInvalids(Qualifier.GetFuncQualifier(), x.type, invalids)
                    ),
                    (k, v) => (k, v)
                )
                .ToDictionary(x => x.k, x => x.v);
            Literals = GetLiterals().ToDictionary(x => x.key, x => x.value);
            Labels   = GetLabels  ().ToDictionary(x => x.key, x => x.value);
        }

        protected TeuchiUdonMethod GetMethodFromName(IEnumerable<TeuchiUdonType> types, bool isTypeType, IEnumerable<string> methodNames, IEnumerable<TeuchiUdonType> inTypes)
        {
            foreach (var t in types)
            {
                var type = isTypeType ? TeuchiUdonType.Type.ApplyArgAsType(t) : t;

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
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(Token, $"arguments of '{name}' is nondeterministic");
                        return null;
                    }
                }
            }

            TeuchiUdonLogicalErrorHandler.Instance.ReportError(Token, $"method is not defined");
            return null;
        }
    }

    public class InvalidResult : TypedResult
    {
        public InvalidResult(IToken token)
            : base(token, TeuchiUdonType.Invalid)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();

        public override void BindType(TeuchiUdonType type)
        {
        }

        public override void ReleaseOutValues()
        {
        }
    }

    public class UnknownTypeResult : TypedResult
    {
        public UnknownTypeResult(IToken token)
            : base(token, TeuchiUdonType.Type.ApplyArgAsType(TeuchiUdonType.Unknown))
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();

        public override void BindType(TeuchiUdonType type)
        {
        }

        public override void ReleaseOutValues()
        {
        }
    }

    public class UnitResult : TypedResult
    {
        public UnitResult(IToken token)
            : base(token, TeuchiUdonType.Unit)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();

        public override void BindType(TeuchiUdonType type)
        {
        }

        public override void ReleaseOutValues()
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

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Expr.Inner.Children;

        public override void BindType(TeuchiUdonType type)
        {
        }

        public override void ReleaseOutValues()
        {
            Expr.Inner.ReleaseOutValues();
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

        public override ITeuchiUdonLeftValue[] LeftValues => Expr.Inner.LeftValues;
        public override IEnumerable<TypedResult> Children => Expr.Inner.Children;

        public override void BindType(TeuchiUdonType type)
        {
            if (TypeBinded || type.IsUnknown()) return;
            Expr.Inner.BindType(type);
            Type       = Type.Fix(type);
            TypeBinded = true;
        }

        public override void ReleaseOutValues()
        {
            Expr.Inner.ReleaseOutValues();
        }
    }

    public class TupleResult : TypedResult
    {
        public ExprResult[] Exprs { get; }

        public TupleResult(IToken token, TeuchiUdonType type, IEnumerable<ExprResult> exprs)
            : base(token, type)
        {
            Exprs = exprs.ToArray();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Exprs.SelectMany(x => x.Inner.LeftValues).ToArray();
        public override IEnumerable<TypedResult> Children => Exprs.SelectMany(x => x.Inner.Children);

        public override void BindType(TeuchiUdonType type)
        {
            if (TypeBinded || type.IsUnknown()) return;
            if (!type.LogicalTypeNameEquals(TeuchiUdonType.Tuple)) return;
            foreach (var x in Exprs.Zip(type.GetArgsAsTuple(), (e, t) => (e, t))) x.e.Inner.BindType(x.t);
            Type       = Type.Fix(type);
            TypeBinded = true;
        }

        public override void ReleaseOutValues()
        {
            foreach (var x in Exprs) x.Inner.ReleaseOutValues();
        }
    }

    public class ArrayCtorResult : ExternResult
    {
        public IterExprResult[] Iters { get; }

        public ArrayCtorResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, IEnumerable<IterExprResult> iters)
            : base(token, type, qualifier)
        {
            Iters = iters.ToArray();
            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Iters;

        public override void BindType(TeuchiUdonType type)
        {
            if (TypeBinded || type.IsUnknown()) return;
            Type = Type.Fix(type);
            Init();
            TypeBinded = true;
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
                            new TeuchiUdonType[] { TeuchiUdonType.Int }
                        )
                    ),
                    (
                        "setter",
                        GetMethodFromName
                        (
                            new TeuchiUdonType[] { Type },
                            false,
                            new string[] { "Set" },
                            new TeuchiUdonType[] { Type, TeuchiUdonType.Int, Type.GetArgAsArray() }
                        )
                    ),
                    (
                        "lessThanOrEqual",
                        GetMethodFromName
                        (
                            new TeuchiUdonType[] { TeuchiUdonType.Int },
                            true,
                            new string[] { "op_LessThanOrEqual" },
                            new TeuchiUdonType[] { TeuchiUdonType.Int, TeuchiUdonType.Int }
                        )
                    ),
                    (
                        "addition",
                        GetMethodFromName
                        (
                            new TeuchiUdonType[] { TeuchiUdonType.Int },
                            true,
                            new string[] { "op_Addition" },
                            new TeuchiUdonType[] { TeuchiUdonType.Int, TeuchiUdonType.Int }
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
                            new TeuchiUdonType[] { TeuchiUdonType.AnyArray },
                            true,
                            new string[] { "ctor" },
                            new TeuchiUdonType[] { TeuchiUdonType.Int }
                        )
                    ),
                    (
                        "setter",
                        GetMethodFromName
                        (
                            new TeuchiUdonType[] { TeuchiUdonType.AnyArray },
                            false,
                            new string[] { "Set" },
                            new TeuchiUdonType[] { TeuchiUdonType.AnyArray, TeuchiUdonType.Int, TeuchiUdonType.Object }
                        )
                    ),
                    (
                        "lessThanOrEqual",
                        GetMethodFromName
                        (
                            new TeuchiUdonType[] { TeuchiUdonType.Int },
                            true,
                            new string[] { "op_LessThanOrEqual" },
                            new TeuchiUdonType[] { TeuchiUdonType.Int, TeuchiUdonType.Int }
                        )
                    ),
                    (
                        "addition",
                        GetMethodFromName
                        (
                            new TeuchiUdonType[] { TeuchiUdonType.Int },
                            true,
                            new string[] { "op_Addition" },
                            new TeuchiUdonType[] { TeuchiUdonType.Int, TeuchiUdonType.Int }
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
                ("key"  , TeuchiUdonType.Int),
                ("limit", TeuchiUdonType.Int)
            };
        }

        protected override IEnumerable<TeuchiUdonOutValue> GetInvalidOutValues()
        {
            return Iters.SelectMany(x => x.OutValuess.Values.SelectMany(y => y).Concat(x.TmpValues.Values));
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return new (string, TeuchiUdonLiteral)[]
            {
                ("0", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "0", TeuchiUdonType.Int)),
                ("1", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "1", TeuchiUdonType.Int))
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

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();

        public override void BindType(TeuchiUdonType type)
        {
        }

        public override void ReleaseOutValues()
        {
        }
    }

    public class ThisResult : TypedResult
    {
        public TeuchiUdonThis This { get; }

        public ThisResult(IToken token)
            : base(token, TeuchiUdonType.GameObject)
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

        public override void BindType(TeuchiUdonType type)
        {
        }

        public override void ReleaseOutValues()
        {
        }
    }

    public class EvalVarResult : TypedResult
    {
        public TeuchiUdonVar Var { get; }

        public EvalVarResult(IToken token, TeuchiUdonType type, TeuchiUdonVar v)
            : base(token, type)
        {
            Var = v;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Var.Mut ? new ITeuchiUdonLeftValue[] { Var } : Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();

        public override void BindType(TeuchiUdonType type)
        {
        }

        public override void ReleaseOutValues()
        {
        }
    }

    public class EvalTypeResult : TypedResult
    {
        public TeuchiUdonType InnerType { get; }

        public EvalTypeResult(IToken token, TeuchiUdonType type, TeuchiUdonType innerType)
            : base(token, type)
        {
            InnerType = innerType;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();

        public override void BindType(TeuchiUdonType type)
        {
        }

        public override void ReleaseOutValues()
        {
        }
    }

    public class EvalQualifierResult : TypedResult
    {
        public TeuchiUdonQualifier Qualifier { get; }

        public EvalQualifierResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier)
            : base(token, type)
        {
            Qualifier = qualifier;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();

        public override void BindType(TeuchiUdonType type)
        {
        }

        public override void ReleaseOutValues()
        {
        }
    }

    public class EvalGetterResult : ExternResult
    {
        private TeuchiUdonMethod Getter { get; }

        public EvalGetterResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, TeuchiUdonMethod getter)
            : base(token, type, qualifier)
        {
            Getter = getter;
            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();

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

        protected override IEnumerable<TeuchiUdonOutValue> GetInvalidOutValues()
        {
            return Enumerable.Empty<TeuchiUdonOutValue>();
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

        public EvalSetterResult(IToken token, TeuchiUdonQualifier qualifier, TeuchiUdonMethod setter)
            : base(token, TeuchiUdonType.Unit, qualifier)
        {
            Setter = setter;
            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => new ITeuchiUdonLeftValue[] { Setter };
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();

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

        protected override IEnumerable<TeuchiUdonOutValue> GetInvalidOutValues()
        {
            return Enumerable.Empty<TeuchiUdonOutValue>();
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

        public EvalGetterSetterResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, TeuchiUdonMethod getter, TeuchiUdonMethod setter)
            : base(token, type, qualifier)
        {
            Getter = getter;
            Setter = setter;
            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => new ITeuchiUdonLeftValue[] { Setter };
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();

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

        protected override IEnumerable<TeuchiUdonOutValue> GetInvalidOutValues()
        {
            return Enumerable.Empty<TeuchiUdonOutValue>();
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

        public EvalFuncResult(IToken token, TeuchiUdonType type, int index, TeuchiUdonQualifier qualifier, ExprResult expr, IEnumerable<ExprResult> args)
            : base(token, type)
        {
            EvalFunc = new TeuchiUdonEvalFunc(index, qualifier);
            OutValue = TeuchiUdonOutValuePool.Instance.RetainOutValue(qualifier.GetFuncQualifier(), TeuchiUdonType.UInt);
            Expr     = expr;
            Args     = args.ToArray();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner }.Concat(Args.Select(x => x.Inner));

        public override void BindType(TeuchiUdonType type)
        {
        }

        public override void ReleaseOutValues()
        {
            TeuchiUdonOutValuePool.Instance.ReleaseOutValue(OutValue);
        }
    }

    public class EvalSpreadFuncResult : TypedResult
    {
        public TeuchiUdonEvalFunc EvalFunc { get; }
        public TeuchiUdonOutValue OutValue { get; }
        public ExprResult Expr { get; }
        public ExprResult Arg { get; }

        public EvalSpreadFuncResult(IToken token, TeuchiUdonType type, int index, TeuchiUdonQualifier qualifier, ExprResult expr, ExprResult arg)
            : base(token, type)
        {
            EvalFunc = new TeuchiUdonEvalFunc(index, qualifier);
            OutValue = TeuchiUdonOutValuePool.Instance.RetainOutValue(qualifier.GetFuncQualifier(), TeuchiUdonType.UInt);
            Expr     = expr;
            Arg      = arg;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner, Arg.Inner };

        public override void BindType(TeuchiUdonType type)
        {
        }

        public override void ReleaseOutValues()
        {
            TeuchiUdonOutValuePool.Instance.ReleaseOutValue(OutValue);
        }
    }

    public class EvalMethodResult : ExternResult
    {
        public ExprResult Expr { get; }
        public ExprResult[] Args { get; }
        private TeuchiUdonMethod Method { get; }

        public EvalMethodResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, TeuchiUdonMethod method, ExprResult expr, IEnumerable<ExprResult> args)
            : base(token, type, qualifier)
        {
            Method = method;
            Expr   = expr;
            Args   = args.ToArray();

            Instance = expr.Inner.Instance;

            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner }.Concat(Args.Select(x => x.Inner));

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

        protected override IEnumerable<TeuchiUdonOutValue> GetInvalidOutValues()
        {
            return Enumerable.Empty<TeuchiUdonOutValue>();
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

        public EvalSpreadMethodResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, TeuchiUdonMethod method, ExprResult expr, ExprResult arg)
            : base(token, type, qualifier)
        {
            Method = method;
            Expr   = expr;
            Arg    = arg;

            Instance = expr.Inner.Instance;

            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner, Arg.Inner };

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

        protected override IEnumerable<TeuchiUdonOutValue> GetInvalidOutValues()
        {
            return Enumerable.Empty<TeuchiUdonOutValue>();
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

    public class EvalVarCandidateResult : TypedResult
    {
        public IdentifierResult Identifier { get; }

        public EvalVarCandidateResult(IToken token, IdentifierResult identifier)
            : base(token, TeuchiUdonType.Unknown)
        {
            Identifier = identifier;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();

        public override void BindType(TeuchiUdonType type)
        {
        }

        public override void ReleaseOutValues()
        {
        }
    }

    public class EvalArrayIndexerResult : ExternResult
    {
        public ExprResult Expr { get; }
        public ExprResult Arg { get; }

        public EvalArrayIndexerResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, ExprResult expr, ExprResult arg)
            : base(token, type, qualifier)
        {
            Expr = expr;
            Arg  = arg;
            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner, Arg.Inner };

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
                        new TeuchiUdonType[] { Expr.Inner.Type, TeuchiUdonType.Int }
                    )
                )
            };
        }

        protected override bool CreateOutValuesForMethods => true;

        protected override IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues()
        {
            return Enumerable.Empty<(string, TeuchiUdonType)>();
        }

        protected override IEnumerable<TeuchiUdonOutValue> GetInvalidOutValues()
        {
            return Enumerable.Empty<TeuchiUdonOutValue>();
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

    public class TypeCastResult : TypedResult
    {
        public ExprResult Expr { get; }
        public ExprResult Arg { get; }

        public TypeCastResult(IToken token, TeuchiUdonType type, ExprResult expr, ExprResult arg)
            : base(token, type)
        {
            Expr = expr;
            Arg  = arg;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner, Arg.Inner };

        public override void BindType(TeuchiUdonType type)
        {
        }

        public override void ReleaseOutValues()
        {
        }
    }

    public class ConvertCastResult : ExternResult
    {
        public ExprResult Expr { get; }
        public ExprResult Arg { get; }

        public ConvertCastResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, ExprResult expr, ExprResult arg)
            : base(token, type, qualifier)
        {
            Expr = expr;
            Arg  = arg;
            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner, Arg.Inner };

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

        protected override IEnumerable<TeuchiUdonOutValue> GetInvalidOutValues()
        {
            return Enumerable.Empty<TeuchiUdonOutValue>();
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

    public abstract class OpResult : ExternResult
    {
        public string Op { get; }

        public OpResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, string op)
            : base(token, type, qualifier)
        {
            Op = op;
        }
    }

    public abstract class UnaryOpResult : OpResult
    {
        public ExprResult Expr { get; }

        public UnaryOpResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, string op, ExprResult expr)
            : base(token, type, qualifier, op)
        {
            Expr = expr;
            Init();
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner };

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class PrefixResult : UnaryOpResult
    {
        public PrefixResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, string op, ExprResult expr)
            : base(token, type, qualifier, op, expr)
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

        protected override IEnumerable<TeuchiUdonOutValue> GetInvalidOutValues()
        {
            return Enumerable.Empty<TeuchiUdonOutValue>();
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

        public BinaryOpResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, string op, ExprResult expr1, ExprResult expr2)
            : base(token, type, qualifier, op)
        {
            Expr1 = expr1;
            Expr2 = expr2;
            Init();
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr1.Inner, Expr2.Inner };

        public override void BindType(TeuchiUdonType type)
        {
        }
    }

    public class InfixResult : BinaryOpResult
    {
        public InfixResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, string op, ExprResult expr1, ExprResult expr2)
            : base(token, type, qualifier, op, expr1, expr2)
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
            var objectType = TeuchiUdonType.Object;

            switch (Op)
            {
                case ".":
                    return Enumerable.Empty<(string, TeuchiUdonMethod)>();
                case "?.":
                    if (Expr1.Inner.Type.LogicalTypeEquals(TeuchiUdonType.NullType))
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
                    if (Expr1.Inner.Type.LogicalTypeEquals(TeuchiUdonType.NullType) && Expr2.Inner.Type.LogicalTypeEquals(TeuchiUdonType.NullType))
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
                    if (Expr1.Inner.Type.LogicalTypeEquals(TeuchiUdonType.NullType) && Expr2.Inner.Type.LogicalTypeEquals(TeuchiUdonType.NullType))
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
                    if (Expr1.Inner.Type.LogicalTypeEquals(TeuchiUdonType.NullType))
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

        protected override IEnumerable<TeuchiUdonOutValue> GetInvalidOutValues()
        {
            return Enumerable.Empty<TeuchiUdonOutValue>();
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            switch (Op)
            {
                case "==":
                    return new (string, TeuchiUdonLiteral)[]
                    {
                        ("true", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "true", TeuchiUdonType.Bool))
                    };
                case "!=":
                    return new (string, TeuchiUdonLiteral)[]
                    {
                        ("false", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "false", TeuchiUdonType.Bool))
                    };
                case "&&":
                    return new (string, TeuchiUdonLiteral)[]
                    {
                        ("false", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "false", TeuchiUdonType.Bool))
                    };
                case "||":
                    return new (string, TeuchiUdonLiteral)[]
                    {
                        ("true", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "true", TeuchiUdonType.Bool))
                    };
                case "?.":
                case "??":
                    return new (string, TeuchiUdonLiteral)[]
                    {
                        ("null", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "null", TeuchiUdonType.NullType))
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

    public class ConditionalResult : TypedResult
    {
        public ExprResult Condition { get; }
        public ExprResult Expr1 { get; }
        public ExprResult Expr2 { get; }
        public ICodeLabel[] Labels { get; }

        public ConditionalResult(IToken token, TeuchiUdonType type, ExprResult condition, ExprResult expr1, ExprResult expr2)
            : base(token, type)
        {
            Condition = condition;
            Expr1     = expr1;
            Expr2     = expr2;

            var index1 = TeuchiUdonTables.Instance.GetBranchIndex();
            var index2 = TeuchiUdonTables.Instance.GetBranchIndex();
            Labels     = new ICodeLabel[] { new TeuchiUdonBranch(index1), new TeuchiUdonBranch(index2) };
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => new TypedResult[] { Condition.Inner };

        public override void BindType(TeuchiUdonType type)
        {
        }

        public override void ReleaseOutValues()
        {
        }
    }

    public class LetInBindResult : TypedResult
    {
        public TeuchiUdonLetIn LetIn { get; }
        public VarBindResult VarBind { get; }
        public ExprResult Expr { get; }

        public LetInBindResult(IToken token, TeuchiUdonType type, int index, TeuchiUdonQualifier qualifier, VarBindResult varBind, ExprResult expr)
            : base(token, type)
        {
            LetIn   = new TeuchiUdonLetIn(index, qualifier);
            VarBind = varBind;
            Expr    = expr;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Expr.Inner.Children;

        public override void BindType(TeuchiUdonType type)
        {
            if (TypeBinded || type.IsUnknown()) return;
            Expr.Inner.BindType(type);
            Type       = Type.Fix(type);
            TypeBinded = true;
        }

        public override void ReleaseOutValues()
        {
            Expr.Inner.ReleaseOutValues();
        }
    }

    public class FuncResult : TypedResult
    {
        public TeuchiUdonFunc Func { get; }
        public VarDeclResult VarDecl { get; }
        public ExprResult Expr { get; }

        public FuncResult(IToken token, TeuchiUdonType type, int index, TeuchiUdonQualifier qualifier, VarDeclResult varDecl, ExprResult expr)
            : base(token, type)
        {
            Func    = new TeuchiUdonFunc(index, qualifier, type, varDecl.Vars, expr);
            VarDecl = varDecl;
            Expr    = expr;

            if (TeuchiUdonTables.Instance.Funcs.ContainsKey(Func))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"{Func} conflicts with another function");
            }
            else
            {
                TeuchiUdonTables.Instance.Funcs.Add(Func, Func);
            }
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();

        public override void BindType(TeuchiUdonType type)
        {
        }

        public override void ReleaseOutValues()
        {
        }
    }

    public class MethodResult : TypedResult
    {
        public IdentifierResult Identifier { get; }

        public MethodResult(IToken token, TeuchiUdonType type, IdentifierResult identifier)
            : base(token, type)
        {
            Identifier = identifier;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();

        public override void BindType(TeuchiUdonType type)
        {
        }

        public override void ReleaseOutValues()
        {
        }
    }

    public class ArgExprResult : TeuchiUdonParserResult
    {
        public ExprResult Expr { get; }
        public bool Ref { get; }

        public ArgExprResult(IToken token, ExprResult expr, bool rf)
            : base(token)
        {
            Expr = expr;
            Ref  = rf;
        }
    }

    public abstract class IterExprResult : ExternResult
    {
        public IterExprResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier)
            : base(token, type, qualifier)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Array.Empty<ITeuchiUdonLeftValue>();
    }

    public class ElementsIterExprResult : IterExprResult
    {
        public ExprResult[] Exprs { get; }

        public ElementsIterExprResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, IEnumerable<ExprResult> exprs)
            : base(token, type, qualifier)
        {
            Exprs = exprs.ToArray();
            Init();
        }

        public override IEnumerable<TypedResult> Children => Enumerable.Empty<TypedResult>();

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

        protected override IEnumerable<TeuchiUdonOutValue> GetInvalidOutValues()
        {
            return Enumerable.Empty<TeuchiUdonOutValue>();
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return new (string, TeuchiUdonLiteral)[]
            {
                ("length", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), Exprs.Length.ToString(), TeuchiUdonType.Int))
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

        public RangeIterExprResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, ExprResult first, ExprResult last)
            : base(token, type, qualifier)
        {
            First = first;
            Last  = last;
            Init();
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { First.Inner, Last.Inner };

        public override void BindType(TeuchiUdonType type)
        {
        }

        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            var convertMethodName = TeuchiUdonMethod.GetConvertMethodName(TeuchiUdonType.Int);
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
                ("condition"  , TeuchiUdonType.Bool),
                ("length"     , TeuchiUdonType.Int),
                ("valueLength", Type),
            };
        }

        protected override IEnumerable<TeuchiUdonOutValue> GetInvalidOutValues()
        {
            return Enumerable.Empty<TeuchiUdonOutValue>();
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

        public SteppedRangeIterExprResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, ExprResult first, ExprResult last, ExprResult step)
            : base(token, type, qualifier)
        {
            First = first;
            Last  = last;
            Step  = step;
            Init();
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { First.Inner, Last.Inner, Step.Inner };

        public override void BindType(TeuchiUdonType type)
        {
        }

        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            var convertMethodName = TeuchiUdonMethod.GetConvertMethodName(TeuchiUdonType.Int);
            return new (string, TeuchiUdonMethod)[]
            {
                (
                    "keyGreaterThan",
                    GetMethodFromName
                    (
                        new TeuchiUdonType[] { TeuchiUdonType.Int },
                        true,
                        new string[] { "op_GreaterThan" },
                        new TeuchiUdonType[] { TeuchiUdonType.Int, TeuchiUdonType.Int }
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
                ("isUpTo"     , TeuchiUdonType.Bool),
                ("condition"  , TeuchiUdonType.Bool),
                ("length"     , TeuchiUdonType.Int),
                ("valueLength", Type),
            };
        }

        protected override IEnumerable<TeuchiUdonOutValue> GetInvalidOutValues()
        {
            return Enumerable.Empty<TeuchiUdonOutValue>();
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return new (string, TeuchiUdonLiteral)[]
            {
                ("1", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "1", Type))
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

        public SpreadIterExprResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, ExprResult expr)
            : base(token, type, qualifier)
        {
            Expr = expr;
            Init();
        }

        public override IEnumerable<TypedResult> Children => new TypedResult[] { Expr.Inner };

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
                )
            };
        }

        protected override bool CreateOutValuesForMethods => false;

        protected override IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues()
        {
            return Enumerable.Empty<(string, TeuchiUdonType)>();
        }

        protected override IEnumerable<TeuchiUdonOutValue> GetInvalidOutValues()
        {
            return Enumerable.Empty<TeuchiUdonOutValue>();
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

    public class ElementExprResult : TeuchiUdonParserResult
    {
        public ExprResult Expr { get; }

        public ElementExprResult(IToken token, ExprResult expr)
            : base(token)
        {
            Expr = expr;
        }
    }
}
