using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonParserResultOps
    {
        private TeuchiUdonPrimitives Primitives { get; }
        private TeuchiUdonStaticTables StaticTables { get; }
        private TeuchiUdonInvalids Invalids { get; }
        private TeuchiUdonLogicalErrorHandler LogicalErrorHandler { get; }
        private TeuchiUdonTables Tables { get; }
        private TeuchiUdonTypeOps TypeOps { get; }
        private TeuchiUdonTableOps TableOps { get; }
        private TeuchiUdonOutValuePool OutValuePool { get; }
        private TeuchiUdonSyntaxOps SyntaxOps { get; }

        public TeuchiUdonParserResultOps
        (
            TeuchiUdonPrimitives primitives,
            TeuchiUdonStaticTables staticTables,
            TeuchiUdonInvalids invalids,
            TeuchiUdonLogicalErrorHandler logicalErrorHandler,
            TeuchiUdonTables tables,
            TeuchiUdonTypeOps typeOps,
            TeuchiUdonTableOps tableOps,
            TeuchiUdonOutValuePool outValuePool,
            TeuchiUdonSyntaxOps syntaxOps
        )
        {
            Primitives          = primitives;
            StaticTables        = staticTables;
            Invalids            = invalids;
            LogicalErrorHandler = logicalErrorHandler;
            Tables              = tables;
            TypeOps             = typeOps;
            TableOps            = tableOps;
            OutValuePool        = outValuePool;
            SyntaxOps           = syntaxOps;
        }

        public TargetResult CreateTarget(IToken start, IToken stop)
        {
            return new TargetResult(Tables, start, stop);
        }

        public TargetResult CreateTarget(IToken start, IToken stop, BodyResult body)
        {
            return new TargetResult(Tables, start, stop, body);
        }

        public BodyResult CreateBody(IToken start, IToken stop)
        {
            return new BodyResult(Tables, start, stop);
        }

        public BodyResult CreateBody(IToken start, IToken stop, IEnumerable<TopStatementResult> topStatements)
        {
            return new BodyResult(Tables, start, stop, topStatements);
        }

        public TopBindResult CreateTopBind(IToken start, IToken stop)
        {
            return new TopBindResult(Tables, start, stop);
        }

        public TopBindResult CreateTopBind(IToken start, IToken stop, VarBindResult varBind, bool pub, TeuchiUdonSyncMode sync)
        {
            return new TopBindResult(Tables, start, stop, varBind, pub, sync);
        }

        public TopExprResult CreateTopExpr(IToken start, IToken stop)
        {
            return new TopExprResult(Tables, start, stop);
        }

        public TopExprResult CreateTopExpr(IToken start, IToken stop, ExprResult expr)
        {
            return new TopExprResult(Tables, start, stop, expr);
        }

        public PublicVarAttrResult CreatePublicVarAttr(IToken start, IToken stop)
        {
            return new PublicVarAttrResult(Tables, start, stop);
        }

        public PublicVarAttrResult CreatePublicVarAttr(IToken start, IToken stop, KeywordResult keyword)
        {
            return new PublicVarAttrResult(Tables, start, stop, keyword);
        }

        public SyncVarAttrResult CreateSyncVarAttr(IToken start, IToken stop)
        {
            return new SyncVarAttrResult(Tables, start, stop);
        }

        public SyncVarAttrResult CreateSyncVarAttr(IToken start, IToken stop, KeywordResult keyword, TeuchiUdonSyncMode mode)
        {
            return new SyncVarAttrResult(Tables, start, stop, keyword, mode);
        }

        public KeywordResult CreateKeyword(IToken start, IToken stop)
        {
            return new KeywordResult(Tables, start, stop);
        }

        public KeywordResult CreateKeyword(IToken start, IToken stop, string name)
        {
            return new KeywordResult(Tables, start, stop, name);
        }

        public VarBindResult CreateVarBind(IToken start, IToken stop)
        {
            return new VarBindResult(Tables, start, stop);
        }

        public VarBindResult CreateVarBind
        (
            IToken start,
            IToken stop,
            int index,
            KeywordResult mutKeyword,
            TeuchiUdonQualifier qualifier,
            IEnumerable<TeuchiUdonVar> vars,
            VarDeclResult varDecl,
            ExprResult expr
        )
        {
            var varBind = new TeuchiUdonVarBind(index, qualifier, vars.Select(x => x.Name));

            foreach (var v in vars)
            {
                Tables.Vars[v] = v;
            }

            return new VarBindResult(Tables, start, stop, mutKeyword, varBind, vars, varDecl, expr);
        }

        public VarDeclResult CreateVarDecl(IToken start, IToken stop)
        {
            return new VarDeclResult(Tables, start, stop);
        }

        public VarDeclResult CreateVarDecl(IToken start, IToken stop, TeuchiUdonQualifier qualifier, IEnumerable<QualifiedVarResult> qualifiedVars)
        {
            var types = qualifiedVars
                .Select(x =>
                    x.Qualified.Inner.Type.LogicalTypeNameEquals(Primitives.Type) ?
                        x.Qualified.Inner.Type.GetArgAsType() :
                        Primitives.Unknown
                );
            var indices = qualifiedVars.Select(_ => Tables.GetVarIndex()).ToArray();
            var vars =
                qualifiedVars
                .Zip(types  , (q, t) => (q, t))
                .Zip(indices, (x, n) => (x.q, x.t, n))
                .Select(x => new TeuchiUdonVar(x.n, qualifier, x.q.Identifier.Name, x.t, false, false)).ToArray();

            foreach (var v in vars)
            {
                if (!TeuchiUdonTableOps.IsValidVarName(v.Name))
                {
                    LogicalErrorHandler.ReportError(start, $"'{v.Name}' is invalid variable name");
                }
                else if (Tables.Vars.ContainsKey(v))
                {
                    LogicalErrorHandler.ReportError(start, $"'{v.Name}' conflicts with another variable");
                }
                else
                {
                    Tables.Vars.Add(v, v);
                }
            }

            return new VarDeclResult(Tables, start, stop, vars, types, qualifiedVars);
        }

        public QualifiedVarResult CreateQualifiedVar(IToken start, IToken stop)
        {
            return new QualifiedVarResult(Tables, start, stop);
        }

        public QualifiedVarResult CreateQualifiedVar(IToken start, IToken stop, IdentifierResult identifier, ExprResult qualified)
        {
            return new QualifiedVarResult(Tables, start, stop, identifier, qualified);
        }

        public IdentifierResult CreateIdentifier(IToken start, IToken stop)
        {
            return new IdentifierResult(Tables, start, stop);
        }

        public IdentifierResult CreateIdentifier(IToken start, IToken stop, string name)
        {
            return new IdentifierResult(Tables, start, stop, name);
        }

        public JumpResult CreateJump(IToken start, IToken stop)
        {
            return new JumpResult(Tables, start, stop);
        }

        public JumpResult CreateJump
        (
            IToken start,
            IToken stop,
            KeywordResult jumpKeyword,
            ExprResult value,
            Func<TeuchiUdonBlock> block,
            Func<ITeuchiUdonLabel> label
        )
        {
            return new JumpResult(Tables, start, stop, jumpKeyword, value, block, label);
        }

        public LetBindResult CreateLetBind(IToken start, IToken stop)
        {
            return new LetBindResult(Tables, start, stop);
        }

        public LetBindResult CreateLetBind(IToken start, IToken stop, KeywordResult letKeyword, VarBindResult varBind)
        {
            return new LetBindResult(Tables, start, stop, letKeyword, varBind);
        }

        public ExprResult CreateExpr(IToken start, IToken stop)
        {
            return new ExprResult(Tables, start, stop);
        }

        public ExprResult CreateExpr(IToken start, IToken stop, TypedResult inner)
        {
            return new ExprResult(Tables, start, stop, inner);
        }

        public ExprResult CreateExpr(IToken start, IToken stop, TypedResult inner, bool returnsValue)
        {
            return new ExprResult(Tables, start, stop, inner) { ReturnsValue = returnsValue };
        }

        public InvalidResult CreateInvalid(IToken start, IToken stop)
        {
            return new InvalidResult(Tables, start, stop, Invalids.InvalidType, true);
        }

        public UnknownTypeResult CreateUnknownType(IToken start, IToken stop)
        {
            return new UnknownTypeResult(Tables, start, stop, Primitives.Type.ApplyArgAsType(Primitives.Unknown), false);
        }

        public UnitResult CreateUnit(IToken start, IToken stop)
        {
            return new UnitResult(Tables, start, stop, Primitives.Unit, true);
        }

        public BlockResult CreateBlock(IToken start, IToken stop)
        {
            return new BlockResult(Tables, start, stop, Invalids.InvalidType);
        }

        public BlockResult CreateBlock
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            int index,
            TeuchiUdonQualifier qualifier,
            IEnumerable<StatementResult> statements,
            ExprResult expr
        )
        {
            var block = new TeuchiUdonBlock(index, qualifier, type);

            if (!Tables.Blocks.ContainsKey(block))
            {
                Tables.Blocks.Add(block, block);
            }

            return new BlockResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), block, statements, expr);
        }

        public ParenResult CreateParen(IToken start, IToken stop)
        {
            return new ParenResult(Tables, start, stop, Invalids.InvalidType);
        }

        public ParenResult CreateParen(IToken start, IToken stop, TeuchiUdonType type, ExprResult expr)
        {
            return new ParenResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), expr);
        }

        public TupleResult CreateTuple(IToken start, IToken stop)
        {
            return new TupleResult(Tables, start, stop, Invalids.InvalidType);
        }

        public TupleResult CreateTuple(IToken start, IToken stop, TeuchiUdonType type, IEnumerable<ExprResult> exprs)
        {
            return new TupleResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), exprs);
        }

        public ArrayCtorResult CreateArrayCtor(IToken start, IToken stop)
        {
            return new ArrayCtorResult(Tables, start, stop, Invalids.InvalidType);
        }

        public ArrayCtorResult CreateArrayCtor(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier, IEnumerable<IterExprResult> iters)
        {
            var result = new ArrayCtorResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), qualifier, iters);

            foreach (var o in result.Children.SelectMany(x => x.ReleasedOutValues))
            {
                OutValuePool.RetainReleasedOutValue(o);
            }

            InitExternResult(result);

            return result;
        }

        public LiteralResult CreateLiteral(IToken start, IToken stop)
        {
            return new LiteralResult(Tables, start, stop, Invalids.InvalidType);
        }

        public LiteralResult CreateLiteral(IToken start, IToken stop, TeuchiUdonType type, int index, string text, object value)
        {
            var literal = new TeuchiUdonLiteral(index, text, type, value);

            if (Tables.Literals.ContainsKey(literal))
            {
                literal = Tables.Literals[literal];
            }
            else
            {
                Tables.Literals.Add(literal, literal);
            }

            return new LiteralResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), literal);
        }

        public ThisResult CreateThis(IToken start, IToken stop)
        {
            var this_ = new TeuchiUdonThis(Primitives.GameObject);

            if (Tables.This.ContainsKey(this_))
            {
                this_ = Tables.This[this_];
            }
            else
            {
                Tables.This.Add(this_, this_);
            }

            return new ThisResult(Tables, start, stop, Primitives.GameObject, true, this_);
        }

        public InterpolatedStringResult CreateInterpolatedString(IToken start, IToken stop)
        {
            return new InterpolatedStringResult(Tables, start, stop, Invalids.InvalidType);
        }

        public InterpolatedStringResult CreateInterpolatedString
        (
            IToken start,
            IToken stop,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonLiteral stringLiteral,
            IEnumerable<ExprResult> exprs
        )
        {
            var result = new InterpolatedStringResult(Tables, start, stop, Primitives.String, true, qualifier, stringLiteral, exprs);

            foreach (var o in result.Children.SelectMany(x => x.ReleasedOutValues))
            {
                OutValuePool.RetainReleasedOutValue(o);
            }

            InitExternResult(result);

            return result;
        }

        public RegularStringInterpolatedStringPartResult CreateRegularStringInterpolatedStringPart(IToken start, IToken stop)
        {
            return new RegularStringInterpolatedStringPartResult(Tables, start, stop, Invalids.InvalidType);
        }

        public RegularStringInterpolatedStringPartResult CreateRegularStringInterpolatedStringPart(IToken start, IToken stop, string rawString)
        {
            return new RegularStringInterpolatedStringPartResult(Tables, start, stop, Primitives.String, true, rawString);
        }

        public ExprInterpolatedStringPartResult CreateExprInterpolatedStringPart(IToken start, IToken stop)
        {
            return new ExprInterpolatedStringPartResult(Tables, start, stop, Invalids.InvalidType);
        }

        public ExprInterpolatedStringPartResult CreateExprInterpolatedStringPart(IToken start, IToken stop, ExprResult expr)
        {
            return new ExprInterpolatedStringPartResult(Tables, start, stop, Primitives.String, true, expr);
        }

        public EvalVarResult CreateEvalVar(IToken start, IToken stop)
        {
            return new EvalVarResult(Tables, start, stop, Invalids.InvalidType);
        }

        public EvalVarResult CreateEvalVar(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonVar v)
        {
            return new EvalVarResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), v);
        }

        public EvalTypeResult CreateEvalType(IToken start, IToken stop)
        {
            return new EvalTypeResult(Tables, start, stop, Invalids.InvalidType);
        }

        public EvalTypeResult CreateEvalType(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonType innerType)
        {
            return new EvalTypeResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), innerType);
        }

        public EvalQualifierResult CreateEvalQualifier(IToken start, IToken stop)
        {
            return new EvalQualifierResult(Tables, start, stop, Invalids.InvalidType);
        }

        public EvalQualifierResult CreateEvalQualifier(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier)
        {
            return new EvalQualifierResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), qualifier);
        }

        public EvalGetterResult CreateEvalGetter(IToken start, IToken stop)
        {
            return new EvalGetterResult(Tables, start, stop, Invalids.InvalidType);
        }

        public EvalGetterResult CreateEvalGetter(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier, TeuchiUdonMethod getter)
        {
            var result = new EvalGetterResult(Tables, start, start, type, !TypeOps.ContainsUnknown(type), qualifier, getter);

            InitExternResult(result);

            return result;
        }

        public EvalSetterResult CreateEvalSetter(IToken start, IToken stop)
        {
            return new EvalSetterResult(Tables, start, stop, Invalids.InvalidType);
        }

        public EvalSetterResult CreateEvalSetter(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier, TeuchiUdonMethod setter)
        {
            var result = new EvalSetterResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), qualifier, setter);

            InitExternResult(result);

            return result;
        }

        public EvalGetterSetterResult CreateEvalGetterSetter(IToken start, IToken stop)
        {
            return new EvalGetterSetterResult(Tables, start, stop, Invalids.InvalidType);
        }

        public EvalGetterSetterResult CreateEvalGetterSetter
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod getter,
            TeuchiUdonMethod setter
        )
        {
            var result = new EvalGetterSetterResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), qualifier, getter, setter);

            InitExternResult(result);

            return result;
        }

        public EvalFuncResult CreateEvalFunc(IToken start, IToken stop)
        {
            return new EvalFuncResult(Tables, start, stop, Invalids.InvalidType);
        }

        public EvalFuncResult CreateEvalFunc
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            int index,
            TeuchiUdonQualifier qualifier,
            ExprResult expr,
            IEnumerable<ExprResult> args
        )
        {
            var evalFunc = new TeuchiUdonEvalFunc(index, qualifier);
            var outValue = OutValuePool.RetainOutValue(qualifier.GetFuncQualifier(), Primitives.UInt);

            return new EvalFuncResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), evalFunc, outValue, expr, args);
        }

        public EvalSpreadFuncResult CreateEvalSpreadFunc(IToken start, IToken stop)
        {
            return new EvalSpreadFuncResult(Tables, start, stop, Invalids.InvalidType);
        }

        public EvalSpreadFuncResult CreateEvalSpreadFunc
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            int index,
            TeuchiUdonQualifier qualifier,
            ExprResult expr,
            ExprResult arg
        )
        {
            var evalFunc = new TeuchiUdonEvalFunc(index, qualifier);
            var outValue = OutValuePool.RetainOutValue(qualifier.GetFuncQualifier(), Primitives.UInt);

            return new EvalSpreadFuncResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), evalFunc, outValue, expr, arg);
        }

        public EvalMethodResult CreateEvalMethod(IToken start, IToken stop)
        {
            return new EvalMethodResult(Tables, start, stop, Invalids.InvalidType);
        }

        public EvalMethodResult CreateEvalMethod
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod method,
            ExprResult expr,
            IEnumerable<ExprResult> args
        )
        {
            var result = new EvalMethodResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), qualifier, method, expr, args);

            result.Instance = expr.Inner.Instance;

            InitExternResult(result);

            return result;
        }

        public EvalSpreadMethodResult CreateEvalSpreadMethod(IToken start, IToken stop)
        {
            return new EvalSpreadMethodResult(Tables, start, stop, Invalids.InvalidType);
        }

        public EvalSpreadMethodResult CreateEvalSpreadMethod
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod method,
            ExprResult expr,
            ExprResult arg
        )
        {
            var result = new EvalSpreadMethodResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), qualifier, method, expr, arg);

            result.Instance = expr.Inner.Instance;

            InitExternResult(result);

            return result;
        }

        public EvalCoalescingMethodResult CreateEvalCoalescingMethod(IToken start, IToken stop)
        {
            return new EvalCoalescingMethodResult(Tables, start, stop, Invalids.InvalidType);
        }

        public EvalCoalescingMethodResult CreateEvalCoalescingMethod
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod method,
            ExprResult expr1,
            ExprResult expr2,
            IEnumerable<ExprResult> args
        )
        {
            var result = new EvalCoalescingMethodResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), qualifier, method, expr1, expr2, args);

            result.Instance = expr2.Inner.Instance;

            InitExternResult(result);

            return result;
        }

        public EvalCoalescingSpreadMethodResult CreateEvalCoalescingSpreadMethod(IToken start, IToken stop)
        {
            return new EvalCoalescingSpreadMethodResult(Tables, start, stop, Invalids.InvalidType);
        }

        public EvalCoalescingSpreadMethodResult CreateEvalCoalescingSpreadMethod
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            TeuchiUdonQualifier qualifier,
            TeuchiUdonMethod method,
            ExprResult expr1,
            ExprResult expr2,
            ExprResult arg
        )
        {
            var result = new EvalCoalescingSpreadMethodResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), qualifier, method, expr1, expr2, arg);

            result.Instance = expr2.Inner.Instance;

            InitExternResult(result);

            return result;
        }

        public EvalCastResult CreateEvalCast(IToken start, IToken stop)
        {
            return new EvalCastResult(Tables, start, stop, Invalids.InvalidType);
        }

        public EvalCastResult CreateEvalCast(IToken start, IToken stop, TeuchiUdonType type, ExprResult expr)
        {
            return new EvalCastResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), expr);
        }

        public EvalTypeOfResult CreateEvalTypeOf(IToken start, IToken stop)
        {
            return new EvalTypeOfResult(Tables, start, stop, Primitives.TypeOf, true);
        }

        public EvalVarCandidateResult CreateEvalVarCandidate(IToken start, IToken stop)
        {
            return new EvalVarCandidateResult(Tables, start, stop, Invalids.InvalidType);
        }

        public EvalVarCandidateResult CreateEvalVarCandidate(IToken start, IToken stop, IdentifierResult identifier)
        {
            return new EvalVarCandidateResult(Tables, start, stop, Primitives.Unknown, false, identifier);
        }

        public EvalArrayIndexerResult CreateEvalArrayIndexer(IToken start, IToken stop)
        {
            return new EvalArrayIndexerResult(Tables, start, stop, Invalids.InvalidType);
        }

        public EvalArrayIndexerResult CreateEvalArrayIndexer
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            TeuchiUdonQualifier qualifier,
            ExprResult expr,
            ExprResult arg
        )
        {
            var result = new EvalArrayIndexerResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), qualifier, expr, arg);

            InitExternResult(result);

            return result;
        }

        public TypeCastResult CreateTypeCast(IToken start, IToken stop)
        {
            return new TypeCastResult(Tables, start, stop, Invalids.InvalidType);
        }

        public TypeCastResult CreateTypeCast(IToken start, IToken stop, TeuchiUdonType type, ExprResult expr, ExprResult arg)
        {
            return new TypeCastResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), expr, arg);
        }

        public ConvertCastResult CreateConvertCast(IToken start, IToken stop)
        {
            return new ConvertCastResult(Tables, start, stop, Invalids.InvalidType);
        }

        public ConvertCastResult CreateConvertCast(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier, ExprResult expr, ExprResult arg)
        {
            var result = new ConvertCastResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), qualifier, expr, arg);

            InitExternResult(result);

            return result;
        }

        public TypeOfResult CreateTypeOf(IToken start, IToken stop)
        {
            return new TypeOfResult(Tables, start, stop, Invalids.InvalidType);
        }

        public TypeOfResult CreateTypeOf(IToken start, IToken stop, TeuchiUdonType type)
        {
            var literal = SyntaxOps.CreateDotNetTypeLiteral(Tables.GetLiteralIndex(), type);

            return new TypeOfResult(Tables, start, stop, Primitives.DotNetType, true, literal);
        }

        public PrefixResult CreatePrefix(IToken start, IToken stop)
        {
            return new PrefixResult(Tables, start, stop, Invalids.InvalidType);
        }

        public PrefixResult CreatePrefix(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier, string op, ExprResult expr)
        {
            var result = new PrefixResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), qualifier, op, expr);

            InitExternResult(result);

            return result;
        }

        public InfixResult CreateInfix(IToken start, IToken stop)
        {
            return new InfixResult(Tables, start, stop, Invalids.InvalidType);
        }

        public InfixResult CreateInfix
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            TeuchiUdonQualifier qualifier,
            string op,
            ExprResult expr1,
            ExprResult expr2
        )
        {
            var result = new InfixResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), qualifier, op, expr1, expr2);

            if (op == "." || op == "?.")
            {
                result.Instance      = expr1;
                expr2.Inner.Instance = expr1;
            }

            InitExternResult(result);

            return result;
        }

        public LetInBindResult CreateLetInBind(IToken start, IToken stop)
        {
            return new LetInBindResult(Tables, start, stop, Invalids.InvalidType);
        }

        public LetInBindResult CreateLetInBind
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            int index,
            KeywordResult letKeyword,
            KeywordResult inKeyword,
            TeuchiUdonQualifier qualifier,
            VarBindResult varBind,
            ExprResult expr
        )
        {
            var letIn = new TeuchiUdonLetIn(index, qualifier);

            return new LetInBindResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), letKeyword, inKeyword, letIn, varBind, expr);
        }

        public IfResult CreateIf(IToken start, IToken stop)
        {
            return new IfResult(Tables, start, stop, Invalids.InvalidType);
        }

        public IfResult CreateIf
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            KeywordResult ifKeyword,
            IEnumerable<KeywordResult> thenKeyword,
            IEnumerable<KeywordResult> elifKeyword,
            IEnumerable<ExprResult> conditions,
            IEnumerable<StatementResult> statements
        )
        {
            var labels =
                Enumerable
                .Range(0, conditions.Count())
                .Select(_ => new TeuchiUdonBranch(Tables.GetBranchIndex()));

            return new IfResult
            (
                Tables,
                start,
                stop,
                type,
                !TypeOps.ContainsUnknown(type),
                ifKeyword,
                thenKeyword,
                elifKeyword,
                conditions,
                statements,
                labels
            );
        }

        public IfElseResult CreateIfElse(IToken start, IToken stop)
        {
            return new IfElseResult(Tables, start, stop, Invalids.InvalidType);
        }

        public IfElseResult CreateIfElse
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            KeywordResult ifKeyword,
            KeywordResult elseKeyword,
            IEnumerable<KeywordResult> thenKeyword,
            IEnumerable<KeywordResult> elifKeyword,
            IEnumerable<ExprResult> conditions,
            IEnumerable<ExprResult> thenParts,
            ExprResult elsePart
        )
        {
            var labels =
                Enumerable
                .Range(0, conditions.Count() + 1)
                .Select(_ => new TeuchiUdonBranch(Tables.GetBranchIndex()))
                .ToArray();

            var result = new IfElseResult
            (
                Tables,
                start,
                stop,
                type,
                !TypeOps.ContainsUnknown(type),
                ifKeyword,
                elseKeyword,
                thenKeyword,
                elifKeyword,
                conditions,
                thenParts,
                elsePart,
                labels
            );

            foreach (var o in result.Children.SelectMany(x => x.ReleasedOutValues))
            {
                OutValuePool.RetainReleasedOutValue(o);
            }

            return result;
        }

        public WhileResult CreateWhile(IToken start, IToken stop)
        {
            return new WhileResult(Tables, start, stop, Invalids.InvalidType);
        }

        public WhileResult CreateWhile
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            KeywordResult whileKeyword,
            KeywordResult doKeyword,
            ExprResult condition,
            ExprResult expr
        )
        {
            var labels = new ICodeLabel[]
            {
                new TeuchiUdonLoop(Tables.GetLoopIndex()),
                new TeuchiUdonLoop(Tables.GetLoopIndex())
            };

            if (expr.Inner is BlockResult block)
            {
                block.Block.Continue = labels[0];
                block.Block.Break    = labels[1];
            }

            return new WhileResult
            (
                Tables,
                start,
                stop,
                type,
                !TypeOps.ContainsUnknown(type),
                whileKeyword,
                doKeyword,
                condition,
                expr,
                labels
            );
        }

        public ForResult CreateFor(IToken start, IToken stop)
        {
            return new ForResult(Tables, start, stop, Invalids.InvalidType);
        }

        public ForResult CreateFor
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            int index,
            IEnumerable<KeywordResult> forKeywords,
            KeywordResult doKeyword,
            IEnumerable<ForBindResult> forBinds,
            ExprResult expr
        )
        {
            var continueLabel = new TeuchiUdonLoop(Tables.GetLoopIndex());

            if (expr.Inner is BlockResult block)
            {
                var forBind = forBinds.FirstOrDefault();
                block.Block.Continue = continueLabel;
                block.Block.Break    =
                    forBind == null ? null :
                    forBind is LetForBindResult    letBind ? letBind.Iter.BreakLabel :
                    forBind is AssignForBindResult assign  ? assign .Iter.BreakLabel :
                        null;
            }

            return new ForResult
            (
                Tables,
                start,
                stop,
                type,
                !TypeOps.ContainsUnknown(type),
                index,
                forKeywords,
                doKeyword,
                forBinds,
                expr,
                continueLabel
            );
        }

        public LoopResult CreateLoop(IToken start, IToken stop)
        {
            return new LoopResult(Tables, start, stop, Invalids.InvalidType);
        }

        public LoopResult CreateLoop(IToken start, IToken stop, TeuchiUdonType type, KeywordResult loopKeyword, ExprResult expr)
        {
            var labels = new ICodeLabel[]
            {
                new TeuchiUdonLoop(Tables.GetLoopIndex()),
                new TeuchiUdonLoop(Tables.GetLoopIndex())
            };

            if (expr.Inner is BlockResult block)
            {
                block.Block.Continue = labels[0];
                block.Block.Break    = labels[1];
            }

            return new LoopResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), loopKeyword, expr, labels);
        }

        public FuncResult CreateFunc(IToken start, IToken stop)
        {
            return new FuncResult(Tables, start, stop, Invalids.InvalidType);
        }

        public FuncResult CreateFunc
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
        )
        {
            var func = new TeuchiUdonFunc(index, qualifier, type, vars, expr, deterministic, Primitives.UInt);

            if (Tables.Funcs.ContainsKey(func))
            {
                LogicalErrorHandler.ReportError(start, $"{func} conflicts with another function");
            }
            else
            {
                Tables.Funcs.Add(func, func);
            }

            if (expr.Inner is BlockResult block)
            {
                block.Block.Return = func.Return;
            }

            return new FuncResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), func, varDecl, expr, deterministic);
        }

        public MethodResult CreateMethod(IToken start, IToken stop)
        {
            return new MethodResult(Tables, start, stop, Invalids.InvalidType);
        }

        public MethodResult CreateMethod(IToken start, IToken stop, TeuchiUdonType type, IdentifierResult identifier)
        {
            return new MethodResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), identifier);
        }

        public ElementsIterExprResult CreateElementsIterExpr(IToken start, IToken stop)
        {
            return new ElementsIterExprResult(Tables, start, stop, Invalids.InvalidType);
        }

        public ElementsIterExprResult CreateElementsIterExpr
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            TeuchiUdonQualifier qualifier,
            IEnumerable<ExprResult> exprs
        )
        {
            var result = new ElementsIterExprResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), qualifier, exprs);

            InitExternResult(result);

            return result;
        }

        public RangeIterExprResult CreateRangeIterExpr(IToken start, IToken stop)
        {
            return new RangeIterExprResult(Tables, start, stop, Invalids.InvalidType);
        }

        public RangeIterExprResult CreateRangeIterExpr
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            TeuchiUdonQualifier qualifier,
            ExprResult first,
            ExprResult last
        )
        {
            var result = new RangeIterExprResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), qualifier, first, last);

            InitExternResult(result);

            return result;
        }

        public SteppedRangeIterExprResult CreateSteppedRangeIterExpr(IToken start, IToken stop)
        {
            return new SteppedRangeIterExprResult(Tables, start, stop, Invalids.InvalidType);
        }

        public SteppedRangeIterExprResult CreateSteppedRangeIterExpr
        (
            IToken start,
            IToken stop,
            TeuchiUdonType type,
            TeuchiUdonQualifier qualifier,
            ExprResult first,
            ExprResult last,
            ExprResult step
        )
        {
            var result = new SteppedRangeIterExprResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), qualifier, first, last, step);

            InitExternResult(result);

            return result;
        }

        public SpreadIterExprResult CreateSpreadIterExpr(IToken start, IToken stop)
        {
            return new SpreadIterExprResult(Tables, start, stop, Invalids.InvalidType);
        }

        public SpreadIterExprResult CreateSpreadIterExpr(IToken start, IToken stop, TeuchiUdonType type, TeuchiUdonQualifier qualifier, ExprResult expr)
        {
            var result = new SpreadIterExprResult(Tables, start, stop, type, !TypeOps.ContainsUnknown(type), qualifier, expr);

            InitExternResult(result);

            return result;
        }

        public IsoExprResult CreateIsoExpr(IToken start, IToken stop)
        {
            return new IsoExprResult(Tables, start, stop);
        }

        public IsoExprResult CreateIsoExpr(IToken start, IToken stop, ExprResult expr)
        {
            return new IsoExprResult(Tables, start, stop, expr);
        }

        public ArgExprResult CreateArgExpr(IToken start, IToken stop)
        {
            return new ArgExprResult(Tables, start, stop);
        }

        public ArgExprResult CreateArgExpr(IToken start, IToken stop, KeywordResult refKeyword, ExprResult expr, bool rf)
        {
            return new ArgExprResult(Tables, start, stop, refKeyword, expr, rf);
        }

        public LetForBindResult CreateLetForBind(IToken start, IToken stop)
        {
            return new LetForBindResult(Tables, start, stop);
        }

        public LetForBindResult CreateLetForBind
        (
            IToken start,
            IToken stop,
            int index,
            KeywordResult letKeyword,
            TeuchiUdonQualifier qualifier,
            IEnumerable<TeuchiUdonVar> vars,
            VarDeclResult varDecl,
            IterExprResult iter
        )
        {
            var varBind = new TeuchiUdonVarBind(index, qualifier, vars.Select(x => x.Name));

            foreach (var v in vars)
            {
                Tables.Vars[v] = v;
            }

            return new LetForBindResult(Tables, start, stop, letKeyword, varBind, vars, varDecl, iter);
        }

        public AssignForBindResult CreateAssignForBind(IToken start, IToken stop)
        {
            return new AssignForBindResult(Tables, start, stop);
        }

        public AssignForBindResult CreateAssignForBind(IToken start, IToken stop, ExprResult expr, IterExprResult iter)
        {
            return new AssignForBindResult(Tables, start, stop, expr, iter);
        }

        private void InitExternResult(ExternResult result)
        {
            result.Methods    = GetMethods(result).ToDictionary(x => x.key, x => x.value);
            result.OutValuess =
                CreateOutValuesForMethods(result) ?
                    result.Methods
                    .Select(x =>
                        (
                            key  : x.Key,
                            value: x.Value == null ?
                                Array.Empty<TeuchiUdonOutValue>() :
                                x.Value.OutTypes.Select(y =>
                                    OutValuePool.RetainOutValue(result.Qualifier.GetFuncQualifier(), y)
                                )
                                .ToArray()
                        )
                    )
                    .ToDictionary(x => x.key, x => x.value)
                    :
                    new Dictionary<string, TeuchiUdonOutValue[]>();
            result.TmpValues =
                GetTmpValues(result)
                .Store(out var outValues)
                .Select(x => x.key)
                .Zip
                (
                    outValues.Select(x =>
                        OutValuePool.RetainOutValue(result.Qualifier.GetFuncQualifier(), x.type)
                    ),
                    (k, v) => (k, v)
                )
                .ToDictionary(x => x.k, x => x.v);
            result.Literals = GetLiterals(result).ToDictionary(x => x.key, x => x.value);
            result.Labels   = GetLabels  (result).ToDictionary(x => x.key, x => x.value);
        }

        
        private IEnumerable<(string key, TeuchiUdonMethod value)> GetMethods(ExternResult result)
        {
            switch (result)
            {
                case ArrayCtorResult arrayCtor:
                if (StaticTables.Types.ContainsKey(arrayCtor.Type))
                {
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "ctor",
                            GetMethodFromName
                            (
                                arrayCtor,
                                new TeuchiUdonType[] { arrayCtor.Type },
                                true,
                                new string[] { "ctor" },
                                new TeuchiUdonType[] { Primitives.Int }
                            )
                        ),
                        (
                            "setter",
                            GetMethodFromName
                            (
                                arrayCtor,
                                new TeuchiUdonType[] { arrayCtor.Type },
                                false,
                                new string[] { "Set" },
                                new TeuchiUdonType[] { arrayCtor.Type, Primitives.Int, arrayCtor.Type.GetArgAsArray() }
                            )
                        ),
                        (
                            "lessThanOrEqual",
                            GetMethodFromName
                            (
                                arrayCtor,
                                new TeuchiUdonType[] { Primitives.Int },
                                true,
                                new string[] { "op_LessThanOrEqual" },
                                new TeuchiUdonType[] { Primitives.Int, Primitives.Int }
                            )
                        ),
                        (
                            "addition",
                            GetMethodFromName
                            (
                                arrayCtor,
                                new TeuchiUdonType[] { Primitives.Int },
                                true,
                                new string[] { "op_Addition" },
                                new TeuchiUdonType[] { Primitives.Int, Primitives.Int }
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
                                arrayCtor,
                                new TeuchiUdonType[] { Primitives.AnyArray },
                                true,
                                new string[] { "ctor" },
                                new TeuchiUdonType[] { Primitives.Int }
                            )
                        ),
                        (
                            "setter",
                            GetMethodFromName
                            (
                                arrayCtor,
                                new TeuchiUdonType[] { Primitives.AnyArray },
                                false,
                                new string[] { "Set" },
                                new TeuchiUdonType[] { Primitives.AnyArray, Primitives.Int, Primitives.Object }
                            )
                        ),
                        (
                            "lessThanOrEqual",
                            GetMethodFromName
                            (
                                arrayCtor,
                                new TeuchiUdonType[] { Primitives.Int },
                                true,
                                new string[] { "op_LessThanOrEqual" },
                                new TeuchiUdonType[] { Primitives.Int, Primitives.Int }
                            )
                        ),
                        (
                            "addition",
                            GetMethodFromName
                            (
                                arrayCtor,
                                new TeuchiUdonType[] { Primitives.Int },
                                true,
                                new string[] { "op_Addition" },
                                new TeuchiUdonType[] { Primitives.Int, Primitives.Int }
                            )
                        )
                    };
                }
                case InterpolatedStringResult interpolatedString:
                {
                    var arrayType = TypeOps.ToArrayType(Primitives.Object);
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "format",
                            GetMethodFromName
                            (
                                interpolatedString,
                                new TeuchiUdonType[] { Primitives.String },
                                true,
                                new string[] { "Format" },
                                new TeuchiUdonType[] { Primitives.String, Primitives.Array.ApplyArgAsArray(Primitives.Object) }
                            )
                        ),
                        (
                            "arrayCtor",
                            GetMethodFromName
                            (
                                interpolatedString,
                                new TeuchiUdonType[] { arrayType },
                                true,
                                new string[] { "ctor" },
                                new TeuchiUdonType[] { Primitives.Int }
                            )
                        ),
                        (
                            "arraySetter",
                            GetMethodFromName
                            (
                                interpolatedString,
                                new TeuchiUdonType[] { arrayType },
                                false,
                                new string[] { "Set" },
                                new TeuchiUdonType[] { arrayType, Primitives.Int, Primitives.Object }
                            )
                        ),
                        (
                            "keyAddition",
                            GetMethodFromName
                            (
                                interpolatedString,
                                new TeuchiUdonType[] { Primitives.Int },
                                true,
                                new string[] { "op_Addition" },
                                new TeuchiUdonType[] { Primitives.Int, Primitives.Int }
                            )
                        )
                    };
                }
                case EvalGetterResult getter:
                    return new (string, TeuchiUdonMethod)[]
                    {
                        ("getter", getter.Getter)
                    };
                case EvalSetterResult setter:
                    return new (string, TeuchiUdonMethod)[]
                    {
                        ("setter", setter.Setter)
                    };
                case EvalGetterSetterResult getterSetter:
                    return new (string, TeuchiUdonMethod)[]
                    {
                        ("getter", getterSetter.Getter),
                        ("setter", getterSetter.Setter)
                    };
                case EvalMethodResult evalMethod:
                    return new (string, TeuchiUdonMethod)[]
                    {
                        ("method", evalMethod.Method)
                    };
                case EvalSpreadMethodResult evalSpreadMethod:
                    return new (string, TeuchiUdonMethod)[]
                    {
                        ("method", evalSpreadMethod.Method)
                    };
                case EvalCoalescingMethodResult evalCoalescingMethod:
                if (evalCoalescingMethod.Expr1.Inner.Type.LogicalTypeEquals(Primitives.NullType))
                {
                    return new (string, TeuchiUdonMethod)[]
                    {
                        ("method", evalCoalescingMethod.Method)
                    };
                }
                else
                {
                    return new (string, TeuchiUdonMethod)[]
                    {
                        ("method", evalCoalescingMethod.Method),
                        (
                            "==",
                            GetMethodFromName
                            (
                                evalCoalescingMethod,
                                new TeuchiUdonType[] { evalCoalescingMethod.Expr1.Inner.Type, Primitives.Object },
                                true,
                                new string[] { "op_Equality" },
                                new TeuchiUdonType[] { evalCoalescingMethod.Expr1.Inner.Type, evalCoalescingMethod.Expr1.Inner.Type }
                            )
                        )
                    };
                }
                case EvalCoalescingSpreadMethodResult evalCoalescingSpreadMethod:
                if (evalCoalescingSpreadMethod.Expr1.Inner.Type.LogicalTypeEquals(Primitives.NullType))
                {
                    return new (string, TeuchiUdonMethod)[]
                    {
                        ("method", evalCoalescingSpreadMethod.Method)
                    };
                }
                else
                {
                    return new (string, TeuchiUdonMethod)[]
                    {
                        ("method", evalCoalescingSpreadMethod.Method),
                        (
                            "==",
                            GetMethodFromName
                            (
                                evalCoalescingSpreadMethod,
                                new TeuchiUdonType[] { evalCoalescingSpreadMethod.Expr1.Inner.Type, Primitives.Object },
                                true,
                                new string[] { "op_Equality" },
                                new TeuchiUdonType[] { evalCoalescingSpreadMethod.Expr1.Inner.Type, evalCoalescingSpreadMethod.Expr1.Inner.Type }
                            )
                        )
                    };
                }
                case EvalArrayIndexerResult evalArrayIndexer:
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "getter",
                            GetMethodFromName
                            (
                                evalArrayIndexer,
                                new TeuchiUdonType[] { evalArrayIndexer.Expr.Inner.Type },
                                false,
                                new string[] { "Get" },
                                new TeuchiUdonType[] { evalArrayIndexer.Expr.Inner.Type, Primitives.Int }
                            )
                        ),
                        (
                            "setter",
                            GetMethodFromName
                            (
                                evalArrayIndexer,
                                new TeuchiUdonType[] { evalArrayIndexer.Expr.Inner.Type },
                                false,
                                new string[] { "Set" },
                                new TeuchiUdonType[] { evalArrayIndexer.Expr.Inner.Type, Primitives.Int, evalArrayIndexer.Expr.Inner.Type.GetArgAsType() }
                            )
                        )
                    };
                case ConvertCastResult convertCast:
                {
                    var methodName = SyntaxOps.GetConvertMethodName(convertCast.Expr.Inner.Type.GetArgAsType());
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "convert",
                            GetMethodFromName
                            (
                                convertCast,
                                new TeuchiUdonType[] { new TeuchiUdonType("SystemConvert") },
                                true,
                                new string[] { methodName },
                                new TeuchiUdonType[] { convertCast.Arg.Inner.Type }
                            )
                        )
                    };
                }
                case PrefixResult prefix:
                {
                    var exprType = prefix.Expr.Inner.Type;
                    switch (prefix.Op)
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
                                        prefix,
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
                                        prefix,
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
                                        prefix,
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
                case InfixResult infix:
                {
                    var expr1Type  = infix.Expr1.Inner.Type;
                    var expr2Type  = infix.Expr2.Inner.Type;
                    var objectType = Primitives.Object;
                    switch (infix.Op)
                    {
                        case ".":
                            return Enumerable.Empty<(string, TeuchiUdonMethod)>();
                        case "?.":
                            if (infix.Expr1.Inner.Type.LogicalTypeEquals(Primitives.NullType))
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
                                            infix,
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
                                        infix,
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
                                        infix,
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
                                        infix,
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
                                        infix,
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
                                        infix,
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
                                        infix,
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
                                        infix,
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
                                    TypeOps.IsAssignableFrom(infix.Expr1.Inner.Type, infix.Expr2.Inner.Type) ?
                                    GetMethodFromName
                                    (
                                        infix,
                                        new TeuchiUdonType[] { expr1Type },
                                        true,
                                        new string[] { "op_LessThan" },
                                        new TeuchiUdonType[] { expr1Type, expr1Type }
                                    ) :
                                    GetMethodFromName
                                    (
                                        infix,
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
                                    TypeOps.IsAssignableFrom(infix.Expr1.Inner.Type, infix.Expr2.Inner.Type) ?
                                    GetMethodFromName
                                    (
                                        infix,
                                        new TeuchiUdonType[] { expr1Type },
                                        true,
                                        new string[] { "op_GreaterThan" },
                                        new TeuchiUdonType[] { expr1Type, expr1Type }
                                    ) :
                                    GetMethodFromName
                                    (
                                        infix,
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
                                    TypeOps.IsAssignableFrom(infix.Expr1.Inner.Type, infix.Expr2.Inner.Type) ?
                                    GetMethodFromName
                                    (
                                        infix,
                                        new TeuchiUdonType[] { expr1Type },
                                        true,
                                        new string[] { "op_LessThanOrEqual" },
                                        new TeuchiUdonType[] { expr1Type, expr1Type }
                                    ) :
                                    GetMethodFromName
                                    (
                                        infix,
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
                                    TypeOps.IsAssignableFrom(infix.Expr1.Inner.Type, infix.Expr2.Inner.Type) ?
                                    GetMethodFromName
                                    (
                                        infix,
                                        new TeuchiUdonType[] { expr1Type },
                                        true,
                                        new string[] { "op_GreaterThanOrEqual" },
                                        new TeuchiUdonType[] { expr1Type, expr1Type }
                                    ) :
                                    GetMethodFromName
                                    (
                                        infix,
                                        new TeuchiUdonType[] { expr2Type },
                                        true,
                                        new string[] { "op_GreaterThanOrEqual" },
                                        new TeuchiUdonType[] { expr2Type, expr2Type }
                                    )
                                )
                            };
                        case "==":
                            if (infix.Expr1.Inner.Type.LogicalTypeEquals(Primitives.NullType) && infix.Expr2.Inner.Type.LogicalTypeEquals(Primitives.NullType))
                            {
                                return Enumerable.Empty<(string, TeuchiUdonMethod)>();
                            }
                            else
                            {
                                return new (string, TeuchiUdonMethod)[]
                                {
                                    (
                                        "op",
                                        TypeOps.IsAssignableFrom(infix.Expr1.Inner.Type, infix.Expr2.Inner.Type) ?
                                        GetMethodFromName
                                        (
                                            infix,
                                            new TeuchiUdonType[] { expr1Type, objectType },
                                            true,
                                            new string[] { "op_Equality" },
                                            new TeuchiUdonType[] { expr1Type, expr1Type }
                                        ) :
                                        GetMethodFromName
                                        (
                                            infix,
                                            new TeuchiUdonType[] { expr2Type, objectType },
                                            true,
                                            new string[] { "op_Equality" },
                                            new TeuchiUdonType[] { expr2Type, expr2Type }
                                        )
                                    )
                                };
                            }
                        case "!=":
                            if (infix.Expr1.Inner.Type.LogicalTypeEquals(Primitives.NullType) && infix.Expr2.Inner.Type.LogicalTypeEquals(Primitives.NullType))
                            {
                                return Enumerable.Empty<(string, TeuchiUdonMethod)>();
                            }
                            else
                            {
                                return new (string, TeuchiUdonMethod)[]
                                {
                                    (
                                        "op",
                                        TypeOps.IsAssignableFrom(infix.Expr1.Inner.Type, infix.Expr2.Inner.Type) ?
                                        GetMethodFromName
                                        (
                                            infix,
                                            new TeuchiUdonType[] { expr1Type, objectType },
                                            true,
                                            new string[] { "op_Inequality" },
                                            new TeuchiUdonType[] { expr1Type, expr1Type }
                                        ) :
                                        GetMethodFromName
                                        (
                                            infix,
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
                                        infix,
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
                                        infix,
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
                                        infix,
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
                            if (infix.Expr1.Inner.Type.LogicalTypeEquals(Primitives.NullType))
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
                                            infix,
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
                case ElementsIterExprResult elementsIterExpr:
                    return Enumerable.Empty<(string, TeuchiUdonMethod)>();
                case RangeIterExprResult rangeIterExpr:
                {
                    var convertMethodName = SyntaxOps.GetConvertMethodName(Primitives.Int);
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "lessThanOrEqual",
                            GetMethodFromName
                            (
                                rangeIterExpr,
                                new TeuchiUdonType[] { rangeIterExpr.Type },
                                true,
                                new string[] { "op_LessThanOrEqual" },
                                new TeuchiUdonType[] { rangeIterExpr.Type, rangeIterExpr.Type }
                            )
                        ),
                        (
                            "greaterThan",
                            GetMethodFromName
                            (
                                rangeIterExpr,
                                new TeuchiUdonType[] { rangeIterExpr.Type },
                                true,
                                new string[] { "op_GreaterThan" },
                                new TeuchiUdonType[] { rangeIterExpr.Type, rangeIterExpr.Type }
                            )
                        ),
                        (
                            "convert",
                            GetMethodFromName
                            (
                                rangeIterExpr,
                                new TeuchiUdonType[] { new TeuchiUdonType("SystemConvert") },
                                true,
                                new string[] { convertMethodName },
                                new TeuchiUdonType[] { rangeIterExpr.Type }
                            )
                        ),
                        (
                            "addition",
                            GetMethodFromName
                            (
                                rangeIterExpr,
                                new TeuchiUdonType[] { rangeIterExpr.Type },
                                true,
                                new string[] { "op_Addition" },
                                new TeuchiUdonType[] { rangeIterExpr.Type, rangeIterExpr.Type }
                            )
                        ),
                        (
                            "subtraction",
                            GetMethodFromName
                            (
                                rangeIterExpr,
                                new TeuchiUdonType[] { rangeIterExpr.Type },
                                true,
                                new string[] { "op_Subtraction" },
                                new TeuchiUdonType[] { rangeIterExpr.Type, rangeIterExpr.Type }
                            )
                        )
                    };
                }
                case SteppedRangeIterExprResult steppedRangeIterExpr:
                {
                    var convertMethodName = SyntaxOps.GetConvertMethodName(Primitives.Int);
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "keyGreaterThan",
                            GetMethodFromName
                            (
                                steppedRangeIterExpr,
                                new TeuchiUdonType[] { Primitives.Int },
                                true,
                                new string[] { "op_GreaterThan" },
                                new TeuchiUdonType[] { Primitives.Int, Primitives.Int }
                            )
                        ),
                        (
                            "equality",
                            GetMethodFromName
                            (
                                steppedRangeIterExpr,
                                new TeuchiUdonType[] { steppedRangeIterExpr.Type },
                                true,
                                new string[] { "op_Equality" },
                                new TeuchiUdonType[] { steppedRangeIterExpr.Type, steppedRangeIterExpr.Type }
                            )
                        ),
                        (
                            "lessThanOrEqual",
                            GetMethodFromName
                            (
                                steppedRangeIterExpr,
                                new TeuchiUdonType[] { steppedRangeIterExpr.Type },
                                true,
                                new string[] { "op_LessThanOrEqual" },
                                new TeuchiUdonType[] { steppedRangeIterExpr.Type, steppedRangeIterExpr.Type }
                            )
                        ),
                        (
                            "greaterThan",
                            GetMethodFromName
                            (
                                steppedRangeIterExpr,
                                new TeuchiUdonType[] { steppedRangeIterExpr.Type },
                                true,
                                new string[] { "op_GreaterThan" },
                                new TeuchiUdonType[] { steppedRangeIterExpr.Type, steppedRangeIterExpr.Type }
                            )
                        ),
                        (
                            "convert",
                            GetMethodFromName
                            (
                                steppedRangeIterExpr,
                                new TeuchiUdonType[] { new TeuchiUdonType("SystemConvert") },
                                true,
                                new string[] { convertMethodName },
                                new TeuchiUdonType[] { steppedRangeIterExpr.Type }
                            )
                        ),
                        (
                            "addition",
                            GetMethodFromName
                            (
                                steppedRangeIterExpr,
                                new TeuchiUdonType[] { steppedRangeIterExpr.Type },
                                true,
                                new string[] { "op_Addition" },
                                new TeuchiUdonType[] { steppedRangeIterExpr.Type, steppedRangeIterExpr.Type }
                            )
                        ),
                        (
                            "subtraction",
                            GetMethodFromName
                            (
                                steppedRangeIterExpr,
                                new TeuchiUdonType[] { steppedRangeIterExpr.Type },
                                true,
                                new string[] { "op_Subtraction" },
                                new TeuchiUdonType[] { steppedRangeIterExpr.Type, steppedRangeIterExpr.Type }
                            )
                        ),
                        (
                            "division",
                            GetMethodFromName
                            (
                                steppedRangeIterExpr,
                                new TeuchiUdonType[] { steppedRangeIterExpr.Type },
                                true,
                                new string[] { "op_Division" },
                                new TeuchiUdonType[] { steppedRangeIterExpr.Type, steppedRangeIterExpr.Type }
                            )
                        )
                    };
                }
                case SpreadIterExprResult spreadIterExpr:
                {
                    var arrayType = TypeOps.ToArrayType(spreadIterExpr.Type);
                    return new (string, TeuchiUdonMethod)[]
                    {
                        (
                            "clone",
                            GetMethodFromName
                            (
                                spreadIterExpr,
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
                                spreadIterExpr,
                                new TeuchiUdonType[] { arrayType },
                                false,
                                new string[] { "Get" },
                                new TeuchiUdonType[] { arrayType, Primitives.Int }
                            )
                        ),
                        (
                            "getLength",
                            GetMethodFromName
                            (
                                spreadIterExpr,
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
                                spreadIterExpr,
                                new TeuchiUdonType[] { Primitives.Int },
                                true,
                                new string[] { "op_LessThan" },
                                new TeuchiUdonType[] { Primitives.Int, Primitives.Int }
                            )
                        ),
                        (
                            "greaterThanOrEqual",
                            GetMethodFromName
                            (
                                spreadIterExpr,
                                new TeuchiUdonType[] { Primitives.Int },
                                true,
                                new string[] { "op_GreaterThanOrEqual" },
                                new TeuchiUdonType[] { Primitives.Int, Primitives.Int }
                            )
                        ),
                        (
                            "addition",
                            GetMethodFromName
                            (
                                spreadIterExpr,
                                new TeuchiUdonType[] { Primitives.Int },
                                true,
                                new string[] { "op_Addition" },
                                new TeuchiUdonType[] { Primitives.Int, Primitives.Int }
                            )
                        )
                    };
                }
                default:
                    throw new NotSupportedException("unsupported parser result type");
            }
        }

        private bool CreateOutValuesForMethods(ExternResult result)
        {
            switch (result)
            {
                case EvalGetterResult _:
                case EvalSetterResult _:
                case EvalGetterSetterResult _:
                case EvalMethodResult _:
                case EvalSpreadMethodResult _:
                case EvalCoalescingMethodResult _:
                case EvalCoalescingSpreadMethodResult _:
                case EvalArrayIndexerResult _:
                case ConvertCastResult _:
                case PrefixResult _:
                case InfixResult _:
                    return true;
                case ArrayCtorResult _:
                case InterpolatedStringResult _:
                case ElementsIterExprResult _:
                case RangeIterExprResult _:
                case SteppedRangeIterExprResult _:
                case SpreadIterExprResult _:
                    return false;
                default:
                    throw new NotSupportedException("unsupported parser result type");
            }
        }

        private IEnumerable<(string key, TeuchiUdonType type)> GetTmpValues(ExternResult result)
        {
            switch (result)
            {
                case ArrayCtorResult arrayCtor:
                    return new (string, TeuchiUdonType)[]
                    {
                        ("array", arrayCtor.Type),
                        ("key"  , Primitives.Int),
                        ("limit", Primitives.Int)
                    };
                case InterpolatedStringResult _:
                    return new (string, TeuchiUdonType)[]
                    {
                        ("array", TypeOps.ToArrayType(Primitives.Object)),
                        ("key"  , Primitives.Int),
                        ("out"  , Primitives.String)
                    };
                case EvalGetterResult _:
                case EvalSetterResult _:
                case EvalGetterSetterResult _:
                case EvalMethodResult _:
                case EvalSpreadMethodResult _:
                    return Enumerable.Empty<(string, TeuchiUdonType)>();
                case EvalCoalescingMethodResult evalCoalescingMethod:
                    return new (string, TeuchiUdonType)[] { ("tmp", evalCoalescingMethod.Expr1.Inner.Type) };
                case EvalCoalescingSpreadMethodResult evalCoalescingSpreadMethod:
                    return new (string, TeuchiUdonType)[] { ("tmp", evalCoalescingSpreadMethod.Expr1.Inner.Type) };
                case EvalArrayIndexerResult _:
                case ConvertCastResult _:
                case PrefixResult _:
                    return Enumerable.Empty<(string, TeuchiUdonType)>();
                case InfixResult infix:
                    switch (infix.Op)
                    {
                        case "?.":
                        case "??":
                            return new (string, TeuchiUdonType)[] { ("tmp", infix.Type) };
                        default:
                            return Enumerable.Empty<(string, TeuchiUdonType)>();
                    }
                case ElementsIterExprResult _:
                    return Enumerable.Empty<(string, TeuchiUdonType)>();
                case RangeIterExprResult rangeIterExpr:
                    return new (string, TeuchiUdonType)[]
                    {
                        ("value"      , rangeIterExpr.Type),
                        ("limit"      , rangeIterExpr.Type),
                        ("condition"  , Primitives.Bool),
                        ("length"     , Primitives.Int),
                        ("valueLength", rangeIterExpr.Type),
                    };
                case SteppedRangeIterExprResult steppedRangeIterExpr:
                    return new (string, TeuchiUdonType)[]
                    {
                        ("value"      , steppedRangeIterExpr.Type),
                        ("limit"      , steppedRangeIterExpr.Type),
                        ("step"       , steppedRangeIterExpr.Type),
                        ("isUpTo"     , Primitives.Bool),
                        ("condition"  , Primitives.Bool),
                        ("length"     , Primitives.Int),
                        ("valueLength", steppedRangeIterExpr.Type),
                    };
                case SpreadIterExprResult spreadIterExpr:
                    return new (string, TeuchiUdonType)[]
                    {
                        ("array"      , TypeOps.ToArrayType(spreadIterExpr.Type)),
                        ("key"        , Primitives.Int),
                        ("value"      , spreadIterExpr.Type),
                        ("length"     , Primitives.Int),
                        ("condition"  , Primitives.Bool)
                    };
                default:
                    throw new NotSupportedException("unsupported parser result type");
            }
        }

        private IEnumerable<(string key, TeuchiUdonLiteral value)> GetLiterals(ExternResult result)
        {
            switch (result)
            {
                case ArrayCtorResult _:
                    return new (string, TeuchiUdonLiteral)[]
                    {
                        ("0", SyntaxOps.CreateValueLiteral(Tables.GetLiteralIndex(), "0", Primitives.Int)),
                        ("1", SyntaxOps.CreateValueLiteral(Tables.GetLiteralIndex(), "1", Primitives.Int))
                    };
                case InterpolatedStringResult interpolatedString:
                    return new (string, TeuchiUdonLiteral)[]
                    {
                        ("0"     , SyntaxOps.CreateValueLiteral(Tables.GetLiteralIndex(), "0"                                       , Primitives.Int)),
                        ("1"     , SyntaxOps.CreateValueLiteral(Tables.GetLiteralIndex(), "1"                                       , Primitives.Int)),
                        ("length", SyntaxOps.CreateValueLiteral(Tables.GetLiteralIndex(), interpolatedString.Exprs.Length.ToString(), Primitives.Int))
                    };
                case EvalGetterResult _:
                case EvalSetterResult _:
                case EvalGetterSetterResult _:
                case EvalMethodResult _:
                case EvalSpreadMethodResult _:
                    return Enumerable.Empty<(string, TeuchiUdonLiteral)>();
                case EvalCoalescingMethodResult _:
                case EvalCoalescingSpreadMethodResult _:
                    return new (string, TeuchiUdonLiteral)[]
                    {
                        ("null", SyntaxOps.CreateValueLiteral(Tables.GetLiteralIndex(), "null", Primitives.NullType))
                    };
                case EvalArrayIndexerResult _:
                case ConvertCastResult _:
                    return Enumerable.Empty<(string, TeuchiUdonLiteral)>();
                case PrefixResult prefix:
                    switch (prefix.Op)
                    {
                        case "~":
                            return new (string, TeuchiUdonLiteral)[]
                            {
                                ("mask", SyntaxOps.CreateMaskLiteral(Tables.GetLiteralIndex(), prefix.Expr.Inner.Type))
                            };
                        default:
                            return Enumerable.Empty<(string, TeuchiUdonLiteral)>();
                    }
                case InfixResult infix:
                    switch (infix.Op)
                    {
                        case "==":
                            return new (string, TeuchiUdonLiteral)[]
                            {
                                ("true", SyntaxOps.CreateValueLiteral(Tables.GetLiteralIndex(), "true", Primitives.Bool))
                            };
                        case "!=":
                            return new (string, TeuchiUdonLiteral)[]
                            {
                                ("false", SyntaxOps.CreateValueLiteral(Tables.GetLiteralIndex(), "false", Primitives.Bool))
                            };
                        case "&&":
                            return new (string, TeuchiUdonLiteral)[]
                            {
                                ("false", SyntaxOps.CreateValueLiteral(Tables.GetLiteralIndex(), "false", Primitives.Bool))
                            };
                        case "||":
                            return new (string, TeuchiUdonLiteral)[]
                            {
                                ("true", SyntaxOps.CreateValueLiteral(Tables.GetLiteralIndex(), "true", Primitives.Bool))
                            };
                        case "?.":
                        case "??":
                            return new (string, TeuchiUdonLiteral)[]
                            {
                                ("null", SyntaxOps.CreateValueLiteral(Tables.GetLiteralIndex(), "null", Primitives.NullType))
                            };
                        default:
                            return Enumerable.Empty<(string, TeuchiUdonLiteral)>();
                    }
                case ElementsIterExprResult elementsIterExpr:
                    return new (string, TeuchiUdonLiteral)[]
                    {
                        ("length", SyntaxOps.CreateValueLiteral(Tables.GetLiteralIndex(), elementsIterExpr.Exprs.Length.ToString(), Primitives.Int))
                    };
                case RangeIterExprResult rangeIterExpr:
                    return new (string, TeuchiUdonLiteral)[]
                    {
                        ("step", SyntaxOps.CreateValueLiteral(Tables.GetLiteralIndex(), "1", rangeIterExpr.Type))
                    };
                case SteppedRangeIterExprResult steppedRangeIterExpr:
                    return new (string, TeuchiUdonLiteral)[]
                    {
                        ("0", SyntaxOps.CreateValueLiteral(Tables.GetLiteralIndex(), "0", steppedRangeIterExpr.Type))
                    };
                case SpreadIterExprResult spreadIterExpr:
                    return new (string, TeuchiUdonLiteral)[]
                    {
                        ("0", SyntaxOps.CreateValueLiteral(Tables.GetLiteralIndex(), "0", spreadIterExpr.Type)),
                        ("1", SyntaxOps.CreateValueLiteral(Tables.GetLiteralIndex(), "1", spreadIterExpr.Type))
                    };
                default:
                    throw new NotSupportedException("unsupported parser result type");
            }
        }

        private IEnumerable<(string key, ICodeLabel value)> GetLabels(ExternResult result)
        {
            switch (result)
            {
                case ArrayCtorResult _:
                case InterpolatedStringResult _:
                case EvalGetterResult _:
                case EvalSetterResult _:
                case EvalGetterSetterResult _:
                case EvalMethodResult _:
                case EvalSpreadMethodResult _:
                    return Enumerable.Empty<(string, ICodeLabel)>();
                case EvalCoalescingMethodResult _:
                case EvalCoalescingSpreadMethodResult _:
                    return new (string, ICodeLabel)[]
                    {
                        ("1", new TeuchiUdonBranch(Tables.GetBranchIndex())),
                        ("2", new TeuchiUdonBranch(Tables.GetBranchIndex()))
                    };
                case EvalArrayIndexerResult _:
                case ConvertCastResult _:
                case PrefixResult _:
                    return Enumerable.Empty<(string, ICodeLabel)>();
                case InfixResult infix:
                    switch (infix.Op)
                    {
                        case "?.":
                        case "&&":
                        case "||":
                        case "??":
                            return new (string, ICodeLabel)[]
                            {
                                ("1", new TeuchiUdonBranch(Tables.GetBranchIndex())),
                                ("2", new TeuchiUdonBranch(Tables.GetBranchIndex()))
                            };
                        default:
                            return Enumerable.Empty<(string, ICodeLabel)>();
                    }
                case ElementsIterExprResult _:
                    return Enumerable.Empty<(string, ICodeLabel)>();
                case RangeIterExprResult _:
                    return new (string, ICodeLabel)[]
                    {
                        ("branch1", new TeuchiUdonBranch(Tables.GetBranchIndex())),
                        ("branch2", new TeuchiUdonBranch(Tables.GetBranchIndex())),
                        ("loop1"  , new TeuchiUdonLoop  (Tables.GetLoopIndex())),
                        ("loop2"  , new TeuchiUdonLoop  (Tables.GetLoopIndex()))
                    };
                case SteppedRangeIterExprResult _:
                    return new (string, ICodeLabel)[]
                    {
                        ("branch1", new TeuchiUdonBranch(Tables.GetBranchIndex())),
                        ("branch2", new TeuchiUdonBranch(Tables.GetBranchIndex())),
                        ("branch3", new TeuchiUdonBranch(Tables.GetBranchIndex())),
                        ("branch4", new TeuchiUdonBranch(Tables.GetBranchIndex())),
                        ("branch5", new TeuchiUdonBranch(Tables.GetBranchIndex())),
                        ("branch6", new TeuchiUdonBranch(Tables.GetBranchIndex())),
                        ("branch7", new TeuchiUdonBranch(Tables.GetBranchIndex())),
                        ("branch8", new TeuchiUdonBranch(Tables.GetBranchIndex())),
                        ("loop1"  , new TeuchiUdonLoop  (Tables.GetLoopIndex())),
                        ("loop2"  , new TeuchiUdonLoop  (Tables.GetLoopIndex()))
                    };
                case SpreadIterExprResult _:
                    return new (string, ICodeLabel)[]
                    {
                        ("loop1", new TeuchiUdonLoop(Tables.GetLoopIndex())),
                        ("loop2", new TeuchiUdonLoop(Tables.GetLoopIndex()))
                    };
                default:
                    throw new NotSupportedException("unsupported parser result type");
            }
        }

        private TeuchiUdonMethod GetMethodFromName
        (
            ExternResult result,
            IEnumerable<TeuchiUdonType> types,
            bool isTypeType,
            IEnumerable<string> methodNames,
            IEnumerable<TeuchiUdonType> inTypes
        )
        {
            foreach (var t in types)
            {
                var type = isTypeType ? Primitives.Type.ApplyArgAsType(t) : t;

                foreach (var name in methodNames)
                {
                    var qm = new TeuchiUdonMethod(type, name, inTypes);
                    var m  = TableOps.GetMostCompatibleMethods(qm).ToArray();
                    if (m.Length == 0)
                    {
                    }
                    else if (m.Length == 1)
                    {
                        return m[0];
                    }
                    else
                    {
                        LogicalErrorHandler.ReportError(result.Start, $"method '{name}' has multiple overloads");
                        return null;
                    }
                }
            }

            LogicalErrorHandler.ReportError(result.Start, $"method is not defined");
            return null;
        }

        public bool Deterministic(TypedResult result)
        {
            switch (result)
            {
                case EvalFuncResult evalFunc:
                    return !TypeOps.ContainsNonDetFunc(evalFunc.Expr.Inner.Type) && evalFunc.Children.All(x => Deterministic(x));
                case EvalSpreadFuncResult evalSpreadFunc:
                    return !TypeOps.ContainsNonDetFunc(evalSpreadFunc.Expr.Inner.Type) && evalSpreadFunc.Children.All(x => Deterministic(x));
                case FuncResult func:
                    return func.IsDet;
                default:
                    return result.Children.All(x => Deterministic(x));
            }
        }

        public void BindType(TypedResult result, TeuchiUdonType type)
        {
            switch (result)
            {
                case ParenResult paren:
                    if (paren.TypeBound || TypeOps.ContainsUnknown(type)) return;
                    BindType(paren.Expr.Inner, type);
                    paren.Type      = TypeOps.Fix(paren.Type, type);
                    paren.TypeBound = true;
                    return;
                case TupleResult tuple:
                    if (tuple.TypeBound || TypeOps.ContainsUnknown(type)) return;
                    if (!type.LogicalTypeNameEquals(Primitives.Tuple)) return;
                    foreach (var x in tuple.Exprs.Zip(type.GetArgsAsTuple(), (e, t) => (e, t))) BindType(x.e.Inner, x.t);
                    tuple.Type      = TypeOps.Fix(tuple.Type, type);
                    tuple.TypeBound = true;
                    return;
                case ArrayCtorResult arrayCtor:
                    if (arrayCtor.TypeBound || TypeOps.ContainsUnknown(type)) return;
                    arrayCtor.Type = TypeOps.Fix(arrayCtor.Type, type);
                    InitExternResult(arrayCtor);
                    arrayCtor.TypeBound = true;
                    return;
                case LetInBindResult letInBind:
                    if (letInBind.TypeBound || TypeOps.ContainsUnknown(type)) return;
                    BindType(letInBind.Expr.Inner, type);
                    letInBind.Type      = TypeOps.Fix(letInBind.Type, type);
                    letInBind.TypeBound = true;
                    return;
                default:
                    return;
            }
        }
    }
}
