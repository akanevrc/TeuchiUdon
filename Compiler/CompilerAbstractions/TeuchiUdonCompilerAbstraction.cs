using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Compiler
{
    public abstract class TeuchiUdonCompilerAbstraction
    {
        private TeuchiUdonPrimitives Primitives { get; }
        private TeuchiUdonStaticTables StaticTables { get; }
        private TeuchiUdonTables Tables { get; }
        private TeuchiUdonTypeOps TypeOps { get; }
        private TeuchiUdonLabelOps LabelOps { get; }
        private TeuchiUdonOutValuePool OutValuePool { get; }

        public TeuchiUdonCompilerAbstraction
        (
            TeuchiUdonPrimitives primitives,
            TeuchiUdonStaticTables staticTables,
            TeuchiUdonTables tables,
            TeuchiUdonTypeOps typeOps,
            TeuchiUdonLabelOps labelOps,
            TeuchiUdonOutValuePool outValuePool
        )
        {
            Primitives   = primitives;
            StaticTables = staticTables;
            Tables       = tables;
            TypeOps      = typeOps;
            LabelOps     = labelOps;
            OutValuePool = outValuePool;
        }

        protected abstract IEnumerable<TeuchiUdonAssembly> Debug(IEnumerable<TeuchiUdonAssembly> asm);
        protected abstract IEnumerable<TeuchiUdonAssembly> ExportData(IDataLabel label);
        protected abstract IEnumerable<TeuchiUdonAssembly> SyncData(IDataLabel label, TeuchiUdonSyncMode mode);
        protected abstract IEnumerable<TeuchiUdonAssembly> DeclData(IDataLabel label, TeuchiUdonAssemblyLiteral literal);
        protected abstract IEnumerable<TeuchiUdonAssembly> Pop(TeuchiUdonType type);
        protected abstract IEnumerable<TeuchiUdonAssembly> Get(IDataLabel label);
        protected abstract IEnumerable<TeuchiUdonAssembly> Set(IDataLabel label);
        protected abstract IEnumerable<TeuchiUdonAssembly> Jump(ITeuchiUdonLabel label);
        protected abstract IEnumerable<TeuchiUdonAssembly> Indirect(ICodeLabel label);
        protected abstract IEnumerable<TeuchiUdonAssembly> Func(TeuchiUdonFunc func);
        protected abstract IEnumerable<TeuchiUdonAssembly> Event(string varName, string eventName, List<TopStatementResult> stats);
        protected abstract IEnumerable<TeuchiUdonAssembly> EvalFunc
        (
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> args,
            ICodeLabel evalFunc,
            IDataLabel funcAddress
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> EvalMethod
        (
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> inValues,
            IEnumerable<IDataLabel> outValues,
            TeuchiUdonMethod method
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> CallMethod
        (
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> inValues,
            IEnumerable<IDataLabel> outValues,
            TeuchiUdonMethod method
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> EvalAssign(IEnumerable<TeuchiUdonAssembly> value1, IEnumerable<TeuchiUdonAssembly> value2);
        protected abstract IEnumerable<TeuchiUdonAssembly> EvalSetterAssign
        (
            IEnumerable<TeuchiUdonAssembly> instance,
            IEnumerable<TeuchiUdonAssembly> value2,
            TeuchiUdonMethod setterMethod
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> EvalArraySetterAssign
        (
            IEnumerable<TeuchiUdonAssembly> instance,
            IEnumerable<TeuchiUdonAssembly> key,
            IEnumerable<TeuchiUdonAssembly> value2,
            TeuchiUdonMethod setterMethod
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> IfElse
        (
            IEnumerable<TeuchiUdonAssembly> condition,
            IEnumerable<TeuchiUdonAssembly> thenPart,
            IEnumerable<TeuchiUdonAssembly> elsePart,
            ICodeLabel label1,
            ICodeLabel label2
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> IfElif
        (
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> conditions,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> thenParts,
            IEnumerable<ICodeLabel> labels,
            IEnumerable<TeuchiUdonType> types
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> IfElifElse
        (
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> conditions,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> thenParts,
            IEnumerable<TeuchiUdonAssembly> elsePart,
            IEnumerable<ICodeLabel> labels
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> While
        (
            IEnumerable<TeuchiUdonAssembly> condition,
            IEnumerable<TeuchiUdonAssembly> expr,
            ICodeLabel label1,
            ICodeLabel label2,
            TeuchiUdonType type
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> For
        (
            IEnumerable<Func<IEnumerable<TeuchiUdonAssembly>, IEnumerable<TeuchiUdonAssembly>>> forIters,
            IEnumerable<TeuchiUdonAssembly> expr,
            ICodeLabel continueLabel,
            TeuchiUdonType type
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> Loop
        (
            IEnumerable<TeuchiUdonAssembly> expr,
            ICodeLabel label1,
            ICodeLabel label2,
            TeuchiUdonType type
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> RangeIter
        (
            IEnumerable<TeuchiUdonAssembly> firstExpr,
            IEnumerable<TeuchiUdonAssembly> lastExpr,
            TeuchiUdonLiteral step,
            IDataLabel value,
            IDataLabel limit,
            IDataLabel condition,
            TeuchiUdonMethod lessThanOrEqual,
            TeuchiUdonMethod greaterThan,
            TeuchiUdonMethod addition,
            ICodeLabel loopLabel1,
            ICodeLabel loopLabel2,
            IEnumerable<TeuchiUdonAssembly> initPart,
            IEnumerable<TeuchiUdonAssembly> bodyPart,
            IEnumerable<TeuchiUdonAssembly> termPart
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> SteppedRangeIter
        (
            IEnumerable<TeuchiUdonAssembly> firstExpr,
            IEnumerable<TeuchiUdonAssembly> lastExpr,
            IEnumerable<TeuchiUdonAssembly> stepExpr,
            TeuchiUdonLiteral zero,
            IDataLabel value,
            IDataLabel limit,
            IDataLabel step,
            IDataLabel isUpTo,
            IDataLabel condition,
            TeuchiUdonMethod equality,
            TeuchiUdonMethod lessThanOrEqual,
            TeuchiUdonMethod greaterThan,
            TeuchiUdonMethod addition,
            ICodeLabel branchLabel1,
            ICodeLabel branchLabel2,
            ICodeLabel branchLabel3,
            ICodeLabel branchLabel4,
            ICodeLabel branchLabel5,
            ICodeLabel branchLabel6,
            ICodeLabel loopLabel1,
            ICodeLabel loopLabel2,
            IEnumerable<TeuchiUdonAssembly> initPart,
            IEnumerable<TeuchiUdonAssembly> bodyPart,
            IEnumerable<TeuchiUdonAssembly> termPart,
            IEnumerable<TeuchiUdonAssembly> zeroPart
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> SpreadIter
        (
            IEnumerable<TeuchiUdonAssembly> expr,
            TeuchiUdonLiteral zero,
            TeuchiUdonLiteral one,
            IDataLabel array,
            IDataLabel key,
            IDataLabel value,
            IDataLabel length,
            IDataLabel condition,
            TeuchiUdonMethod getter,
            TeuchiUdonMethod getLength,
            TeuchiUdonMethod lessThan,
            TeuchiUdonMethod greaterThanOrEqual,
            TeuchiUdonMethod addition,
            ICodeLabel loopLabel1,
            ICodeLabel loopLabel2,
            IEnumerable<TeuchiUdonAssembly> initPart,
            IEnumerable<TeuchiUdonAssembly> bodyPart,
            IEnumerable<TeuchiUdonAssembly> termPart
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> EmptyArrayCtor
        (
            TeuchiUdonLiteral zero,
            TeuchiUdonOutValue array,
            TeuchiUdonMethod ctor
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> ElementsArrayCtor
        (
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> elements,
            TeuchiUdonLiteral zero,
            TeuchiUdonLiteral one,
            TeuchiUdonLiteral length,
            TeuchiUdonOutValue array,
            TeuchiUdonOutValue key,
            TeuchiUdonMethod ctor,
            TeuchiUdonMethod setter,
            TeuchiUdonMethod addition
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> RangeArrayCtor
        (
            IEnumerable<TeuchiUdonAssembly> firstExpr,
            IEnumerable<TeuchiUdonAssembly> lastExpr,
            TeuchiUdonLiteral zero,
            TeuchiUdonLiteral one,
            TeuchiUdonLiteral step,
            TeuchiUdonOutValue value,
            TeuchiUdonOutValue valueLimit,
            TeuchiUdonOutValue condition,
            TeuchiUdonOutValue length,
            TeuchiUdonOutValue valueLength,
            TeuchiUdonOutValue array,
            TeuchiUdonOutValue key,
            TeuchiUdonMethod ctor,
            TeuchiUdonMethod setter,
            TeuchiUdonMethod valueLessThanOrEqual,
            TeuchiUdonMethod valueGreaterThan,
            TeuchiUdonMethod valueToKey,
            TeuchiUdonMethod keyAddition,
            TeuchiUdonMethod valueAddition,
            TeuchiUdonMethod valueSubtraction,
            ICodeLabel branchLabel1,
            ICodeLabel branchLabel2,
            ICodeLabel loopLabel1,
            ICodeLabel loopLabel2,
            TeuchiUdonType type
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> SteppedRangeArrayCtor
        (
            IEnumerable<TeuchiUdonAssembly> firstExpr,
            IEnumerable<TeuchiUdonAssembly> lastExpr,
            IEnumerable<TeuchiUdonAssembly> stepExpr,
            TeuchiUdonLiteral zero,
            TeuchiUdonLiteral one,
            TeuchiUdonLiteral valueZero,
            TeuchiUdonOutValue value,
            TeuchiUdonOutValue valueLimit,
            TeuchiUdonOutValue valueStep,
            TeuchiUdonOutValue isUpTo,
            TeuchiUdonOutValue condition,
            TeuchiUdonOutValue length,
            TeuchiUdonOutValue valueLength,
            TeuchiUdonOutValue array,
            TeuchiUdonOutValue key,
            TeuchiUdonMethod ctor,
            TeuchiUdonMethod setter,
            TeuchiUdonMethod keyGreaterThan,
            TeuchiUdonMethod valueEquality,
            TeuchiUdonMethod valueLessThanOrEqual,
            TeuchiUdonMethod valueGreaterThan,
            TeuchiUdonMethod valueToKey,
            TeuchiUdonMethod keyAddition,
            TeuchiUdonMethod valueAddition,
            TeuchiUdonMethod valueSubtraction,
            TeuchiUdonMethod valueDivision,
            ICodeLabel branchLabel1,
            ICodeLabel branchLabel2,
            ICodeLabel branchLabel3,
            ICodeLabel branchLabel4,
            ICodeLabel branchLabel5,
            ICodeLabel branchLabel6,
            ICodeLabel branchLabel7,
            ICodeLabel branchLabel8,
            ICodeLabel loopLabel1,
            ICodeLabel loopLabel2,
            TeuchiUdonType type
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> SpreadArrayCtor
        (
            IEnumerable<TeuchiUdonAssembly> expr,
            TeuchiUdonOutValue array,
            TeuchiUdonMethod clone
        );

        public IEnumerable<TeuchiUdonAssembly> GetDataPartFromTables()
        {
            return
                        Tables.PublicVars.Keys.SelectMany(x => ExportData(x))
                .Concat(Tables.SyncedVars     .SelectMany(x => SyncData(x.Key, x.Value)))
                .Concat(Tables.Vars.Values    .SelectMany(x => DeclData(x, new AssemblyLiteral_NULL())))
                .Concat(Tables.Literals.Values.SelectMany(x => DeclData(x, new AssemblyLiteral_NULL())))
                .Concat(Tables.This.Values    .SelectMany(x => DeclData(x, new AssemblyLiteral_THIS())))
                .Concat(Tables.Funcs.Values   .SelectMany(x => DeclData(x.Return, new AssemblyLiteral_NULL())));
        }

        public IEnumerable<TeuchiUdonAssembly> GetDataPartFromOutValuePool()
        {
            return OutValuePool.OutValues.Values.SelectMany(x => DeclData(x, new AssemblyLiteral_NULL()));
        }

        public IEnumerable<TeuchiUdonAssembly> GetCodePartFromTables()
        {
            return VisitTables();
        }

        public IEnumerable<TeuchiUdonAssembly> GetCodePartFromResult(TargetResult result)
        {
            return VisitBody(result.Body);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitTables()
        {
            return
                Tables.Funcs.Values.Count == 0 ? Enumerable.Empty<TeuchiUdonAssembly>() :
                Tables.Funcs.Values.Select(x => Func(x))
                .Aggregate((acc, x) => acc
                    .Concat(new TeuchiUdonAssembly[] { new Assembly_NEW_LINE() })
                    .Concat(x)
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitTopStatement(TopStatementResult result)
        {
            switch (result)
            {
                case TopBindResult topBind: return VisitTopBind(topBind);
                case TopExprResult topExpr: return VisitTopExpr(topExpr);
                default: throw new InvalidOperationException("unsupported parser result type");
            }
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitStatement(StatementResult result)
        {
            switch (result)
            {
                case JumpResult    jump   : return VisitJump   (jump);
                case LetBindResult letBind: return VisitLetBind(letBind);
                case ExprResult    expr   : return VisitExpr   (expr);
                default: throw new NotSupportedException("unsupported parser result type");
            }
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitTyped(TypedResult result)
        {
            switch (result)
            {
                case InvalidResult                    invalid                   : return VisitInvalid                   (invalid);
                case UnknownTypeResult                unknownType               : return VisitUnknownType               (unknownType);
                case UnitResult                       unit                      : return VisitUnit                      (unit);
                case BlockResult                      block                     : return VisitBlock                     (block);
                case ParenResult                      paren                     : return VisitParen                     (paren);
                case TupleResult                      tuple                     : return VisitTuple                     (tuple);
                case ArrayCtorResult                  arrayCtor                 : return VisitArrayCtor                 (arrayCtor);
                case LiteralResult                    literal                   : return VisitLiteral                   (literal);
                case ThisResult                       this_                     : return VisitThis                      (this_);
                case InterpolatedStringResult         interpolatedString        : return VisitInterpolatedString        (interpolatedString);
                case EvalVarResult                    evalVar                   : return VisitEvalVar                   (evalVar);
                case EvalTypeResult                   evalType                  : return VisitEvalType                  (evalType);
                case EvalQualifierResult              evalQualifier             : return VisitEvalQualifier             (evalQualifier);
                case EvalGetterResult                 evalGetter                : return VisitEvalGetter                (evalGetter);
                case EvalSetterResult                 evalSetter                : return VisitEvalSetter                (evalSetter);
                case EvalGetterSetterResult           evalGetterSetter          : return VisitEvalGetterSetter          (evalGetterSetter);
                case EvalFuncResult                   evalFunc                  : return VisitEvalFunc                  (evalFunc);
                case EvalSpreadFuncResult             evalSpreadFunc            : return VisitEvalSpreadFunc            (evalSpreadFunc);
                case EvalMethodResult                 evalMethod                : return VisitEvalMethod                (evalMethod);
                case EvalSpreadMethodResult           evalSpreadMethod          : return VisitEvalSpreadMethod          (evalSpreadMethod);
                case EvalCoalescingMethodResult       evalCoalescingMethod      : return VisitEvalCoalescingMethod      (evalCoalescingMethod);
                case EvalCoalescingSpreadMethodResult evalCoalescingSpreadMethod: return VisitEvalCoalescingSpreadMethod(evalCoalescingSpreadMethod);
                case EvalCastResult                   evalCast                  : return VisitEvalCast                  (evalCast);
                case EvalTypeOfResult                 evalTypeOf                : return VisitEvalTypeOf                (evalTypeOf);
                case EvalArrayIndexerResult           evalArrayIndexer          : return VisitEvalArrayIndexer          (evalArrayIndexer);
                case TypeCastResult                   typeCast                  : return VisitTypeCast                  (typeCast);
                case TypeOfResult                     typeOf                    : return VisitTypeOf                    (typeOf);
                case ConvertCastResult                convertCast               : return VisitConvertCast               (convertCast);
                case PrefixResult                     prefix                    : return VisitPrefix                    (prefix);
                case InfixResult                      infix                     : return VisitInfix                     (infix);
                case LetInBindResult                  letInBind                 : return VisitLetInBind                 (letInBind);
                case IfResult                         if_                       : return VisitIf                        (if_);
                case IfElseResult                     ifElse                    : return VisitIfElse                    (ifElse);
                case WhileResult                      while_                    : return VisitWhile                     (while_);
                case ForResult                        for_                      : return VisitFor                       (for_);
                case LoopResult                       loop                      : return VisitLoop                      (loop);
                case FuncResult                       func                      : return VisitFunc                      (func);
                case MethodResult                     method                    : return VisitMethod                    (method);
                default: throw new NotSupportedException("unsupported parser result type");
            }
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitArrayIter(ArrayCtorResult ctor, IterExprResult result)
        {
            switch (result)
            {
                case ElementsIterExprResult     elementsIter    : return VisitArrayElementsIter    (ctor, elementsIter);
                case RangeIterExprResult        rangeIter       : return VisitArrayRangeIter       (ctor, rangeIter);
                case SteppedRangeIterExprResult steppedRangeIter: return VisitArraySteppedRangeIter(ctor, steppedRangeIter);
                case SpreadIterExprResult       spreadIter      : return VisitArraySpreadIter      (ctor, spreadIter);
                default: throw new NotSupportedException("unsupported parser result type");
            }
        }

        protected Func<IEnumerable<TeuchiUdonAssembly>, IEnumerable<TeuchiUdonAssembly>> VisitForBindForSetter(ForBindResult result)
        {
            switch (result)
            {
                case LetForBindResult    letBind: return VisitLetForBindForSetter   (letBind);
                case AssignForBindResult assign : return VisitAssignForBindForSetter(assign);
                default: throw new NotSupportedException("unsupported parser result type");
            }
        }

        protected Func<IEnumerable<TeuchiUdonAssembly>, IEnumerable<TeuchiUdonAssembly>> VisitForIter
        (
            IterExprResult result,
            Func<IEnumerable<TeuchiUdonAssembly>, IEnumerable<TeuchiUdonAssembly>> setter
        )
        {
            switch (result)
            {
                case RangeIterExprResult        rangeIter       : return VisitForRangeIter       (rangeIter       , setter);
                case SteppedRangeIterExprResult steppedRangeIter: return VisitForSteppedRangeIter(steppedRangeIter, setter);
                case SpreadIterExprResult       spreadIter      : return VisitForSpreadIter      (spreadIter      , setter);
                default: throw new NotSupportedException("unsupported parser result type");
            }
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitBody(BodyResult result)
        {
            var topEvents = result.TopStatements
                .Where(x =>
                    x is TopBindResult topBind &&
                    topBind.VarBind.Vars.Length == 1 && TypeOps.IsFunc(topBind.VarBind.Vars[0].Type) &&
                    (StaticTables.Events.ContainsKey(topBind.VarBind.Vars[0].Name) || topBind.Public)
                )
                .Cast<TopBindResult>()
                .Select(x =>
                    (
                        varName  : x.VarBind.Vars[0].Name,
                        eventName: LabelOps.GetFullLabel(x.VarBind.Vars[0])
                    ))
                .ToArray();
            var topStats = result.TopStatements
                .Where(x =>
                    !(x is TopBindResult topBind) ||
                    topBind.VarBind.Vars.Length == 1 && TypeOps.IsFunc(topBind.VarBind.Vars[0].Type) ||
                    !topBind.Public
                )
                .ToArray();

            var startVarName   = "Start";
            var startEventName = TeuchiUdonTableOps.GetEventName(startVarName);
            var topEventStats   = new Dictionary<string, (string eventName, List<TopStatementResult> stats)>();
            foreach (var ev in topEvents)
            {
                if (!topEventStats.ContainsKey(ev.varName))
                {
                    topEventStats.Add(ev.varName, (ev.eventName, new List<TopStatementResult>()));
                }
            }
            foreach (var stat in topStats)
            {
                if (!topEventStats.ContainsKey(startVarName))
                {
                    topEventStats.Add(startVarName, (startEventName, new List<TopStatementResult>()));
                }
                topEventStats[startVarName].stats.Add(stat);
            }

            return
                topEventStats.Count == 0 ? Enumerable.Empty<TeuchiUdonAssembly>() :
                topEventStats.Select(x => Event(x.Key, x.Value.eventName, x.Value.stats))
                .Aggregate((acc, x) => acc
                    .Concat(new TeuchiUdonAssembly[] { new Assembly_NEW_LINE() })
                    .Concat(x)
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitTopBind(TopBindResult result)
        {
            return
                VisitExpr(result.VarBind.Expr)
                .Concat(result.VarBind.Vars.Reverse().SelectMany(x => Set(x)));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitTopExpr(TopExprResult result)
        {
            return VisitExpr(result.Expr);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitJump(JumpResult result)
        {
            return
                VisitExpr(result.Value)
                .Concat(Jump(result.Label()));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitLetBind(LetBindResult result)
        {
            return
                VisitExpr(result.VarBind.Expr)
                .Concat(result.VarBind.Vars.Reverse().SelectMany(x => Set(x)));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitExpr(ExprResult result)
        {
            return
                VisitTyped(result.Inner)
                .Concat
                (
                    result.ReturnsValue ?
                        Enumerable.Empty<TeuchiUdonAssembly>() :
                        Pop(result.Inner.Type)
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitInvalid(InvalidResult result)
        {
            return Enumerable.Empty<TeuchiUdonAssembly>();
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitUnknownType(UnknownTypeResult result)
        {
            return Enumerable.Empty<TeuchiUdonAssembly>();
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitUnit(UnitResult result)
        {
            return Enumerable.Empty<TeuchiUdonAssembly>();
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitBlock(BlockResult result)
        {
            return result.Statements.SelectMany(x => VisitStatement(x)).Concat(VisitExpr(result.Expr));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitParen(ParenResult result)
        {
            return VisitExpr(result.Expr);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitTuple(TupleResult result)
        {
            return result.Exprs.SelectMany(x => VisitExpr(x));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitArrayCtor(ArrayCtorResult result)
        {
            return
                result.Methods["ctor"           ] == null ||
                result.Methods["setter"         ] == null ||
                result.Methods["lessThanOrEqual"] == null ||
                result.Methods["addition"       ] == null ?
                    Enumerable.Empty<TeuchiUdonAssembly>() :
                result.Iters.Any() ?
                    result.Iters.SelectMany(x => VisitArrayIter(result, x)) :
                    EmptyArrayCtor
                    (
                        result.Literals ["0"],
                        result.TmpValues["array"],
                        result.Methods  ["ctor"]
                    );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitLiteral(LiteralResult result)
        {
            return Get(result.Literal);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitThis(ThisResult result)
        {
            return Get(result.This);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitInterpolatedString(InterpolatedStringResult result)
        {
            return
                result.Methods["format"     ] == null ||
                result.Methods["arrayCtor"  ] == null ||
                result.Methods["arraySetter"] == null ||
                result.Methods["keyAddition"] == null ?
                Enumerable.Empty<TeuchiUdonAssembly>() :
                EvalMethod
                (
                    new IEnumerable<TeuchiUdonAssembly>[]
                    {
                        Get(result.StringLiteral),
                        ElementsArrayCtor
                        (
                            result.Exprs.Select(x => VisitExpr(x)),
                            result.Literals ["0"],
                            result.Literals ["1"],
                            result.Literals ["length"],
                            result.TmpValues["array"],
                            result.TmpValues["key"],
                            result.Methods  ["arrayCtor"],
                            result.Methods  ["arraySetter"],
                            result.Methods  ["keyAddition"]
                        )
                    },
                    new IDataLabel[] { result.TmpValues["out"] },
                    result.Methods["format"]
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalVar(EvalVarResult result)
        {
            return Get(result.Var);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalType(EvalTypeResult result)
        {
            return Enumerable.Empty<TeuchiUdonAssembly>();
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalQualifier(EvalQualifierResult result)
        {
            return Enumerable.Empty<TeuchiUdonAssembly>();
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalGetter(EvalGetterResult result)
        {
            return EvalMethod(Enumerable.Empty<TeuchiUdonAssembly[]>(), result.OutValuess["getter"], result.Methods["getter"]);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalSetter(EvalSetterResult result)
        {
            return Enumerable.Empty<TeuchiUdonAssembly>();
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalGetterSetter(EvalGetterSetterResult result)
        {
            return EvalMethod(Enumerable.Empty<TeuchiUdonAssembly[]>(), result.OutValuess["getter"], result.Methods["getter"]);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalFunc(EvalFuncResult result)
        {
            return
                VisitExpr(result.Expr)
                .Concat(Set(result.OutValue))
                .Concat(EvalFunc(result.Args.Select(x => VisitExpr(x)), result.EvalFunc, result.OutValue));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalSpreadFunc(EvalSpreadFuncResult result)
        {
            return
                VisitExpr(result.Expr)
                .Concat(Set(result.OutValue))
                .Concat(EvalFunc(new IEnumerable<TeuchiUdonAssembly>[] { VisitExpr(result.Arg) }, result.EvalFunc, result.OutValue));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalMethod(EvalMethodResult result)
        {
            return
                EvalMethod
                (
                    new IEnumerable<TeuchiUdonAssembly>[] { VisitExpr(result.Expr) }.Concat(result.Args.Select(x => VisitExpr(x))),
                    result.OutValuess["method"],
                    result.Methods   ["method"]
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalSpreadMethod(EvalSpreadMethodResult result)
        {
            return
                EvalMethod
                (
                    new IEnumerable<TeuchiUdonAssembly>[]
                    {
                        VisitExpr(result.Expr),
                        VisitExpr(result.Arg)
                    },
                    result.OutValuess["method"],
                    result.Methods   ["method"]
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalCoalescingMethod(EvalCoalescingMethodResult result)
        {
            return
                 result.Methods["method"] == null ? Enumerable.Empty<TeuchiUdonAssembly>() :
                 result.Methods.ContainsKey("==") && result.Methods["=="] == null ? Enumerable.Empty<TeuchiUdonAssembly>() :
                !result.Methods.ContainsKey("==") ? Get(result.Literals["null"]) :
                IfElse
                (
                    VisitExpr(result.Expr1)
                    .Concat(Set(result.TmpValues["tmp"]))
                    .Concat
                    (
                        EvalMethod
                        (
                            new IEnumerable<TeuchiUdonAssembly>[] { Get(result.TmpValues["tmp"]), Get(result.Literals["null"]) },
                            result.OutValuess["=="],
                            result.Methods   ["=="]
                        )
                    ),
                    Get(result.Literals["null"]),
                    EvalMethod
                    (
                        new IEnumerable<TeuchiUdonAssembly>[] { Get(result.TmpValues["tmp"]) }.Concat(result.Args.Select(x => VisitExpr(x))),
                        result.OutValuess["method"],
                        result.Methods   ["method"]
                    ),
                    result.Labels["1"],
                    result.Labels["2"]
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalCoalescingSpreadMethod(EvalCoalescingSpreadMethodResult result)
        {
            return
                 result.Methods["method"] == null ? Enumerable.Empty<TeuchiUdonAssembly>() :
                 result.Methods.ContainsKey("==") && result.Methods["=="] == null ? Enumerable.Empty<TeuchiUdonAssembly>() :
                !result.Methods.ContainsKey("==") ? Get(result.Literals["null"]) :
                IfElse
                (
                    VisitExpr(result.Expr1)
                    .Concat(Set(result.TmpValues["tmp"]))
                    .Concat
                    (
                        EvalMethod
                        (
                            new IEnumerable<TeuchiUdonAssembly>[] { Get(result.TmpValues["tmp"]), Get(result.Literals["null"]) },
                            result.OutValuess["=="],
                            result.Methods   ["=="]
                        )
                    ),
                    Get(result.Literals["null"]),
                    EvalMethod
                    (
                        new IEnumerable<TeuchiUdonAssembly>[]
                        {
                            Get(result.TmpValues["tmp"]),
                            VisitExpr(result.Arg)
                        },
                        result.OutValuess["method"],
                        result.Methods   ["method"]
                    ),
                    result.Labels["1"],
                    result.Labels["2"]
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalCast(EvalCastResult result)
        {
            return Enumerable.Empty<TeuchiUdonAssembly>();
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalTypeOf(EvalTypeOfResult result)
        {
            return Enumerable.Empty<TeuchiUdonAssembly>();
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalArrayIndexer(EvalArrayIndexerResult result)
        {
            return EvalMethod
            (
                new IEnumerable<TeuchiUdonAssembly>[] { VisitExpr(result.Expr), VisitExpr(result.Arg) },
                result.OutValuess["getter"],
                result.Methods   ["getter"]
            );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitTypeCast(TypeCastResult result)
        {
            return VisitExpr(result.Expr).Concat(VisitExpr(result.Arg));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitConvertCast(ConvertCastResult result)
        {
            return
                result.Methods.Values.Any(x => x == null) ? Enumerable.Empty<TeuchiUdonAssembly>() :
                VisitExpr(result.Expr)
                .Concat
                (
                    EvalMethod
                    (
                        new IEnumerable<TeuchiUdonAssembly>[] { VisitExpr(result.Arg) },
                        result.OutValuess["convert"],
                        result.Methods   ["convert"]
                    )
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitTypeOf(TypeOfResult result)
        {
            return Get(result.Literal);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitPrefix(PrefixResult result)
        {
            switch (result.Op)
            {
                case "+":
                    return VisitExpr(result.Expr);
                case "-":
                case "!":
                    return
                        result.Methods["op"] == null ? Enumerable.Empty<TeuchiUdonAssembly>() :
                        EvalMethod
                        (
                            new IEnumerable<TeuchiUdonAssembly>[] { VisitExpr(result.Expr) },
                            result.OutValuess["op"],
                            result.Methods   ["op"]
                        );
                case "~":
                    return
                        result.Methods["op"] == null ? Enumerable.Empty<TeuchiUdonAssembly>() :
                        EvalMethod
                        (
                            new IEnumerable<TeuchiUdonAssembly>[] { VisitExpr(result.Expr), Get(result.Literals["mask"]) },
                            result.OutValuess["op"],
                            result.Methods   ["op"]
                        );
                default:
                    return Enumerable.Empty<TeuchiUdonAssembly>();
            }
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitInfix(InfixResult result)
        {
            switch (result.Op)
            {
                case ".":
                    return VisitExpr(result.Expr1).Concat(VisitExpr(result.Expr2));
                case "?.":
                    return
                         result.Methods.ContainsKey("==") && result.Methods["=="] == null ? Enumerable.Empty<TeuchiUdonAssembly>() :
                        !result.Methods.ContainsKey("==") ? Get(result.Literals["null"]) :
                        IfElse
                        (
                            VisitExpr(result.Expr1)
                            .Concat(Set(result.TmpValues["tmp"]))
                            .Concat
                            (
                                EvalMethod
                                (
                                    new IEnumerable<TeuchiUdonAssembly>[] { Get(result.TmpValues["tmp"]), Get(result.Literals["null"]) },
                                    result.OutValuess["=="],
                                    result.Methods   ["=="]
                                )
                            ),
                            Get(result.Literals["null"]),
                            Get(result.TmpValues["tmp"]).Concat(VisitExpr(result.Expr2)),
                            result.Labels["1"],
                            result.Labels["2"]
                        );
                case "+":
                case "-":
                case "*":
                case "/":
                case "%":
                case "<<":
                case ">>":
                case "<":
                case ">":
                case "<=":
                case ">=":
                case "&":
                case "^":
                case "|":
                    return
                        result.Methods["op"] == null ? Enumerable.Empty<TeuchiUdonAssembly>() :
                        EvalMethod
                        (
                            new IEnumerable<TeuchiUdonAssembly>[] { VisitExpr(result.Expr1), VisitExpr(result.Expr2) },
                            result.OutValuess["op"],
                            result.Methods   ["op"]
                        );
                case "==":
                    return
                         result.Methods.ContainsKey("op") && result.Methods["op"] == null ? Enumerable.Empty<TeuchiUdonAssembly>() :
                        !result.Methods.ContainsKey("op") ? Get(result.Literals["true"]) :
                        EvalMethod
                        (
                            new IEnumerable<TeuchiUdonAssembly>[] { VisitExpr(result.Expr1), VisitExpr(result.Expr2) },
                            result.OutValuess["op"],
                            result.Methods   ["op"]
                        );
                case "!=":
                    return
                         result.Methods.ContainsKey("op") && result.Methods["op"] == null ? Enumerable.Empty<TeuchiUdonAssembly>() :
                        !result.Methods.ContainsKey("op") ? Get(result.Literals["false"]) :
                        EvalMethod
                        (
                            new IEnumerable<TeuchiUdonAssembly>[] { VisitExpr(result.Expr1), VisitExpr(result.Expr2) },
                            result.OutValuess["op"],
                            result.Methods   ["op"]
                        );
                case "&&":
                    return
                        IfElse
                        (
                            VisitExpr(result.Expr1),
                            VisitExpr(result.Expr2),
                            Get(result.Literals["false"]),
                            result.Labels["1"],
                            result.Labels["2"]
                        );
                case "||":
                    return
                        IfElse
                        (
                            VisitExpr(result.Expr1),
                            Get(result.Literals["true"]),
                            VisitExpr(result.Expr2),
                            result.Labels["1"],
                            result.Labels["2"]
                        );
                case "??":
                    return
                         result.Methods.ContainsKey("==") && result.Methods["=="] == null ? Enumerable.Empty<TeuchiUdonAssembly>() :
                        !result.Methods.ContainsKey("==") ? VisitExpr(result.Expr2) :
                        IfElse
                        (
                            VisitExpr(result.Expr1)
                            .Concat(Set(result.TmpValues["tmp"]))
                            .Concat
                            (
                                EvalMethod
                                (
                                    new IEnumerable<TeuchiUdonAssembly>[] { Get(result.TmpValues["tmp"]), Get(result.Literals["null"]) },
                                    result.OutValuess["=="],
                                    result.Methods   ["=="]
                                )
                            ),
                            VisitExpr(result.Expr2),
                            Get(result.TmpValues["tmp"]),
                            result.Labels["1"],
                            result.Labels["2"]
                        );
                case "<-":
                {
                    return
                        result.Expr1.Inner.Type.LogicalTypeEquals(Primitives.Unit) || result.Expr2.Inner.Type.LogicalTypeEquals(Primitives.Unit) ?
                            Enumerable.Empty<TeuchiUdonAssembly>() :
                        result.Expr1.Inner.LeftValues.Length == 1 ?
                            result.Expr1.Inner.LeftValues[0] is TeuchiUdonVar ?
                                EvalAssign
                                (
                                    VisitExpr(result.Expr1),
                                    VisitExpr(result.Expr2)
                                ) :
                            result.Expr1.Inner.LeftValues[0] is TeuchiUdonMethod m ?
                                EvalSetterAssign
                                (
                                    VisitExpr(result.Expr1.Inner.Instance),
                                    VisitExpr(result.Expr2),
                                    m
                                ) :
                            result.Expr1.Inner.LeftValues[0] is TeuchiUdonArraySetter s ?
                                EvalArraySetterAssign
                                (
                                    VisitExpr(s.Expr),
                                    VisitExpr(s.Arg),
                                    VisitExpr(result.Expr2),
                                    s.Method
                                ) :
                                Enumerable.Empty<TeuchiUdonAssembly>() :
                            Enumerable.Empty<TeuchiUdonAssembly>();
                }
                default:
                    return Enumerable.Empty<TeuchiUdonAssembly>();
            }
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitLetInBind(LetInBindResult result)
        {
            return
                VisitExpr(result.VarBind.Expr)
                .Concat(result.VarBind.Vars.Reverse().SelectMany(x => Set(x)))
                .Concat(VisitExpr(result.Expr));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitIf(IfResult result)
        {
            return
                IfElif
                (
                    result.Conditions.Select(x => VisitExpr(x)),
                    result.Statements.Select(x => VisitStatement(x)),
                    result.Labels,
                    result.Statements.Select(x => x is ExprResult expr ? expr.Inner.Type : Primitives.Unit)
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitIfElse(IfElseResult result)
        {
            return
                IfElifElse
                (
                    result.Conditions.Select(x => VisitExpr(x)),
                    result.ThenParts.Select(x => VisitExpr(x)),
                    VisitExpr(result.ElsePart),
                    result.Labels
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitWhile(WhileResult result)
        {
            return
                While
                (
                    VisitExpr(result.Condition),
                    VisitExpr(result.Expr),
                    result.Labels[0],
                    result.Labels[1],
                    result.Expr.Inner.Type
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitFor(ForResult result)
        {
            return
                For
                (
                    result.ForBinds.Select(x =>
                        x is LetForBindResult    letBind ? VisitForIter(letBind.Iter, VisitLetForBindForSetter   (letBind)) :
                        x is AssignForBindResult assign  ? VisitForIter(assign .Iter, VisitAssignForBindForSetter(assign))  :
                        _ => Enumerable.Empty<TeuchiUdonAssembly>()
                    ),
                    VisitExpr(result.Expr),
                    result.ContinueLabel,
                    result.Expr.Inner.Type
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitLoop(LoopResult result)
        {
            return
                Loop
                (
                    VisitExpr(result.Expr),
                    result.Labels[0],
                    result.Labels[1],
                    result.Expr.Inner.Type
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitFunc(FuncResult result)
        {
            return Indirect(result.Func);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitMethod(MethodResult result)
        {
            return Enumerable.Empty<TeuchiUdonAssembly>();
        }
        
        protected IEnumerable<TeuchiUdonAssembly> VisitArrayElementsIter(ArrayCtorResult ctor, ElementsIterExprResult result)
        {
            return ElementsArrayCtor
            (
                result.Exprs.Select(x => VisitExpr(x)),
                ctor  .Literals ["0"],
                ctor  .Literals ["1"],
                result.Literals ["length"],
                ctor  .TmpValues["array"],
                ctor  .TmpValues["key"],
                ctor  .Methods  ["ctor"],
                ctor  .Methods  ["setter"],
                ctor  .Methods  ["addition"]
            );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitArrayRangeIter(ArrayCtorResult ctor, RangeIterExprResult result)
        {
            return
                result.Methods["lessThanOrEqual"] == null ||
                result.Methods["greaterThan"    ] == null ||
                result.Methods["convert"        ] == null ||
                result.Methods["addition"       ] == null ||
                result.Methods["subtraction"    ] == null ?
                Enumerable.Empty<TeuchiUdonAssembly>() :
                RangeArrayCtor
                (
                    VisitExpr(result.First),
                    VisitExpr(result.Last),
                    ctor  .Literals ["0"],
                    ctor  .Literals ["1"],
                    result.Literals ["step"],
                    result.TmpValues["value"],
                    result.TmpValues["limit"],
                    result.TmpValues["condition"],
                    result.TmpValues["length"],
                    result.TmpValues["valueLength"],
                    ctor  .TmpValues["array"],
                    ctor  .TmpValues["key"],
                    ctor  .Methods  ["ctor"],
                    ctor  .Methods  ["setter"],
                    result.Methods  ["lessThanOrEqual"],
                    result.Methods  ["greaterThan"],
                    result.Methods  ["convert"],
                    ctor  .Methods  ["addition"],
                    result.Methods  ["addition"],
                    result.Methods  ["subtraction"],
                    result.Labels   ["branch1"],
                    result.Labels   ["branch2"],
                    result.Labels   ["loop1"],
                    result.Labels   ["loop2"],
                    result.Type
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitArraySteppedRangeIter(ArrayCtorResult ctor, SteppedRangeIterExprResult result)
        {
            return
                result.Methods["keyGreaterThan" ] == null ||
                result.Methods["equality"       ] == null ||
                result.Methods["lessThanOrEqual"] == null ||
                result.Methods["greaterThan"    ] == null ||
                result.Methods["convert"        ] == null ||
                result.Methods["addition"       ] == null ||
                result.Methods["subtraction"    ] == null ||
                result.Methods["division"       ] == null ?
                Enumerable.Empty<TeuchiUdonAssembly>() :
                SteppedRangeArrayCtor
                (
                    VisitExpr(result.First),
                    VisitExpr(result.Last),
                    VisitExpr(result.Step),
                    ctor  .Literals ["0"],
                    ctor  .Literals ["1"],
                    result.Literals ["0"],
                    result.TmpValues["value"],
                    result.TmpValues["limit"],
                    result.TmpValues["step"],
                    result.TmpValues["isUpTo"],
                    result.TmpValues["condition"],
                    result.TmpValues["length"],
                    result.TmpValues["valueLength"],
                    ctor  .TmpValues["array"],
                    ctor  .TmpValues["key"],
                    ctor  .Methods  ["ctor"],
                    ctor  .Methods  ["setter"],
                    result.Methods  ["keyGreaterThan"],
                    result.Methods  ["equality"],
                    result.Methods  ["lessThanOrEqual"],
                    result.Methods  ["greaterThan"],
                    result.Methods  ["convert"],
                    ctor  .Methods  ["addition"],
                    result.Methods  ["addition"],
                    result.Methods  ["subtraction"],
                    result.Methods  ["division"],
                    result.Labels   ["branch1"],
                    result.Labels   ["branch2"],
                    result.Labels   ["branch3"],
                    result.Labels   ["branch4"],
                    result.Labels   ["branch5"],
                    result.Labels   ["branch6"],
                    result.Labels   ["branch7"],
                    result.Labels   ["branch8"],
                    result.Labels   ["loop1"],
                    result.Labels   ["loop2"],
                    result.Type
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitArraySpreadIter(ArrayCtorResult ctor, SpreadIterExprResult result)
        {
            return
                result.Methods["clone"] == null ?
                Enumerable.Empty<TeuchiUdonAssembly>() :
                SpreadArrayCtor(VisitExpr(result.Expr), ctor.TmpValues["array"], result.Methods["clone"]);
        }

        protected Func<IEnumerable<TeuchiUdonAssembly>, IEnumerable<TeuchiUdonAssembly>> VisitLetForBindForSetter(LetForBindResult result)
        {
            return x => x.Concat(result.Vars.Reverse().SelectMany(y => Set(y)));
        }

        protected Func<IEnumerable<TeuchiUdonAssembly>, IEnumerable<TeuchiUdonAssembly>> VisitAssignForBindForSetter(AssignForBindResult result)
        {
            return x =>
                result.Expr.Inner.LeftValues.Length == 1 && result.Expr.Inner.LeftValues[0] is TeuchiUdonVar ?
                    EvalAssign
                    (
                        VisitExpr(result.Expr),
                        x
                    ) :
                result.Expr.Inner.LeftValues.Length == 1 && result.Expr.Inner.LeftValues[0] is TeuchiUdonMethod m ?
                    EvalSetterAssign
                    (
                        VisitExpr(result.Expr.Inner.Instance),
                        x,
                        m
                    ) :
                Enumerable.Empty<TeuchiUdonAssembly>();
        }

        protected Func<IEnumerable<TeuchiUdonAssembly>, IEnumerable<TeuchiUdonAssembly>> VisitForRangeIter
        (
            RangeIterExprResult result,
            Func<IEnumerable<TeuchiUdonAssembly>, IEnumerable<TeuchiUdonAssembly>> setter
        )
        {
            return x =>
                result.Methods["lessThanOrEqual"] == null ||
                result.Methods["greaterThan"    ] == null ||
                result.Methods["addition"       ] == null ?
                Enumerable.Empty<TeuchiUdonAssembly>() :
                RangeIter
                (
                    VisitExpr(result.First),
                    VisitExpr(result.Last),
                    result.Literals ["step"],
                    result.TmpValues["value"],
                    result.TmpValues["limit"],
                    result.TmpValues["condition"],
                    result.Methods  ["lessThanOrEqual"],
                    result.Methods  ["greaterThan"],
                    result.Methods  ["addition"],
                    result.Labels   ["loop1"],
                    result.Labels   ["loop2"],
                    Enumerable.Empty<TeuchiUdonAssembly>(),
                    setter(Get(result.TmpValues["value"])).Concat(x),
                    Enumerable.Empty<TeuchiUdonAssembly>()
                );
        }

        protected Func<IEnumerable<TeuchiUdonAssembly>, IEnumerable<TeuchiUdonAssembly>> VisitForSteppedRangeIter
        (
            SteppedRangeIterExprResult result,
            Func<IEnumerable<TeuchiUdonAssembly>, IEnumerable<TeuchiUdonAssembly>> setter
        )
        {
            return x =>
                result.Methods["equality"       ] == null ||
                result.Methods["lessThanOrEqual"] == null ||
                result.Methods["greaterThan"    ] == null ||
                result.Methods["addition"       ] == null ?
                Enumerable.Empty<TeuchiUdonAssembly>() :
                SteppedRangeIter
                (
                    VisitExpr(result.First),
                    VisitExpr(result.Last),
                    VisitExpr(result.Step),
                    result.Literals ["0"],
                    result.TmpValues["value"],
                    result.TmpValues["limit"],
                    result.TmpValues["step"],
                    result.TmpValues["isUpTo"],
                    result.TmpValues["condition"],
                    result.Methods  ["equality"],
                    result.Methods  ["lessThanOrEqual"],
                    result.Methods  ["greaterThan"],
                    result.Methods  ["addition"],
                    result.Labels   ["branch1"],
                    result.Labels   ["branch2"],
                    result.Labels   ["branch3"],
                    result.Labels   ["branch4"],
                    result.Labels   ["branch5"],
                    result.Labels   ["branch6"],
                    result.Labels   ["loop1"],
                    result.Labels   ["loop2"],
                    Enumerable.Empty<TeuchiUdonAssembly>(),
                    setter(Get(result.TmpValues["value"])).Concat(x),
                    Enumerable.Empty<TeuchiUdonAssembly>(),
                    Enumerable.Empty<TeuchiUdonAssembly>()
                );
        }

        protected Func<IEnumerable<TeuchiUdonAssembly>, IEnumerable<TeuchiUdonAssembly>> VisitForSpreadIter
        (
            SpreadIterExprResult result,
            Func<IEnumerable<TeuchiUdonAssembly>, IEnumerable<TeuchiUdonAssembly>> setter
        )
        {
            return x =>
                result.Methods["getter"            ] == null ||
                result.Methods["getLength"         ] == null ||
                result.Methods["lessThan"          ] == null ||
                result.Methods["greaterThanOrEqual"] == null ||
                result.Methods["addition"          ] == null ?
                Enumerable.Empty<TeuchiUdonAssembly>() :
                SpreadIter
                (
                    VisitExpr(result.Expr),
                    result.Literals ["0"],
                    result.Literals ["1"],
                    result.TmpValues["array"],
                    result.TmpValues["key"],
                    result.TmpValues["value"],
                    result.TmpValues["length"],
                    result.TmpValues["condition"],
                    result.Methods  ["getter"],
                    result.Methods  ["getLength"],
                    result.Methods  ["lessThan"],
                    result.Methods  ["greaterThanOrEqual"],
                    result.Methods  ["addition"],
                    result.Labels   ["loop1"],
                    result.Labels   ["loop2"],
                    Enumerable.Empty<TeuchiUdonAssembly>(),
                    setter(Get(result.TmpValues["value"])).Concat(x),
                    Enumerable.Empty<TeuchiUdonAssembly>()
                );
        }

        public IEnumerable<TeuchiUdonAssembly> DeclIndirectAddresses(IEnumerable<(TeuchiUdonIndirect indirect, uint address)> pairs)
        {
            return pairs.SelectMany(x => DeclData(x.indirect, new AssemblyLiteral_NULL()));
        }
    }
}
