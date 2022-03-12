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

        protected abstract IEnumerable<TeuchiUdonAssembly> ExportData(ITypedLabel label);
        protected abstract IEnumerable<TeuchiUdonAssembly> SyncData(ITypedLabel label, TeuchiUdonSyncMode mode);
        protected abstract IEnumerable<TeuchiUdonAssembly> DeclData(ITypedLabel label, TeuchiUdonAssemblyLiteral literal);
        protected abstract IEnumerable<TeuchiUdonAssembly> Pop();
        protected abstract IEnumerable<TeuchiUdonAssembly> Get(ITeuchiUdonLabel label);
        protected abstract IEnumerable<TeuchiUdonAssembly> Indirect(ITeuchiUdonLabel label);
        protected abstract IEnumerable<TeuchiUdonAssembly> Set(ITeuchiUdonLabel label);
        protected abstract IEnumerable<TeuchiUdonAssembly> Jump(ITeuchiUdonLabel label);
        protected abstract IEnumerable<TeuchiUdonAssembly> Func(TeuchiUdonFunc func);
        protected abstract IEnumerable<TeuchiUdonAssembly> Event(string varName, string eventName, TeuchiUdonMethod ev, List<TopStatementResult> stats);
        protected abstract IEnumerable<TeuchiUdonAssembly> EvalFunc
        (
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> args,
            ITeuchiUdonLabel evalFunc,
            ITypedLabel funcAddress
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> EvalMethod
        (
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> inValues,
            IEnumerable<TeuchiUdonOutValue> outValues,
            TeuchiUdonMethod method
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> IfElse
        (
            IEnumerable<TeuchiUdonAssembly> condition,
            IEnumerable<TeuchiUdonAssembly> truePart,
            IEnumerable<TeuchiUdonAssembly> falsePart,
            ITeuchiUdonLabel label1,
            ITeuchiUdonLabel label2
        );
        protected abstract IEnumerable<TeuchiUdonAssembly> EvalAssign(IEnumerable<TeuchiUdonAssembly> value1, IEnumerable<TeuchiUdonAssembly> value2);
        protected abstract IEnumerable<TeuchiUdonAssembly> EvalSetterAssign
        (
            IEnumerable<TeuchiUdonAssembly> instance,
            IEnumerable<TeuchiUdonAssembly> value2,
            TeuchiUdonMethod setterMethod
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

        public IEnumerable<TeuchiUdonAssembly> GetCodePartFromResult(TeuchiUdonParserResult result)
        {
            return VisitResult(result);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitTables()
        {
            return
                TeuchiUdonTables.Instance.Funcs.Values.Count == 0 ? new TeuchiUdonAssembly[0] :
                TeuchiUdonTables.Instance.Funcs.Values.Select(x => Func(x))
                .Aggregate((acc, x) => acc
                    .Concat(new TeuchiUdonAssembly[] { new Assembly_NEW_LINE() })
                    .Concat(x)
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitResult(TeuchiUdonParserResult result)
        {
            if (result is BodyResult                 body                ) return VisitBody                (body);
            if (result is TopBindResult              topBind             ) return VisitTopBind             (topBind);
            if (result is TopExprResult              topExpr             ) return VisitTopExpr             (topExpr);
            if (result is InitVarAttrResult          initVarAttr         ) return VisitInitVarAttr         (initVarAttr);
            if (result is PublicVarAttrResult        publicVarAttr       ) return VisitPublicVarAttr       (publicVarAttr);
            if (result is SyncVarAttrResult          syncVarAttr         ) return VisitSyncVarAttr         (syncVarAttr);
            if (result is InitExprAttrResult         initExprAttr        ) return VisitInitExprAttr        (initExprAttr);
            if (result is VarBindResult              varBind             ) return VisitVarBind             (varBind);
            if (result is VarDeclResult              varDecl             ) return VisitVarDecl             (varDecl);
            if (result is QualifiedVarResult         qualifiedVar        ) return VisitQualifiedVar        (qualifiedVar);
            if (result is IdentifierResult           identifier          ) return VisitIdentifier          (identifier);
            if (result is JumpResult                 jump                ) return VisitJump                (jump);
            if (result is LetBindResult              letBind             ) return VisitLetBind             (letBind);
            if (result is ExprResult                 expr                ) return VisitExpr                (expr);
            if (result is BottomResult               bottom              ) return VisitBottom              (bottom);
            if (result is UnknownTypeResult          unknownType         ) return VisitUnknownType         (unknownType);
            if (result is UnitResult                 unit                ) return VisitUnit                (unit);
            if (result is BlockResult                block               ) return VisitBlock               (block);
            if (result is ParenResult                paren               ) return VisitParen               (paren);
            if (result is ListCtorResult             listCtor            ) return VisitListCtor            (listCtor);
            if (result is ElementListExprResult      elementListExpr     ) return VisitElementListExpr     (elementListExpr);
            if (result is RangeListExprResult        rangeListExpr       ) return VisitRangeListExpr       (rangeListExpr);
            if (result is SteppedRangeListExprResult steppedRangeListExpr) return VisitSteppedRangeListExpr(steppedRangeListExpr);
            if (result is SpreadListExprResult       spreadListExpr      ) return VisitSpreadListExpr      (spreadListExpr);
            if (result is LiteralResult              literal             ) return VisitLiteral             (literal);
            if (result is ThisResult                 ths                 ) return VisitThis                (ths);
            if (result is EvalVarResult              evalVar             ) return VisitEvalVar             (evalVar);
            if (result is EvalTypeResult             evalType            ) return VisitEvalType            (evalType);
            if (result is EvalQualifierResult        evalQualifier       ) return VisitEvalQualifier       (evalQualifier);
            if (result is EvalGetterResult           evalGetter          ) return VisitEvalGetter          (evalGetter);
            if (result is EvalSetterResult           evalSetter          ) return VisitEvalSetter          (evalSetter);
            if (result is EvalGetterSetterResult     evalGetterSetter    ) return VisitEvalGetterSetter    (evalGetterSetter);
            if (result is EvalFuncResult             evalFunc            ) return VisitEvalFunc            (evalFunc);
            if (result is EvalMethodResult           evalMethod          ) return VisitEvalMethod          (evalMethod);
            if (result is EvalVarCandidateResult     evalVarCandidate    ) return VisitEvalVarCandidate    (evalVarCandidate);
            if (result is ArgExprResult              argExpr             ) return VisitArgExpr             (argExpr);
            if (result is TypeCastResult             typeCast            ) return VisitTypeCast            (typeCast);
            if (result is ConvertCastResult          convertCast         ) return VisitConvertCast         (convertCast);
            if (result is PrefixResult               prefix              ) return VisitPrefix              (prefix);
            if (result is InfixResult                infix               ) return VisitInfix               (infix);
            if (result is ConditionalResult          conditional         ) return VisitConditional         (conditional);
            if (result is LetInBindResult            letInBind           ) return VisitLetInBind           (letInBind);
            if (result is FuncResult                 func                ) return VisitFunc                (func);
            if (result is MethodResult               method              ) return VisitMethod              (method);
            throw new InvalidOperationException("unsupported parser result type");
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
                topEventStats.Count == 0 ? new TeuchiUdonAssembly[0] :
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

        protected IEnumerable<TeuchiUdonAssembly> VisitInitVarAttr(InitVarAttrResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitPublicVarAttr(PublicVarAttrResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitSyncVarAttr(SyncVarAttrResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitInitExprAttr(InitExprAttrResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitVarBind(VarBindResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitVarDecl(VarDeclResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitQualifiedVar(QualifiedVarResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitIdentifier(IdentifierResult result)
        {
            return new TeuchiUdonAssembly[0];
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
                VisitResult(result.Inner)
                .Concat(result.ReturnsValue || result.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ? new TeuchiUdonAssembly[0] : Pop());
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitBottom(BottomResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitUnknownType(UnknownTypeResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitUnit(UnitResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitBlock(BlockResult result)
        {
            return result.Statements.SelectMany(x => VisitResult(x)).Concat(VisitExpr(result.Expr));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitParen(ParenResult result)
        {
            return VisitExpr(result.Expr);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitListCtor(ListCtorResult result)
        {
            return new TeuchiUdonAssembly[0];
        }
        
        protected IEnumerable<TeuchiUdonAssembly> VisitElementListExpr(ElementListExprResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitRangeListExpr(RangeListExprResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitSteppedRangeListExpr(SteppedRangeListExprResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitSpreadListExpr(SpreadListExprResult result)
        {
            return new TeuchiUdonAssembly[0];
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
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalQualifier(EvalQualifierResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalGetter(EvalGetterResult result)
        {
            return EvalMethod(new TeuchiUdonAssembly[0][], result.OutValues, result.Method);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalSetter(EvalSetterResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalGetterSetter(EvalGetterSetterResult result)
        {
            return EvalMethod(new TeuchiUdonAssembly[0][], result.OutValues, result.Getter);
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

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalVarCandidate(EvalVarCandidateResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitTypeCast(TypeCastResult result)
        {
            return VisitExpr(result.Expr).Concat(VisitExpr(result.Arg));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitConvertCast(ConvertCastResult result)
        {
            return
                result.Methods.Any(x => x == null) ? new TeuchiUdonAssembly[0] :
                VisitExpr(result.Expr)
                .Concat(EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { VisitExpr(result.Arg) }, result.OutValuess[0], result.Methods[0]));
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
                        result.Methods[0] == null ? new TeuchiUdonAssembly[0] :
                        EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { VisitExpr(result.Expr) }, result.OutValuess[0], result.Methods[0]);
                case "~":
                    return
                        result.Methods[0] == null ? new TeuchiUdonAssembly[0] :
                        EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { VisitExpr(result.Expr), Get(result.Literals[0]) }, result.OutValuess[0], result.Methods[0]);
                default:
                    return new TeuchiUdonAssembly[0];
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
                        result.Methods.Length == 1 && result.Methods[0] == null ? new TeuchiUdonAssembly[0] :
                        result.Methods.Length == 0 ? Get(result.Literals[0]) :
                        IfElse
                        (
                            VisitExpr(result.Expr1)
                            .Concat(Set(result.OutValuess[1][0]))
                            .Concat(EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(result.OutValuess[1][0]), Get(result.Literals[0]) }, result.OutValuess[0], result.Methods[0])),
                            Get(result.Literals[0]),
                            Get(result.OutValuess[1][0]).Concat(VisitExpr(result.Expr2)),
                            result.Labels[0],
                            result.Labels[1]
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
                        result.Methods[0] == null ? new TeuchiUdonAssembly[0] :
                        EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { VisitExpr(result.Expr1), VisitExpr(result.Expr2) }, result.OutValuess[0], result.Methods[0]);
                case "==":
                case "!=":
                    return
                        result.Methods.Length == 1 && result.Methods[0] == null ? new TeuchiUdonAssembly[0] :
                        result.Methods.Length == 0 ? Get(result.Literals[0]) :
                        EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { VisitExpr(result.Expr1), VisitExpr(result.Expr2) }, result.OutValuess[0], result.Methods[0]);
                case "&&":
                    return
                        IfElse
                        (
                            VisitExpr(result.Expr1),
                            VisitExpr(result.Expr2),
                            Get(result.Literals[0]),
                            result.Labels[0],
                            result.Labels[1]
                        );
                case "||":
                    return
                        IfElse
                        (
                            VisitExpr(result.Expr1),
                            Get(result.Literals[0]),
                            VisitExpr(result.Expr2),
                            result.Labels[0],
                            result.Labels[1]
                        );
                case "??":
                    return
                        result.Methods.Length == 1 && result.Methods[0] == null ? new TeuchiUdonAssembly[0] :
                        result.Methods.Length == 0 ? VisitExpr(result.Expr2) :
                        IfElse
                        (
                            VisitExpr(result.Expr1)
                            .Concat(Set(result.OutValuess[1][0]))
                            .Concat(EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(result.OutValuess[1][0]), Get(result.Literals[0]) }, result.OutValuess[0], result.Methods[0])),
                            VisitExpr(result.Expr2),
                            Get(result.OutValuess[1][0]),
                            result.Labels[0],
                            result.Labels[1]
                        );
                case "<-":
                {
                    return
                        result.Expr1.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Unit) || result.Expr1.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                            new TeuchiUdonAssembly[0] :
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
                            new TeuchiUdonAssembly[0];
                }
                default:
                    return new TeuchiUdonAssembly[0];
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
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitArgExpr(ArgExprResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        public IEnumerable<TeuchiUdonAssembly> DeclIndirectAddresses(IEnumerable<(TeuchiUdonIndirect indirect, uint address)> pairs)
        {
            return pairs.SelectMany(x => DeclData(x.indirect, new AssemblyLiteral_NULL()));
        }
    }
}
