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

        public IEnumerable<TeuchiUdonAssembly> GetAssemblyDataPart()
        {
            var exports = TopStatements
                .Select(x => x is TopBindResult topBind ? topBind : null)
                .Where(x => x != null && x.Export);
            var syncs = TopStatements
                .Select(x => x is TopBindResult topBind ? topBind : null)
                .Where(x => x != null && x.Sync != TeuchiUdonSyncMode.Disable);

            return
                exports.SelectMany(x =>
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_EXPORT_DATA(new TextLabel(x.VarBind.Vars[0].Name))
                    }
                )
                .Concat(syncs.SelectMany(x =>
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_SYNC_DATA(new TextLabel(x.VarBind.Vars[0].Name), TeuchiUdonAssemblySyncMode.Create(x.Sync))
                    }
                ));
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            var topFuncs = TopStatements
                .Where(x => x is TopBindResult topBind && topBind.VarBind.Vars.Length == 1 && topBind.VarBind.Vars[0].Type.LogicalTypeNameEquals(TeuchiUdonType.Func))
                .Select(x => (name: ((TopBindResult)x).VarBind.Vars[0].Name, x: (TopBindResult)x));
            var topStats = TopStatements
                .Where(x => !(x is TopBindResult topBind && topBind.Export))
                .Select(x => x is TopBindResult topBind ? (name: topBind.Init, x) : x is TopExprResult topExpr ? (name: topExpr.Init, x) : (name: null, x))
                .Where(x => x.name != null);

            var topFuncStats = new Dictionary<string, List<TopStatementResult>>();
            foreach (var func in topFuncs)
            {
                if (func.name == "_start" || func.x.Export)
                {
                    topFuncStats.Add(func.name, new List<TopStatementResult>());
                    topFuncStats[func.name].Add(func.x);
                }
            }
            foreach (var stat in topStats)
            {
                if (topFuncStats.ContainsKey(stat.name))
                {
                    topFuncStats[stat.name].Add(stat.x);
                }
            }

            return
                topFuncStats.Count == 0 ? new TeuchiUdonAssembly[0] :
                topFuncStats.Select(x =>
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_EXPORT_CODE(new TextLabel(x.Key)),
                        new Assembly_LABEL      (new TextLabel(x.Key)),
                        new Assembly_INDENT(1)
                    }
                .Concat(x.Value.SelectMany(y => y.GetAssemblyCodePart()))
                .Concat(TeuchiUdonTables.Instance.Vars.ContainsKey(new TeuchiUdonVar(TeuchiUdonQualifier.Top, x.Key)) ?
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_PUSH(new AssemblyAddress_INDIRECT_LABEL(new TextLabel($"topcall[{x.Key}]"))),
                        new Assembly_JUMP_INDIRECT(new AssemblyAddress_DATA_LABEL(TeuchiUdonTables.Instance.Vars[new TeuchiUdonVar(TeuchiUdonQualifier.Top, x.Key)])),
                        new Assembly_LABEL(new TextLabel($"topcall[{x.Key}]")),
                        new Assembly_JUMP(new AssemblyAddress_NUMBER(0xFFFFFFFC)),
                        new Assembly_INDENT(-1)
                    } :
                    new TeuchiUdonAssembly[0]
                ))
                .Aggregate((acc, x) => acc
                    .Concat(new TeuchiUdonAssembly[] { new Assembly_NEW_LINE() })
                    .Concat(x));
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
        public string Init { get; }
        public bool Export { get; }
        public TeuchiUdonSyncMode Sync { get; }

        public TopBindResult(IToken token, VarBindResult varBind, string init, bool export, TeuchiUdonSyncMode sync)
            : base(token)
        {
            VarBind = varBind;
            Init    = init;
            Export  = export;
            Sync    = sync;
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return
                VarBind.Expr.GetAssemblyCodePart()
                .Concat(VarBind.Vars.Reverse().SelectMany(x => new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)),
                    new Assembly_COPY()
                }));
        }
    }

    public class TopExprResult : TopStatementResult
    {
        public ExprResult Expr { get; }
        public string Init { get; }

        public TopExprResult(IToken token, ExprResult expr, string init)
            : base(token)
        {
            Expr = expr;
            Init = init;
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return Expr.GetAssemblyCodePart();
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

    public class ExportVarAttrResult : VarAttrResult
    {
        public ExportVarAttrResult(IToken token)
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
        public IdentifierResult[] Identifiers { get; }
        public ExprResult[] Qualifieds { get; }

        public VarDeclResult(IToken token, TeuchiUdonQualifier qualifier, IEnumerable<IdentifierResult> identifiers, IEnumerable<ExprResult> qualifieds)
            : base(token)
        {
            Types       = qualifieds .Select(x => x.Inner.Type.GetArgAsType()).ToArray();
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
                .Concat(VarBind.Vars.Reverse().SelectMany(x => new TeuchiUdonAssembly[]
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

        public abstract bool IsLeftValue { get; }
    }

    public class BottomResult : TypedResult
    {
        public BottomResult(IToken token)
            : base(token, TeuchiUdonType.Bottom)
        {
        }

        public override bool IsLeftValue => false;

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            TeuchiUdonLogicalErrorHandler.Instance.ReportError(Token, $"bottom detected");
            return new TeuchiUdonAssembly[0];
        }
    }

    public class UnknownTypeResult : TypedResult
    {
        public UnknownTypeResult(IToken token)
            : base(token, TeuchiUdonType.Type.ApplyArgAsType(TeuchiUdonType.Unknown))
        {
        }

        public override bool IsLeftValue => false;
    }

    public class UnitResult : TypedResult
    {
        public UnitResult(IToken token)
            : base(token, TeuchiUdonType.Unit)
        {
        }

        public override bool IsLeftValue => false;
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

        public override bool IsLeftValue => false;

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

        public override bool IsLeftValue => Expr.Inner.IsLeftValue;

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

        public override bool IsLeftValue => false;

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

        public override bool IsLeftValue => true;

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

        public override bool IsLeftValue => false;
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

        public override bool IsLeftValue => false;
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

        public override bool IsLeftValue => false;

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
        public TeuchiUdonOutValue[] OutValues { get; }

        public EvalMethodResult(IToken token, TeuchiUdonType type, TeuchiUdonMethod method, IdentifierResult identifier, IEnumerable<ExprResult> args)
            : base(token, type)
        {
            Method     = method;
            Identifier = identifier;
            Args       = args.ToArray();
            OutValues  = method.OutTypes.Select(x => TeuchiUdonTables.Instance.GetOutValue(x)).ToArray();
        }

        public override bool IsLeftValue => false;

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return
                Method.SortAlongParams
                (
                    Args.Select(x => x.GetAssemblyCodePart()),
                    OutValues.Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) })
                )
                .SelectMany(x => x)
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(Method)
                })
                .Concat(OutValues.SelectMany(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) }));
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

        public override bool IsLeftValue => false;
    }

    public class EvalMethodCandidateResult : EvalCandidateResult
    {
        public ExprResult[] Args { get; }

        public EvalMethodCandidateResult(IToken token, IdentifierResult identifier, IEnumerable<ExprResult> args)
            : base(token, identifier)
        {
            Args = args.ToArray();
        }

        public override bool IsLeftValue => false;
    }

    public abstract class UnaryOpResult : TypedResult
    {
        public string Op { get; }
        public ExprResult Expr { get; }
        public TeuchiUdonMethod[] Methods { get; }
        public TeuchiUdonOutValue[][] OutValuess { get; }
        public TeuchiUdonLiteral[] Literals { get; }

        public UnaryOpResult(IToken token, TeuchiUdonType type, string op, ExprResult expr)
            : base(token, type)
        {
            Op         = op;
            Expr       = expr;
            Methods    = GetMethods   ().ToArray();
            OutValuess = GetOutValuess().Select(x => x.ToArray()).ToArray();
            Literals   = GetLiterals  ().ToArray();
        }

        protected abstract IEnumerable<TeuchiUdonMethod> GetMethods();
        protected abstract IEnumerable<IEnumerable<TeuchiUdonOutValue>> GetOutValuess();
        protected abstract IEnumerable<TeuchiUdonLiteral> GetLiterals();

        protected TeuchiUdonType GetTypeFromLogicalName(string logicalName, bool isTypeType)
        {
            var qt = new TeuchiUdonType(logicalName);
            if (!TeuchiUdonTables.Instance.LogicalTypes.ContainsKey(qt))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(Token, $"type '{logicalName}' is not defined");
                return TeuchiUdonType.Bottom;
            }

            var type = TeuchiUdonTables.Instance.LogicalTypes[qt];
            return isTypeType ? TeuchiUdonType.Type.ApplyArgAsType(type) : type;
        }

        protected TeuchiUdonMethod GetMethodFromName(string logicalTypeName, bool isTypeType, string methodName, params string[] inTypeNames)
        {
            var type    = GetTypeFromLogicalName(logicalTypeName, isTypeType);
            var inTypes = inTypeNames.Select(x => GetTypeFromLogicalName(x, false)).ToArray();
            var qm      = new TeuchiUdonMethod(type, methodName, inTypes);
            var m       = TeuchiUdonTables.Instance.GetMostCompatibleMethods(qm).ToArray();
            if (m.Length == 0)
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(Token, $"operator '{Op}' is not defined");
                return null;
            }
            else if (m.Length == 1)
            {
                return m[0];
            }
            else
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(Token, $"arguments of operator '{Op}' is nondeterministic");
                return null;
            }
        }
    }

    public class PrefixResult : UnaryOpResult
    {
        public PrefixResult(IToken token, TeuchiUdonType type, string op, ExprResult expr)
            : base(token, type, op, expr)
        {
        }

        public override bool IsLeftValue => false;

        protected override IEnumerable<TeuchiUdonMethod> GetMethods()
        {
            var exprType = Expr.Inner.Type.LogicalName;

            switch (Op)
            {
                case "+":
                    return new TeuchiUdonMethod[0];
                case "-":
                    return new TeuchiUdonMethod[] { GetMethodFromName(exprType, true, "op_UnaryMinus"   , exprType) };
                case "!":
                    return new TeuchiUdonMethod[] { GetMethodFromName(exprType, true, "op_UnaryNegation", exprType) };
                case "~":
                    return new TeuchiUdonMethod[] { GetMethodFromName(exprType, true, "op_LogicalXor"   , exprType, exprType) };
                case "++":
                    return new TeuchiUdonMethod[] { GetMethodFromName(exprType, true, "op_Addition"     , exprType, exprType) };
                case "--":
                    return new TeuchiUdonMethod[] { GetMethodFromName(exprType, true, "op_Subtraction"  , exprType, exprType) };
                default:
                    return new TeuchiUdonMethod[0];
            }
        }

        protected override IEnumerable<IEnumerable<TeuchiUdonOutValue>> GetOutValuess()
        {
            switch (Op)
            {
                case "+":
                    return new TeuchiUdonOutValue[0][];
                case "-":
                case "!":
                case "~":
                    return Methods.Select(x => x.OutTypes.Select(y => TeuchiUdonTables.Instance.GetOutValue(y)));
                case "++":
                case "--":
                    return new TeuchiUdonOutValue[0][];
                default:
                    return new TeuchiUdonOutValue[0][];
            }
        }

        protected override IEnumerable<TeuchiUdonLiteral> GetLiterals()
        {
            switch (Op)
            {
                case "+":
                case "-":
                case "!":
                    return new TeuchiUdonLiteral[0];
                case "~":
                case "++":
                case "--":
                    if (Methods[0] == null)
                    {
                        return new TeuchiUdonLiteral[] { null };
                    }
                    else
                    {
                        var index = TeuchiUdonTables.Instance.GetLiteralIndex();
                        return new TeuchiUdonLiteral[] { TeuchiUdonLiteral.CreateNumber(index, "1", Expr.Inner.Type) };
                    }
                default:
                    return new TeuchiUdonLiteral[0];
            }
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            switch (Op)
            {
                case "+":
                    return Expr.GetAssemblyCodePart();
                case "-":
                case "!":
                    return
                        Methods[0] == null ? new TeuchiUdonAssembly[0] :
                        EvalMethod
                        (
                            Methods[0],
                            new TeuchiUdonAssembly[][]
                            {
                                Expr.GetAssemblyCodePart().ToArray()
                            },
                            OutValuess[0].Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) })
                        );
                case "~":
                    return
                        Methods[0] == null || Literals[0] == null ? new TeuchiUdonAssembly[0] :
                        EvalMethod
                        (
                            Methods[0],
                            new TeuchiUdonAssembly[][]
                            {
                                Expr.GetAssemblyCodePart().ToArray(),
                                new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Literals[0])) }
                            },
                            OutValuess[0].Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) })
                        );
                case "++":
                case "--":
                    return
                        Methods[0] == null || Literals[0] == null ? new TeuchiUdonAssembly[0] :
                        EvalMethod
                        (
                            Methods[0],
                            new TeuchiUdonAssembly[][]
                            {
                                Expr.GetAssemblyCodePart().ToArray(),
                                new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Literals[0])) }
                            },
                            new TeuchiUdonAssembly[][]
                            {
                                Expr.GetAssemblyCodePart().ToArray()
                            }
                        );
                default:
                    return new TeuchiUdonAssembly[0];
            }
        }

        private IEnumerable<TeuchiUdonAssembly> EvalMethod
        (
            TeuchiUdonMethod method,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> inValues,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> outValues
        )
        {
            return
                method.SortAlongParams(inValues, outValues).SelectMany(x => x)
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(method)
                })
                .Concat(outValues.SelectMany(x => x));
        }
    }

    public class PostfixResult : UnaryOpResult
    {
        public PostfixResult(IToken token, TeuchiUdonType type, string op, ExprResult expr)
            : base(token, type, op, expr)
        {
        }

        public override bool IsLeftValue => false;

        protected override IEnumerable<TeuchiUdonMethod> GetMethods()
        {
            var exprType = Expr.Inner.Type.LogicalName;

            switch (Op)
            {
                case "++":
                    return new TeuchiUdonMethod[] { GetMethodFromName(exprType, true, "op_Addition"   , exprType, exprType) };
                case "--":
                    return new TeuchiUdonMethod[] { GetMethodFromName(exprType, true, "op_Subtraction", exprType, exprType) };
                default:
                    return new TeuchiUdonMethod[0];
            }
        }

        protected override IEnumerable<IEnumerable<TeuchiUdonOutValue>> GetOutValuess()
        {
            switch (Op)
            {
                case "++":
                case "--":
                    return new TeuchiUdonOutValue[][] { new TeuchiUdonOutValue[] { TeuchiUdonTables.Instance.GetOutValue(Expr.Inner.Type) } };
                default:
                    return new TeuchiUdonOutValue[0][];
            }
        }

        protected override IEnumerable<TeuchiUdonLiteral> GetLiterals()
        {
            switch (Op)
            {
                case "++":
                case "--":
                    if (Methods[0] == null)
                    {
                        return new TeuchiUdonLiteral[] { null };
                    }
                    else
                    {
                        var index = TeuchiUdonTables.Instance.GetLiteralIndex();
                        return new TeuchiUdonLiteral[] { TeuchiUdonLiteral.CreateNumber(index, "1", Expr.Inner.Type) };
                    }
                default:
                    return new TeuchiUdonLiteral[0];
            }
        }

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            switch (Op)
            {
                case "++":
                case "--":
                    return
                        Methods[0] == null || Literals[0] == null ? new TeuchiUdonAssembly[0] :
                        EvalMethod
                        (
                            Methods[0],
                            new TeuchiUdonAssembly[][]
                            {
                                Expr.GetAssemblyCodePart().ToArray(),
                                new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Literals[0])) }
                            },
                            new TeuchiUdonAssembly[][]
                            {
                                Expr.GetAssemblyCodePart().ToArray()
                            },
                            OutValuess[0].Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) })
                        );
                default:
                    return new TeuchiUdonAssembly[0];
            }
        }

        private IEnumerable<TeuchiUdonAssembly> EvalMethod
        (
            TeuchiUdonMethod method,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> inValues,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> outValues,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> tmpValues
        )
        {
            return
                inValues.Zip(tmpValues, (i, t) => (i, t)).SelectMany(x => x.i.Concat(x.t).Concat(new TeuchiUdonAssembly[] { new Assembly_COPY() }))
                .Concat(method.SortAlongParams(inValues, outValues).SelectMany(x => x))
                .Concat(new TeuchiUdonAssembly[]
                    {
                        new Assembly_EXTERN(method)
                    })
                .Concat(tmpValues.SelectMany(x => x));
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

        public override bool IsLeftValue => (Op == "." || Op == "?.") && Expr2.Inner.IsLeftValue;

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return Expr1.GetAssemblyCodePart().Concat(Expr2.GetAssemblyCodePart());
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

        public override bool IsLeftValue => false;

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return
                VarBind.Expr.GetAssemblyCodePart()
                .Concat(VarBind.Vars.Reverse().SelectMany(x => new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)),
                    new Assembly_COPY()
                }))
                .Concat(Expr.GetAssemblyCodePart());
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

        public override bool IsLeftValue => false;

        public override IEnumerable<TeuchiUdonAssembly> GetAssemblyCodePart()
        {
            return new TeuchiUdonAssembly[]
            {
                new Assembly_PUSH(new AssemblyAddress_INDIRECT_LABEL(Func))
            };
        }
    }
}
