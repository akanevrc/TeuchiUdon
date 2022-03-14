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

    public class InitVarAttrResult : VarAttrResult
    {
        public IdentifierResult Identifier { get; }

        public InitVarAttrResult(IToken token, IdentifierResult identifier)
            : base(token)
        {
            Identifier = identifier;
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

    public class InitExprAttrResult : ExprAttrResult
    {
        public IdentifierResult Identifier { get; }

        public InitExprAttrResult(IToken token, IdentifierResult identifier)
            : base(token)
        {
            Identifier = identifier;
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
        public TeuchiUdonType Type { get; }

        public TypedResult(IToken token, TeuchiUdonType type)
            : base(token)
        {
            Type = type;
        }

        public ExprResult Instance { get; set; } = null;
        public abstract ITeuchiUdonLeftValue[] LeftValues { get; }
    }

    public abstract class ExternResult : TypedResult
    {
        public TeuchiUdonQualifier Qualifier { get; }
        public Dictionary<string, TeuchiUdonMethod> Methods { get; protected set; }
        public Dictionary<string, TeuchiUdonOutValue[]> OutValuess { get; protected set; }
        public Dictionary<string, TeuchiUdonLiteral> Literals { get; protected set; }
        public Dictionary<string, ICodeLabel> Labels { get; protected set; }

        public ExternResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier)
            : base(token, type)
        {
            Qualifier = qualifier;
        }

        protected abstract IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods();
        protected abstract IEnumerable<(string key, int count)> GetOutValuess();
        protected abstract IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals();
        protected abstract IEnumerable<(string key, ICodeLabel value)> GetLabels();

        protected void Init()
        {
            Methods    = GetMethods().ToDictionary(x => x.key, x => x.value);
            OutValuess =
                Methods
                .Select(x =>
                    (
                        key  : x.Key,
                        value: x.Value == null ?
                            new TeuchiUdonOutValue[0] :
                            TeuchiUdonOutValuePool.Instance.RetainOutValues(Qualifier.GetFuncQualifier(), x.Value.OutTypes.Length).ToArray()
                    )
                )
                .Concat
                (
                    GetOutValuess()
                    .Select(x =>
                        (
                            key  : x.key,
                            value: TeuchiUdonOutValuePool.Instance.RetainOutValues(Qualifier.GetFuncQualifier(), x.count).ToArray()
                        )
                    )
                )
                .ToDictionary(x => x.key, x => x.value);

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

    public class BottomResult : TypedResult
    {
        public BottomResult(IToken token)
            : base(token, TeuchiUdonType.Bottom)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];
    }

    public class UnknownTypeResult : TypedResult
    {
        public UnknownTypeResult(IToken token)
            : base(token, TeuchiUdonType.Type.ApplyArgAsType(TeuchiUdonType.Unknown))
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];
    }

    public class UnitResult : TypedResult
    {
        public UnitResult(IToken token)
            : base(token, TeuchiUdonType.Unit)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];
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

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];
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
    }

    public class ListCtorResult : ExternResult
    {
        public ListExprResult[] Exprs { get; }

        public ListCtorResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, IEnumerable<ListExprResult> exprs)
            : base(token, type, qualifier)
        {
            Exprs = exprs.ToArray();
            Init();
        }

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];

        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            return new (string, TeuchiUdonMethod)[0];
        }

        protected override IEnumerable<(string key, int count)> GetOutValuess()
        {
            return new (string, int)[0];
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return new (string, TeuchiUdonLiteral)[0];
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            return new (string, ICodeLabel)[0];
        }
    }

    public abstract class ListExprResult : TeuchiUdonParserResult
    {
        public TeuchiUdonType Type { get; }

        public ListExprResult(IToken token, TeuchiUdonType type)
            : base(token)
        {
            Type = type;
        }
    }

    public class ElementListExprResult : ListExprResult
    {
        public ExprResult Expr { get; }

        public ElementListExprResult(IToken token, TeuchiUdonType type, ExprResult expr)
            : base(token, type)
        {
            Expr = expr;
        }
    }

    public class RangeListExprResult : ListExprResult
    {
        public ExprResult FromExpr { get; }
        public ExprResult ToExpr { get; }

        public RangeListExprResult(IToken token, TeuchiUdonType type, ExprResult fromExpr, ExprResult toExpr)
            : base(token, type)
        {
            FromExpr = fromExpr;
            ToExpr   = toExpr;
        }
    }

    public class SteppedRangeListExprResult : ListExprResult
    {
        public ExprResult FromExpr { get; }
        public ExprResult ToExpr { get; }
        public ExprResult StepExpr { get; }

        public SteppedRangeListExprResult(IToken token, TeuchiUdonType type, ExprResult fromExpr, ExprResult toExpr, ExprResult stepExpr)
            : base(token, type)
        {
            FromExpr = fromExpr;
            ToExpr   = toExpr;
            StepExpr = stepExpr;
        }
    }

    public class SpreadListExprResult : ListExprResult
    {
        public ExprResult Expr { get; }

        public SpreadListExprResult(IToken token, TeuchiUdonType type, ExprResult expr)
            : base(token, type)
        {
            Expr = expr;
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

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];
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

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];
    }

    public class EvalVarResult : TypedResult
    {
        public TeuchiUdonVar Var { get; }

        public EvalVarResult(IToken token, TeuchiUdonType type, TeuchiUdonVar v)
            : base(token, type)
        {
            Var = v;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => Var.Mut ? new ITeuchiUdonLeftValue[] { Var } : new ITeuchiUdonLeftValue[0];
    }

    public class EvalTypeResult : TypedResult
    {
        public TeuchiUdonType InnerType { get; }

        public EvalTypeResult(IToken token, TeuchiUdonType type, TeuchiUdonType innerType)
            : base(token, type)
        {
            InnerType = innerType;
        }

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];
    }

    public class EvalQualifierResult : TypedResult
    {
        public TeuchiUdonQualifier Qualifier { get; }

        public EvalQualifierResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier)
            : base(token, type)
        {
            Qualifier = qualifier;
        }

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];
    }

    public class EvalGetterResult : TypedResult
    {
        public TeuchiUdonMethod Method { get; }
        public TeuchiUdonOutValue[] OutValues { get; }

        public EvalGetterResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, TeuchiUdonMethod method)
            : base(token, type)
        {
            Method    = method;
            OutValues = TeuchiUdonOutValuePool.Instance.RetainOutValues(qualifier.GetFuncQualifier(), method.OutTypes.Length).ToArray();
        }

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];
    }

    public class EvalSetterResult : TypedResult
    {
        public TeuchiUdonMethod Method { get; }

        public EvalSetterResult(IToken token, TeuchiUdonMethod method)
            : base(token, TeuchiUdonType.Unit)
        {
            Method = method;
        }

        public override ITeuchiUdonLeftValue[] LeftValues => new ITeuchiUdonLeftValue[] { Method };
    }

    public class EvalGetterSetterResult : TypedResult
    {
        public TeuchiUdonMethod Getter { get; }
        public TeuchiUdonMethod Setter { get; }
        public TeuchiUdonOutValue[] OutValues { get; }

        public EvalGetterSetterResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, TeuchiUdonMethod getter, TeuchiUdonMethod setter)
            : base(token, type)
        {
            Getter    = getter;
            Setter    = setter;
            OutValues = TeuchiUdonOutValuePool.Instance.RetainOutValues(qualifier.GetFuncQualifier(), getter.OutTypes.Length).ToArray();
        }

        public override ITeuchiUdonLeftValue[] LeftValues => new ITeuchiUdonLeftValue[] { Setter };
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
            OutValue = TeuchiUdonOutValuePool.Instance.RetainOutValues(qualifier.GetFuncQualifier(), 1).First();
            Expr     = expr;
            Args     = args.ToArray();
        }

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];
    }

    public class EvalMethodResult : TypedResult
    {
        public TeuchiUdonMethod Method { get; }
        public ExprResult Expr { get; }
        public ExprResult[] Args { get; }
        public TeuchiUdonOutValue[] OutValues { get; }

        public EvalMethodResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, TeuchiUdonMethod method, ExprResult expr, IEnumerable<ExprResult> args)
            : base(token, type)
        {
            Method    = method;
            Expr      = expr;
            Args      = args.ToArray();
            OutValues = TeuchiUdonOutValuePool.Instance.RetainOutValues(qualifier.GetFuncQualifier(), method.OutTypes.Length).ToArray();

            Instance = expr.Inner.Instance;
        }

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];
    }

    public class EvalVarCandidateResult : TypedResult
    {
        public IdentifierResult Identifier { get; }

        public EvalVarCandidateResult(IToken token, IdentifierResult identifier)
            : base(token, TeuchiUdonType.Unknown)
        {
            Identifier = identifier;
        }

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];
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

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];
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

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];

        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            var methodName = ToConvertMethodName(Expr.Inner.Type.GetArgAsType());
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

        private string ToConvertMethodName(TeuchiUdonType type)
        {
            if (type.LogicalTypeEquals(TeuchiUdonType.Bool))
            {
                return "ToBoolean";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Byte))
            {
                return "ToByte";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Char))
            {
                return "ToChar";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.DateTime))
            {
                return "ToDateTime";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Decimal))
            {
                return "ToDecimal";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Double))
            {
                return "ToDouble";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Short))
            {
                return "ToInt16";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Int))
            {
                return "ToInt32";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Long))
            {
                return "ToInt64";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.SByte))
            {
                return "ToSByte";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Float))
            {
                return "ToSingle";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.String))
            {
                return "ToString";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.UShort))
            {
                return "ToUInt16";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.UInt))
            {
                return "ToUInt32";
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.ULong))
            {
                return "ToUInt64";
            }
            else
            {
                return "";
            }
        }

        protected override IEnumerable<(string key, int count)> GetOutValuess()
        {
            return new (string, int)[0];
        }

        protected override IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals()
        {
            return new (string, TeuchiUdonLiteral)[0];
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            return new (string, ICodeLabel)[0];
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
    }

    public class PrefixResult : UnaryOpResult
    {
        public PrefixResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, string op, ExprResult expr)
            : base(token, type, qualifier, op, expr)
        {
        }

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];

        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            var exprType = Expr.Inner.Type;

            switch (Op)
            {
                case "+":
                    return new (string, TeuchiUdonMethod)[0];
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
                    return new (string, TeuchiUdonMethod)[0];
            }
        }

        protected override IEnumerable<(string key, int count)> GetOutValuess()
        {
            switch (Op)
            {
                default:
                    return new (string, int)[0];
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
                    return new (string, TeuchiUdonLiteral)[0];
            }
        }

        protected override IEnumerable<(string key, ICodeLabel value)> GetLabels()
        {
            return new (string, ICodeLabel)[0];
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

        public override ITeuchiUdonLeftValue[] LeftValues => Op == "." ? Expr2.Inner.LeftValues : new ITeuchiUdonLeftValue[0];

        protected override IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods()
        {
            var expr1Type  = Expr1.Inner.Type;
            var expr2Type  = Expr2.Inner.Type;
            var objectType = TeuchiUdonType.Object;

            switch (Op)
            {
                case ".":
                    return new (string, TeuchiUdonMethod)[0];
                case "?.":
                    if (Expr1.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Bottom))
                    {
                        return new (string, TeuchiUdonMethod)[0];
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
                    if (Expr1.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Bottom) && Expr2.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Bottom))
                    {
                        return new (string, TeuchiUdonMethod)[0];
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
                    if (Expr1.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Bottom) && Expr2.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Bottom))
                    {
                        return new (string, TeuchiUdonMethod)[0];
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
                    return new (string, TeuchiUdonMethod)[0];
                case "||":
                    return new (string, TeuchiUdonMethod)[0];
                case "??":
                    if (Expr1.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Bottom))
                    {
                        return new (string, TeuchiUdonMethod)[0];
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
                    return new (string, TeuchiUdonMethod)[0];
                default:
                    return new (string, TeuchiUdonMethod)[0];
            }
        }

        protected override IEnumerable<(string key, int count)> GetOutValuess()
        {
            switch (Op)
            {
                case "?.":
                case "??":
                    return new (string, int)[] { ("tmp", 1) };
                default:
                    return new (string, int)[0];
            }
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
                        ("null", TeuchiUdonLiteral.CreateValue(TeuchiUdonTables.Instance.GetLiteralIndex(), "null", TeuchiUdonType.Bottom))
                    };
                default:
                    return new (string, TeuchiUdonLiteral)[0];
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
                        ("0", new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex())),
                        ("1", new TeuchiUdonBranch(TeuchiUdonTables.Instance.GetBranchIndex()))
                    };
                default:
                    return new (string, ICodeLabel)[0];
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

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];
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

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];
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

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];
    }

    public class MethodResult : TypedResult
    {
        public IdentifierResult Identifier { get; }

        public MethodResult(IToken token, TeuchiUdonType type, IdentifierResult identifier)
            : base(token, type)
        {
            Identifier = identifier;
        }

        public override ITeuchiUdonLeftValue[] LeftValues { get; } = new ITeuchiUdonLeftValue[0];
    }
}
