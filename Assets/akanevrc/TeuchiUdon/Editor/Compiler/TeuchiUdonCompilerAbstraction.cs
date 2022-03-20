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

        protected abstract IEnumerable<TeuchiUdonAssembly> ExportData(IDataLabel label);
        protected abstract IEnumerable<TeuchiUdonAssembly> SyncData(IDataLabel label, TeuchiUdonSyncMode mode);
        protected abstract IEnumerable<TeuchiUdonAssembly> DeclData(IDataLabel label, TeuchiUdonAssemblyLiteral literal);
        protected abstract IEnumerable<TeuchiUdonAssembly> Pop();
        protected abstract IEnumerable<TeuchiUdonAssembly> Get(IDataLabel label);
        protected abstract IEnumerable<TeuchiUdonAssembly> Set(IDataLabel label);
        protected abstract IEnumerable<TeuchiUdonAssembly> Jump(IDataLabel label);
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
            IEnumerable<TeuchiUdonOutValue> outValues,
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
            IEnumerable<TeuchiUdonAssembly> truePart,
            IEnumerable<TeuchiUdonAssembly> falsePart,
            ICodeLabel label1,
            ICodeLabel label2
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> ArrayCtor
        (
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> elements,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> lengths,
            TeuchiUdonLiteral zero,
            TeuchiUdonOutValue array,
            TeuchiUdonOutValue counter,
            TeuchiUdonMethod ctor,
            TeuchiUdonMethod addition
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> ArrayElementsCtor
        (
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> elements,
            TeuchiUdonLiteral one,
            TeuchiUdonOutValue array,
            TeuchiUdonOutValue counter,
            TeuchiUdonMethod setter,
            TeuchiUdonMethod addition
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> ArrayElementsCtorLength(TeuchiUdonLiteral length);

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
            if (result is BottomResult           bottom          ) return VisitBottom          (bottom);
            if (result is UnknownTypeResult      unknownType     ) return VisitUnknownType     (unknownType);
            if (result is UnitResult             unit            ) return VisitUnit            (unit);
            if (result is BlockResult            block           ) return VisitBlock           (block);
            if (result is ParenResult            paren           ) return VisitParen           (paren);
            if (result is ArrayCtorResult        arrayCtor       ) return VisitArrayCtor       (arrayCtor);
            if (result is LiteralResult          literal         ) return VisitLiteral         (literal);
            if (result is ThisResult             ths             ) return VisitThis            (ths);
            if (result is EvalVarResult          evalVar         ) return VisitEvalVar         (evalVar);
            if (result is EvalTypeResult         evalType        ) return VisitEvalType        (evalType);
            if (result is EvalQualifierResult    evalQualifier   ) return VisitEvalQualifier   (evalQualifier);
            if (result is EvalGetterResult       evalGetter      ) return VisitEvalGetter      (evalGetter);
            if (result is EvalSetterResult       evalSetter      ) return VisitEvalSetter      (evalSetter);
            if (result is EvalGetterSetterResult evalGetterSetter) return VisitEvalGetterSetter(evalGetterSetter);
            if (result is EvalFuncResult         evalFunc        ) return VisitEvalFunc        (evalFunc);
            if (result is EvalMethodResult       evalMethod      ) return VisitEvalMethod      (evalMethod);
            if (result is EvalArrayIndexerResult evalArrayIndexer) return VisitEvalArrayIndexer(evalArrayIndexer);
            if (result is TypeCastResult         typeCast        ) return VisitTypeCast        (typeCast);
            if (result is ConvertCastResult      convertCast     ) return VisitConvertCast     (convertCast);
            if (result is PrefixResult           prefix          ) return VisitPrefix          (prefix);
            if (result is InfixResult            infix           ) return VisitInfix           (infix);
            if (result is ConditionalResult      conditional     ) return VisitConditional     (conditional);
            if (result is LetInBindResult        letInBind       ) return VisitLetInBind       (letInBind);
            if (result is FuncResult             func            ) return VisitFunc            (func);
            if (result is MethodResult           method          ) return VisitMethod          (method);
            throw new NotSupportedException("unsupported parser result type");
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitArrayExprToGetSetters(ArrayCtorResult ctor, ArrayExprResult result)
        {
            if (result is ElementsArrayExprResult     elementsArrayExpr    ) return VisitElementsArrayExprToGetSetters    (ctor, elementsArrayExpr);
            if (result is RangeArrayExprResult        rangeArrayExpr       ) return VisitRangeArrayExprToGetSetters       (ctor, rangeArrayExpr);
            if (result is SteppedRangeArrayExprResult steppedRangeArrayExpr) return VisitSteppedRangeArrayExprToGetSetters(ctor, steppedRangeArrayExpr);
            if (result is SpreadArrayExprResult       spreadArrayExpr      ) return VisitSpreadArrayExprToGetSetters      (ctor, spreadArrayExpr);
            throw new NotSupportedException("unsupported parser result type");
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitArrayExprToGetLengths(ArrayExprResult result)
        {
            if (result is ElementsArrayExprResult     elementsArrayExpr    ) return VisitElementsArrayExprToGetLengths    (elementsArrayExpr);
            if (result is RangeArrayExprResult        rangeArrayExpr       ) return VisitRangeArrayExprToGetLengths       (rangeArrayExpr);
            if (result is SteppedRangeArrayExprResult steppedRangeArrayExpr) return VisitSteppedRangeArrayExprToGetLengths(steppedRangeArrayExpr);
            if (result is SpreadArrayExprResult       spreadArrayExpr      ) return VisitSpreadArrayExprToGetLengths      (spreadArrayExpr);
            throw new NotSupportedException("unsupported parser result type");
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitBody(BodyResult result)
        {
            var topEvents = result.TopStatements
                .Where(x =>
                    x is TopBindResult topBind &&
                    topBind.VarBind.Vars.Length == 1 && topBind.VarBind.Vars[0].Type.LogicalTypeNameEquals(TeuchiUdonType.Func) &&
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
                    topBind.VarBind.Vars.Length == 1 && topBind.VarBind.Vars[0].Type.LogicalTypeNameEquals(TeuchiUdonType.Func) ||
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
                .Concat(Jump(result.Label));
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
                .Concat(result.ReturnsValue || result.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ? Enumerable.Empty<TeuchiUdonAssembly>() : Pop());
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitBottom(BottomResult result)
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

        protected IEnumerable<TeuchiUdonAssembly> VisitArrayCtor(ArrayCtorResult result)
        {
            return
                result.Methods["ctor"    ] == null ||
                result.Methods["setter"  ] == null ||
                result.Methods["addition"] == null ?
                    Enumerable.Empty<TeuchiUdonAssembly>() :
                    ArrayCtor
                    (
                        result.Exprs.Select(x => VisitArrayExprToGetSetters(result, x)),
                        result.Exprs.Select(x => VisitArrayExprToGetLengths(x)),
                        result.Literals["0"],
                        result.TmpValues["array"  ],
                        result.TmpValues["counter"],
                        result.Methods["ctor"],
                        result.Methods["addition"]
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
            return EvalMethod(Enumerable.Empty<TeuchiUdonAssembly[]>(), result.OutValues, result.Method);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalSetter(EvalSetterResult result)
        {
            return Enumerable.Empty<TeuchiUdonAssembly>();
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalGetterSetter(EvalGetterSetterResult result)
        {
            return EvalMethod(Enumerable.Empty<TeuchiUdonAssembly[]>(), result.OutValues, result.Getter);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalFunc(EvalFuncResult result)
        {
            return
                VisitExpr(result.Expr)
                .Concat(Set(result.OutValue))
                .Concat(EvalFunc(result.Args.Select(x => VisitExpr(x)), result.EvalFunc, result.OutValue));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalMethod(EvalMethodResult result)
        {
            return
                VisitExpr(result.Expr)
                .Concat(EvalMethod(result.Args.Select(x => VisitExpr(x)), result.OutValues, result.Method));
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
                        result.Expr1.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Unit) || result.Expr1.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
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

        protected IEnumerable<TeuchiUdonAssembly> VisitConditional(ConditionalResult result)
        {
            return
                IfElse
                (
                    VisitExpr(result.Condition),
                    VisitExpr(result.Expr1),
                    VisitExpr(result.Expr2),
                    result.Labels[0],
                    result.Labels[1]
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitLetInBind(LetInBindResult result)
        {
            return
                VisitExpr(result.VarBind.Expr)
                .Concat(result.VarBind.Vars.Reverse().SelectMany(x => Set(x)))
                .Concat(VisitExpr(result.Expr));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitFunc(FuncResult result)
        {
            return Indirect(result.Func);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitMethod(MethodResult result)
        {
            return Enumerable.Empty<TeuchiUdonAssembly>();
        }
        
        protected IEnumerable<TeuchiUdonAssembly> VisitElementsArrayExprToGetSetters(ArrayCtorResult ctor, ElementsArrayExprResult result)
        {
            return ArrayElementsCtor
            (
                result.Exprs.Select(x => VisitExpr(x)),
                result.Literals["1"],
                ctor.TmpValues["array"  ],
                ctor.TmpValues["counter"],
                ctor.Methods["setter"],
                ctor.Methods["addition"]
            );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitRangeArrayExprToGetSetters(ArrayCtorResult ctor, RangeArrayExprResult result)
        {
            return Enumerable.Empty<TeuchiUdonAssembly>();
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitSteppedRangeArrayExprToGetSetters(ArrayCtorResult ctor, SteppedRangeArrayExprResult result)
        {
            return Enumerable.Empty<TeuchiUdonAssembly>();
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitSpreadArrayExprToGetSetters(ArrayCtorResult ctor, SpreadArrayExprResult result)
        {
            return Enumerable.Empty<TeuchiUdonAssembly>();
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitElementsArrayExprToGetLengths(ElementsArrayExprResult result)
        {
            return ArrayElementsCtorLength(result.Literals["length"]);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitRangeArrayExprToGetLengths(RangeArrayExprResult result)
        {
            return Enumerable.Empty<TeuchiUdonAssembly>();
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitSteppedRangeArrayExprToGetLengths(SteppedRangeArrayExprResult result)
        {
            return Enumerable.Empty<TeuchiUdonAssembly>();
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitSpreadArrayExprToGetLengths(SpreadArrayExprResult result)
        {
            return Enumerable.Empty<TeuchiUdonAssembly>();
        }

        public IEnumerable<TeuchiUdonAssembly> DeclIndirectAddresses(IEnumerable<(TeuchiUdonIndirect indirect, uint address)> pairs)
        {
            return pairs.SelectMany(x => DeclData(x.indirect, new AssemblyLiteral_NULL()));
        }
    }
}
