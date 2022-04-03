using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public abstract class TeuchiUdonCompilerAbstraction
    {
        public virtual void Init()
        {
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
        protected abstract IEnumerable<TeuchiUdonAssembly> Event(string varName, string eventName, TeuchiUdonMethod ev, List<TopStatementResult> stats);
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
                        TeuchiUdonTables.Instance.PublicVars.Keys.SelectMany(x => ExportData(x))
                .Concat(TeuchiUdonTables.Instance.SyncedVars     .SelectMany(x => SyncData(x.Key, x.Value)))
                .Concat(TeuchiUdonTables.Instance.Vars.Values    .SelectMany(x => DeclData(x, new AssemblyLiteral_NULL())))
                .Concat(TeuchiUdonTables.Instance.Literals.Values.SelectMany(x => DeclData(x, new AssemblyLiteral_NULL())))
                .Concat(TeuchiUdonTables.Instance.This.Values    .SelectMany(x => DeclData(x, new AssemblyLiteral_THIS())))
                .Concat(TeuchiUdonTables.Instance.Funcs.Values   .SelectMany(x => DeclData(x.Return, new AssemblyLiteral_NULL())));
        }

        public IEnumerable<TeuchiUdonAssembly> GetDataPartFromOutValuePool()
        {
            return TeuchiUdonOutValuePool.Instance.OutValues.Values.SelectMany(x => DeclData(x, new AssemblyLiteral_NULL()));
        }

        public IEnumerable<TeuchiUdonAssembly> GetCodePartFromTables()
        {
            return VisitTables();
        }

        public IEnumerable<TeuchiUdonAssembly> GetCodePartFromResult(BodyResult result)
        {
            return VisitBody(result);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitTables()
        {
            return
                TeuchiUdonTables.Instance.Funcs.Values.Count == 0 ? Enumerable.Empty<TeuchiUdonAssembly>() :
                TeuchiUdonTables.Instance.Funcs.Values.Select(x => Func(x))
                .Aggregate((acc, x) => acc
                    .Concat(new TeuchiUdonAssembly[] { new Assembly_NEW_LINE() })
                    .Concat(x)
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitTopStatement(TopStatementResult result)
        {
            if (result is TopBindResult topBind) return VisitTopBind(topBind);
            if (result is TopExprResult topExpr) return VisitTopExpr(topExpr);
            throw new InvalidOperationException("unsupported parser result type");
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitStatement(StatementResult result)
        {
            if (result is JumpResult    jump   ) return VisitJump   (jump);
            if (result is LetBindResult letBind) return VisitLetBind(letBind);
            if (result is ExprResult    expr   ) return VisitExpr   (expr);
            throw new NotSupportedException("unsupported parser result type");
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitTyped(TypedResult result)
        {
            if (result is InvalidResult            invalid           ) return VisitInvalid           (invalid);
            if (result is UnknownTypeResult        unknownType       ) return VisitUnknownType       (unknownType);
            if (result is UnitResult               unit              ) return VisitUnit              (unit);
            if (result is BlockResult              block             ) return VisitBlock             (block);
            if (result is ParenResult              paren             ) return VisitParen             (paren);
            if (result is TupleResult              tuple             ) return VisitTuple             (tuple);
            if (result is ArrayCtorResult          arrayCtor         ) return VisitArrayCtor         (arrayCtor);
            if (result is LiteralResult            literal           ) return VisitLiteral           (literal);
            if (result is ThisResult               this_             ) return VisitThis              (this_);
            if (result is InterpolatedStringResult interpolatedString) return VisitInterpolatedString(interpolatedString);
            if (result is EvalVarResult            evalVar           ) return VisitEvalVar           (evalVar);
            if (result is EvalTypeResult           evalType          ) return VisitEvalType          (evalType);
            if (result is EvalQualifierResult      evalQualifier     ) return VisitEvalQualifier     (evalQualifier);
            if (result is EvalGetterResult         evalGetter        ) return VisitEvalGetter        (evalGetter);
            if (result is EvalSetterResult         evalSetter        ) return VisitEvalSetter        (evalSetter);
            if (result is EvalGetterSetterResult   evalGetterSetter  ) return VisitEvalGetterSetter  (evalGetterSetter);
            if (result is EvalFuncResult           evalFunc          ) return VisitEvalFunc          (evalFunc);
            if (result is EvalSpreadFuncResult     evalSpreadFunc    ) return VisitEvalSpreadFunc    (evalSpreadFunc);
            if (result is EvalMethodResult         evalMethod        ) return VisitEvalMethod        (evalMethod);
            if (result is EvalSpreadMethodResult   evalSpreadMethod  ) return VisitEvalSpreadMethod  (evalSpreadMethod);
            if (result is EvalArrayIndexerResult   evalArrayIndexer  ) return VisitEvalArrayIndexer  (evalArrayIndexer);
            if (result is TypeCastResult           typeCast          ) return VisitTypeCast          (typeCast);
            if (result is ConvertCastResult        convertCast       ) return VisitConvertCast       (convertCast);
            if (result is PrefixResult             prefix            ) return VisitPrefix            (prefix);
            if (result is InfixResult              infix             ) return VisitInfix             (infix);
            if (result is LetInBindResult          letInBind         ) return VisitLetInBind         (letInBind);
            if (result is IfResult                 if_               ) return VisitIf                (if_);
            if (result is IfElseResult             ifElse            ) return VisitIfElse            (ifElse);
            if (result is WhileResult              while_            ) return VisitWhile             (while_);
            if (result is ForResult                for_              ) return VisitFor               (for_);
            if (result is LoopResult               loop              ) return VisitLoop              (loop);
            if (result is FuncResult               func              ) return VisitFunc              (func);
            if (result is MethodResult             method            ) return VisitMethod            (method);
            throw new NotSupportedException("unsupported parser result type");
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitArrayIter(ArrayCtorResult ctor, IterExprResult result)
        {
            if (result is ElementsIterExprResult     elementsIter    ) return VisitArrayElementsIter    (ctor, elementsIter);
            if (result is RangeIterExprResult        rangeIter       ) return VisitArrayRangeIter       (ctor, rangeIter);
            if (result is SteppedRangeIterExprResult steppedRangeIter) return VisitArraySteppedRangeIter(ctor, steppedRangeIter);
            if (result is SpreadIterExprResult       spreadIter      ) return VisitArraySpreadIter      (ctor, spreadIter);
            throw new NotSupportedException("unsupported parser result type");
        }

        protected Func<IEnumerable<TeuchiUdonAssembly>, IEnumerable<TeuchiUdonAssembly>> VisitForBindForSetter(ForBindResult result)
        {
            if (result is LetForBindResult    letBind) return VisitLetForBindForSetter   (letBind);
            if (result is AssignForBindResult assign ) return VisitAssignForBindForSetter(assign);
            throw new NotSupportedException("unsupported parser result type");
        }

        protected Func<IEnumerable<TeuchiUdonAssembly>, IEnumerable<TeuchiUdonAssembly>> VisitForIter
        (
            IterExprResult result,
            Func<IEnumerable<TeuchiUdonAssembly>, IEnumerable<TeuchiUdonAssembly>> setter
        )
        {
            if (result is RangeIterExprResult        rangeIter       ) return VisitForRangeIter       (rangeIter       , setter);
            if (result is SteppedRangeIterExprResult steppedRangeIter) return VisitForSteppedRangeIter(steppedRangeIter, setter);
            if (result is SpreadIterExprResult       spreadIter      ) return VisitForSpreadIter      (spreadIter      , setter);
            throw new NotSupportedException("unsupported parser result type");
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitBody(BodyResult result)
        {
            var topEvents = result.TopStatements
                .Where(x =>
                    x is TopBindResult topBind &&
                    topBind.VarBind.Vars.Length == 1 && topBind.VarBind.Vars[0].Type.IsFunc() &&
                    (TeuchiUdonTables.Instance.Events.ContainsKey(topBind.VarBind.Vars[0].Name) || topBind.Public)
                )
                .Cast<TopBindResult>()
                .Select(x =>
                    (
                        varName  : x.VarBind.Vars[0].Name,
                        eventName: x.VarBind.Vars[0].GetFullLabel(),
                        method   : TeuchiUdonTables.Instance.Events.ContainsKey(x.VarBind.Vars[0].Name) ? TeuchiUdonTables.Instance.Events[x.VarBind.Vars[0].Name] : null
                    ))
                .ToArray();
            var topStats = result.TopStatements
                .Where(x =>
                    !(x is TopBindResult topBind) ||
                    topBind.VarBind.Vars.Length == 1 && topBind.VarBind.Vars[0].Type.IsFunc() ||
                    !topBind.Public
                )
                .ToArray();

            var startVarName   = "Start";
            var startEventName = TeuchiUdonTables.GetEventName(startVarName);
            var topEventStats   = new Dictionary<string, (string eventName, TeuchiUdonMethod ev, List<TopStatementResult> stats)>();
            foreach (var ev in topEvents)
            {
                if (!topEventStats.ContainsKey(ev.varName))
                {
                    topEventStats.Add(ev.varName, (ev.eventName, ev.method, new List<TopStatementResult>()));
                }
            }
            foreach (var stat in topStats)
            {
                if (!topEventStats.ContainsKey(startVarName))
                {
                    topEventStats.Add(startVarName, (startEventName, null, new List<TopStatementResult>()));
                }
                topEventStats[startVarName].stats.Add(stat);
            }

            return
                topEventStats.Count == 0 ? Enumerable.Empty<TeuchiUdonAssembly>() :
                topEventStats.Select(x => Event(x.Key, x.Value.eventName, x.Value.ev, x.Value.stats))
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
                VisitExpr(result.Expr)
                .Concat(EvalMethod(result.Args.Select(x => VisitExpr(x)), result.OutValuess["method"], result.Methods["method"]));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalSpreadMethod(EvalSpreadMethodResult result)
        {
            return
                VisitExpr(result.Expr)
                .Concat(EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { VisitExpr(result.Arg) }, result.OutValuess["method"], result.Methods["method"]));
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
                        result.Expr1.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Unit) || result.Expr2.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                            Enumerable.Empty<TeuchiUdonAssembly>() :
                        result.Expr1.Inner.LeftValues.Length == 1 && result.Expr1.Inner.LeftValues[0] is TeuchiUdonVar ?
                            EvalAssign
                            (
                                VisitExpr(result.Expr1),
                                VisitExpr(result.Expr2)
                            ) :
                        result.Expr1.Inner.LeftValues.Length == 1 && result.Expr1.Inner.LeftValues[0] is TeuchiUdonMethod m ?
                            EvalSetterAssign
                            (
                                VisitExpr(result.Expr1.Inner.Instance),
                                VisitExpr(result.Expr2),
                                m
                            ) :
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
                    result.Statements.Select(x => x is ExprResult expr ? expr.Inner.Type : TeuchiUdonType.Unit)
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
