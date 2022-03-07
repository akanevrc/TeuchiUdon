using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonCompilerStrategy
    {
        public static TeuchiUdonCompilerStrategy Instance { get; } = new TeuchiUdonCompilerStrategy();

        protected TeuchiUdonCompilerStrategy()
        {
        }

        public void Init()
        {
        }

        public IEnumerable<TeuchiUdonAssembly> GetDataPartFromTables()
        {
            return
                TeuchiUdonTables.Instance.PublicVars.Keys.SelectMany(x => x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_EXPORT_DATA(new TextLabel(x.GetFullLabel()))
                    })
                .Concat(TeuchiUdonTables.Instance.SyncedVars.SelectMany(x => x.Key.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_SYNC_DATA(new TextLabel(x.Key.GetFullLabel()), TeuchiUdonAssemblySyncMode.Create(x.Value))
                    }))
                .Concat(TeuchiUdonTables.Instance.Vars.Values.SelectMany(x =>
                    x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(x, x.Type, new AssemblyLiteral_NULL())
                    }))
                .Concat(TeuchiUdonTables.Instance.Literals.Values.SelectMany(x =>
                    x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(x, x.Type, new AssemblyLiteral_NULL())
                    }))
                .Concat(TeuchiUdonTables.Instance.This.Values.SelectMany(x =>
                    x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(x, x.Type, new AssemblyLiteral_THIS())
                    }))
                .Concat(TeuchiUdonTables.Instance.Funcs.Values.SelectMany(x =>
                    x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(x.Return, x.Type, new AssemblyLiteral_NULL())
                    }));
        }

        public IEnumerable<TeuchiUdonAssembly> GetDataPartFromOutValuePool()
        {
            return
                TeuchiUdonOutValuePool.Instance.OutValues.Values.SelectMany(x =>
                x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                new TeuchiUdonAssembly[0] :
                new TeuchiUdonAssembly[]
                {
                    new Assembly_DECL_DATA(x, x.Type, new AssemblyLiteral_NULL())
                });
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
                TeuchiUdonTables.Instance.Funcs.Values.Select(x =>
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_LABEL (x),
                        new Assembly_INDENT(1),
                    }
                    .Concat(new TeuchiUdonAssembly[]
                    {
                        new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x.Return)),
                        new Assembly_COPY()
                    })
                    .Concat(x.Vars.Reverse().SelectMany(y =>
                        y.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                        new TeuchiUdonAssembly[0] :
                        new TeuchiUdonAssembly[]
                        {
                            new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(y)),
                            new Assembly_COPY()
                        }
                    ))
                    .Concat(VisitResult(x.Expr))
                    .Concat(new TeuchiUdonAssembly[]
                        {
                            new Assembly_JUMP_INDIRECT(new AssemblyAddress_DATA_LABEL(x.Return)),
                            new Assembly_INDENT(-1)
                        }
                    )
                )
                .Aggregate((acc, x) => acc
                    .Concat(new TeuchiUdonAssembly[] { new Assembly_NEW_LINE() })
                    .Concat(x)
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitResult(TeuchiUdonParserResult result)
        {
            if (result is BodyResult             body            ) return VisitBody            (body);
            if (result is TopBindResult          topBind         ) return VisitTopBind         (topBind);
            if (result is TopExprResult          topExpr         ) return VisitTopExpr         (topExpr);
            if (result is InitVarAttrResult      initVarAttr     ) return VisitInitVarAttr     (initVarAttr);
            if (result is PublicVarAttrResult    publicVarAttr   ) return VisitPublicVarAttr   (publicVarAttr);
            if (result is SyncVarAttrResult      syncVarAttr     ) return VisitSyncVarAttr     (syncVarAttr);
            if (result is InitExprAttrResult     initExprAttr    ) return VisitInitExprAttr    (initExprAttr);
            if (result is VarBindResult          varBind         ) return VisitVarBind         (varBind);
            if (result is VarDeclResult          varDecl         ) return VisitVarDecl         (varDecl);
            if (result is QualifiedVarResult     qualifiedVar    ) return VisitQualifiedVar    (qualifiedVar);
            if (result is IdentifierResult       identifier      ) return VisitIdentifier      (identifier);
            if (result is JumpResult             jump            ) return VisitJump            (jump);
            if (result is LetBindResult          letBind         ) return VisitLetBind         (letBind);
            if (result is ExprResult             expr            ) return VisitExpr            (expr);
            if (result is BottomResult           bottom          ) return VisitBottom          (bottom);
            if (result is UnknownTypeResult      unknownType     ) return VisitUnknownType     (unknownType);
            if (result is UnitResult             unit            ) return VisitUnit            (unit);
            if (result is BlockResult            block           ) return VisitBlock           (block);
            if (result is ParenResult            paren           ) return VisitParen           (paren);
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
            if (result is EvalVarCandidateResult evalVarCandidate) return VisitEvalVarCandidate(evalVarCandidate);
            if (result is ArgExprResult          argExpr         ) return VisitArgExpr         (argExpr);
            if (result is PrefixResult           prefix          ) return VisitPrefix          (prefix);
            if (result is InfixResult            infix           ) return VisitInfix           (infix);
            if (result is ConditionalResult      conditional     ) return VisitConditional     (conditional);
            if (result is LetInBindResult        letInBind       ) return VisitLetInBind       (letInBind);
            if (result is FuncResult             func            ) return VisitFunc            (func);
            if (result is MethodResult           method          ) return VisitMethod          (method);
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
                topEventStats.Select(x =>
                {
                    var v =
                        TeuchiUdonTables.Instance.Vars.ContainsKey(new TeuchiUdonVar(TeuchiUdonQualifier.Top, x.Key)) ?
                        TeuchiUdonTables.Instance.Vars[new TeuchiUdonVar(TeuchiUdonQualifier.Top, x.Key)] :
                        null;
                    return
                        new TeuchiUdonAssembly[]
                        {
                            new Assembly_EXPORT_CODE(new TextLabel(x.Value.eventName)),
                            new Assembly_LABEL      (new TextLabel(x.Value.eventName)),
                            new Assembly_INDENT(1)
                        }
                        .Concat(x.Value.stats.SelectMany(y => VisitResult(y)))
                        .Concat(v?.Type.LogicalTypeNameEquals(TeuchiUdonType.Func) ?? false ?
                            x.Value.ev.OutParamUdonNames.SelectMany(y =>
                                new TeuchiUdonAssembly[]
                                {
                                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(new TextLabel(TeuchiUdonTables.GetEventParamName(x.Key, y))))
                                }
                            )
                            .Concat(new TeuchiUdonAssembly[]
                            {
                                new Assembly_PUSH(new AssemblyAddress_INDIRECT_LABEL(new TextLabel($"topcall[{x.Value.eventName}]"))),
                                new Assembly_JUMP_INDIRECT(new AssemblyAddress_DATA_LABEL(v)),
                                new Assembly_LABEL(new TextLabel($"topcall[{x.Value.eventName}]"))
                            }) :
                            new TeuchiUdonAssembly[0]
                        )
                        .Concat(new TeuchiUdonAssembly[]
                        {
                            new Assembly_JUMP(new AssemblyAddress_NUMBER(0xFFFFFFFC)),
                            new Assembly_INDENT(-1)
                        });
                })
                .Aggregate((acc, x) => acc
                    .Concat(new TeuchiUdonAssembly[] { new Assembly_NEW_LINE() })
                    .Concat(x)
                );
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitTopBind(TopBindResult result)
        {
            return
                VisitExpr(result.VarBind.Expr)
                .Concat(result.VarBind.Vars.Reverse().SelectMany(x =>
                    x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)),
                        new Assembly_COPY()
                    }
                ));
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
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_JUMP_INDIRECT(new AssemblyAddress_DATA_LABEL(result.Label))
                });
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitLetBind(LetBindResult result)
        {
            return
                VisitExpr(result.VarBind.Expr)
                .Concat(result.VarBind.Vars.Reverse().SelectMany(x =>
                    x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)),
                        new Assembly_COPY()
                    }
                ));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitExpr(ExprResult result)
        {
            return
                VisitResult(result.Inner)
                .Concat
                (
                    result.ReturnsValue || result.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                        new TeuchiUdonAssembly[0] :
                        new TeuchiUdonAssembly[]
                        {
                            new Assembly_POP()
                        }
                );
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

        protected IEnumerable<TeuchiUdonAssembly> VisitLiteral(LiteralResult result)
        {
            return
                result.Literal.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                new TeuchiUdonAssembly[0] :
                new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(result.Literal))
                };
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitThis(ThisResult result)
        {
            return
                result.This.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                new TeuchiUdonAssembly[0] :
                new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(result.This))
                };
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalVar(EvalVarResult result)
        {
            return
                result.Var.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                new TeuchiUdonAssembly[0] :
                new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(result.Var))
                };
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
            return
                result.OutValues.SelectMany(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) })
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(result.Method)
                })
                .Concat(result.OutValues.SelectMany(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) }));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalSetter(EvalSetterResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalGetterSetter(EvalGetterSetterResult result)
        {
            return
                result.OutValues.SelectMany(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) })
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(result.Getter)
                })
                .Concat(result.OutValues.SelectMany(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) }));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalFunc(EvalFuncResult result)
        {
            return
                VisitExpr(result.Expr)
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(result.OutValue)),
                    new Assembly_COPY()
                })
                .Concat(result.Args.SelectMany(x => VisitExpr(x)))
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_INDIRECT_LABEL(result.EvalFunc)),
                    new Assembly_JUMP_INDIRECT(new AssemblyAddress_DATA_LABEL(result.OutValue)),
                    new Assembly_LABEL(result.EvalFunc)
                });
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalMethod(EvalMethodResult result)
        {
            return
                VisitExpr(result.Expr)
                .Concat(result.Args.SelectMany(x => VisitExpr(x)))
                .Concat(result.OutValues.SelectMany(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) }))
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(result.Method)
                })
                .Concat(result.OutValues.SelectMany(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) }));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalVarCandidate(EvalVarCandidateResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitArgExpr(ArgExprResult result)
        {
            return new TeuchiUdonAssembly[0];
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
                        EvalPrefix
                        (
                            result.Methods[0],
                            VisitExpr(result.Expr),
                            result.OutValuess[0].SelectMany(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) })
                        );
                case "~":
                    return
                        result.Methods[0] == null || result.Literals[0] == null ? new TeuchiUdonAssembly[0] :
                        EvalPrefix
                        (
                            result.Methods[0],
                            VisitExpr(result.Expr)
                            .Concat(new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(result.Literals[0])) }),
                            result.OutValuess[0].SelectMany(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) })
                        );
                default:
                    return new TeuchiUdonAssembly[0];
            }
        }

        private IEnumerable<TeuchiUdonAssembly> EvalPrefix
        (
            TeuchiUdonMethod method,
            IEnumerable<TeuchiUdonAssembly> inValues,
            IEnumerable<TeuchiUdonAssembly> outValues
        )
        {
            return
                inValues
                .Concat(outValues)
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(method)
                })
                .Concat(outValues);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitInfix(InfixResult result)
        {
            switch (result.Op)
            {
                case ".":
                    return VisitExpr(result.Expr1).Concat(VisitExpr(result.Expr2));
                case "?.":
                    return
                        result.Methods.Length == 1 && result.Methods[0] == null || result.Literals[0] == null ? new TeuchiUdonAssembly[0] :
                        result.Methods.Length == 0 ? new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(result.Literals[0])) } :
                        EvalInfixCoalescingAccess
                        (
                            result.Methods[0],
                            VisitExpr(result.Expr1),
                            VisitExpr(result.Expr2),
                            new TeuchiUdonAssembly[] { new Assembly_PUSH         (new AssemblyAddress_DATA_LABEL(result.OutValuess[0][0])) },
                            new TeuchiUdonAssembly[] { new Assembly_PUSH         (new AssemblyAddress_DATA_LABEL(result.OutValuess[1][0])) },
                            new TeuchiUdonAssembly[] { new Assembly_PUSH         (new AssemblyAddress_DATA_LABEL(result.Literals[0])) },
                            new TeuchiUdonAssembly[] { new Assembly_JUMP_IF_FALSE(new AssemblyAddress_CODE_LABEL(result.Labels[0])) },
                            new TeuchiUdonAssembly[] { new Assembly_JUMP         (new AssemblyAddress_CODE_LABEL(result.Labels[1])) },
                            new TeuchiUdonAssembly[] { new Assembly_LABEL        (result.Labels[0]) },
                            new TeuchiUdonAssembly[] { new Assembly_LABEL        (result.Labels[1]) }
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
                        EvalInfix
                        (
                            result.Methods[0],
                            VisitExpr(result.Expr1).Concat(VisitExpr(result.Expr2)),
                            result.OutValuess[0].SelectMany(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) })
                        );
                case "==":
                case "!=":
                    return
                        result.Methods.Length == 1 && result.Methods[0] == null || result.Literals[0] == null ? new TeuchiUdonAssembly[0] :
                        result.Methods.Length == 0 ? new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(result.Literals[0])) } :
                        EvalInfix
                        (
                            result.Methods[0],
                            VisitExpr(result.Expr1).Concat(VisitExpr(result.Expr2)),
                            result.OutValuess[0].SelectMany(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) })
                        );
                case "&&":
                    return
                        EvalInfixConditionalAnd
                        (
                            VisitExpr(result.Expr1),
                            VisitExpr(result.Expr2),
                            new TeuchiUdonAssembly[] { new Assembly_PUSH         (new AssemblyAddress_DATA_LABEL(result.Literals[0])) },
                            new TeuchiUdonAssembly[] { new Assembly_JUMP_IF_FALSE(new AssemblyAddress_CODE_LABEL(result.Labels[0])) },
                            new TeuchiUdonAssembly[] { new Assembly_JUMP         (new AssemblyAddress_CODE_LABEL(result.Labels[1])) },
                            new TeuchiUdonAssembly[] { new Assembly_LABEL        (result.Labels[0]) },
                            new TeuchiUdonAssembly[] { new Assembly_LABEL        (result.Labels[1]) }
                        );
                case "||":
                    return
                        EvalInfixConditionalOr
                        (
                            VisitExpr(result.Expr1),
                            VisitExpr(result.Expr2),
                            new TeuchiUdonAssembly[] { new Assembly_PUSH         (new AssemblyAddress_DATA_LABEL(result.Literals[0])) },
                            new TeuchiUdonAssembly[] { new Assembly_JUMP_IF_FALSE(new AssemblyAddress_CODE_LABEL(result.Labels[0])) },
                            new TeuchiUdonAssembly[] { new Assembly_JUMP         (new AssemblyAddress_CODE_LABEL(result.Labels[1])) },
                            new TeuchiUdonAssembly[] { new Assembly_LABEL        (result.Labels[0]) },
                            new TeuchiUdonAssembly[] { new Assembly_LABEL        (result.Labels[1]) }
                        );
                case "??":
                    return
                        result.Methods.Length == 1 && result.Methods[0] == null || result.Literals[0] == null ? new TeuchiUdonAssembly[0] :
                        result.Methods.Length == 0 ? VisitExpr(result.Expr2) :
                        EvalInfixCoalescing
                        (
                            result.Methods[0],
                            VisitExpr(result.Expr1),
                            VisitExpr(result.Expr2),
                            new TeuchiUdonAssembly[] { new Assembly_PUSH         (new AssemblyAddress_DATA_LABEL(result.OutValuess[0][0])) },
                            new TeuchiUdonAssembly[] { new Assembly_PUSH         (new AssemblyAddress_DATA_LABEL(result.OutValuess[1][0])) },
                            new TeuchiUdonAssembly[] { new Assembly_PUSH         (new AssemblyAddress_DATA_LABEL(result.Literals[0])) },
                            new TeuchiUdonAssembly[] { new Assembly_JUMP_IF_FALSE(new AssemblyAddress_CODE_LABEL(result.Labels[0])) },
                            new TeuchiUdonAssembly[] { new Assembly_JUMP         (new AssemblyAddress_CODE_LABEL(result.Labels[1])) },
                            new TeuchiUdonAssembly[] { new Assembly_LABEL        (result.Labels[0]) },
                            new TeuchiUdonAssembly[] { new Assembly_LABEL        (result.Labels[1]) }
                        );
                case "<-":
                {
                    return
                        result.Expr1.Inner.LeftValues.Length == 1 && result.Expr1.Inner.LeftValues[0] is TeuchiUdonVar ?
                        EvalInfixAssign
                        (
                            VisitExpr(result.Expr1),
                            VisitExpr(result.Expr2)
                        ) :
                        result.Expr1.Inner.LeftValues.Length == 1 && result.Expr1.Inner.LeftValues[0] is TeuchiUdonMethod m ?
                        EvalInfixAssignMethod
                        (
                            VisitExpr(result.Expr1.Inner.Instance),
                            new TeuchiUdonAssembly[] { new Assembly_EXTERN(m) },
                            VisitExpr(result.Expr2)
                        ) :
                        new TeuchiUdonAssembly[0];
                }
                default:
                    return new TeuchiUdonAssembly[0];
            }
        }

        private IEnumerable<TeuchiUdonAssembly> EvalInfix
        (
            TeuchiUdonMethod method,
            IEnumerable<TeuchiUdonAssembly> inValues,
            IEnumerable<TeuchiUdonAssembly> outValues
        )
        {
            return
                inValues
                .Concat(outValues)
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(method)
                })
                .Concat(outValues);
        }

        private IEnumerable<TeuchiUdonAssembly> EvalInfixConditionalAnd
        (
            IEnumerable<TeuchiUdonAssembly> value1,
            IEnumerable<TeuchiUdonAssembly> value2,
            IEnumerable<TeuchiUdonAssembly> literal,
            IEnumerable<TeuchiUdonAssembly> jumpIfFalse,
            IEnumerable<TeuchiUdonAssembly> jump,
            IEnumerable<TeuchiUdonAssembly> label1,
            IEnumerable<TeuchiUdonAssembly> label2
        )
        {
            return
                value1
                .Concat(jumpIfFalse)
                .Concat(value2)
                .Concat(jump)
                .Concat(label1)
                .Concat(literal)
                .Concat(label2);
        }

        private IEnumerable<TeuchiUdonAssembly> EvalInfixConditionalOr
        (
            IEnumerable<TeuchiUdonAssembly> value1,
            IEnumerable<TeuchiUdonAssembly> value2,
            IEnumerable<TeuchiUdonAssembly> literal,
            IEnumerable<TeuchiUdonAssembly> jumpIfFalse,
            IEnumerable<TeuchiUdonAssembly> jump,
            IEnumerable<TeuchiUdonAssembly> label1,
            IEnumerable<TeuchiUdonAssembly> label2
        )
        {
            return
                value1
                .Concat(jumpIfFalse)
                .Concat(literal)
                .Concat(jump)
                .Concat(label1)
                .Concat(value2)
                .Concat(label2);
        }

        private IEnumerable<TeuchiUdonAssembly> EvalInfixCoalescing
        (
            TeuchiUdonMethod method,
            IEnumerable<TeuchiUdonAssembly> value1,
            IEnumerable<TeuchiUdonAssembly> value2,
            IEnumerable<TeuchiUdonAssembly> outValue,
            IEnumerable<TeuchiUdonAssembly> tmpValue,
            IEnumerable<TeuchiUdonAssembly> literal,
            IEnumerable<TeuchiUdonAssembly> jumpIfFalse,
            IEnumerable<TeuchiUdonAssembly> jump,
            IEnumerable<TeuchiUdonAssembly> label1,
            IEnumerable<TeuchiUdonAssembly> label2
        )
        {
            return
                value1
                .Concat(tmpValue)
                .Concat(new TeuchiUdonAssembly[] { new Assembly_COPY() })
                .Concat(tmpValue)
                .Concat(literal)
                .Concat(outValue)
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(method)
                })
                .Concat(outValue)
                .Concat(jumpIfFalse)
                .Concat(value2)
                .Concat(jump)
                .Concat(label1)
                .Concat(tmpValue)
                .Concat(label2);
        }

        private IEnumerable<TeuchiUdonAssembly> EvalInfixCoalescingAccess
        (
            TeuchiUdonMethod method,
            IEnumerable<TeuchiUdonAssembly> value1,
            IEnumerable<TeuchiUdonAssembly> value2,
            IEnumerable<TeuchiUdonAssembly> outValue,
            IEnumerable<TeuchiUdonAssembly> tmpValue,
            IEnumerable<TeuchiUdonAssembly> literal,
            IEnumerable<TeuchiUdonAssembly> jumpIfFalse,
            IEnumerable<TeuchiUdonAssembly> jump,
            IEnumerable<TeuchiUdonAssembly> label1,
            IEnumerable<TeuchiUdonAssembly> label2
        )
        {
            return
                value1
                .Concat(tmpValue)
                .Concat(new TeuchiUdonAssembly[] { new Assembly_COPY() })
                .Concat(tmpValue)
                .Concat(literal)
                .Concat(outValue)
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(method)
                })
                .Concat(outValue)
                .Concat(jumpIfFalse)
                .Concat(literal)
                .Concat(jump)
                .Concat(label1)
                .Concat(tmpValue)
                .Concat(value2)
                .Concat(label2);
        }

        private IEnumerable<TeuchiUdonAssembly> EvalInfixAssign
        (
            IEnumerable<TeuchiUdonAssembly> value1,
            IEnumerable<TeuchiUdonAssembly> value2
        )
        {
            return
                value2
                .Concat(value1)
                .Concat(new TeuchiUdonAssembly[] { new Assembly_COPY() });
        }

        private IEnumerable<TeuchiUdonAssembly> EvalInfixAssignMethod
        (
            IEnumerable<TeuchiUdonAssembly> instance,
            IEnumerable<TeuchiUdonAssembly> setterMethod,
            IEnumerable<TeuchiUdonAssembly> value2
        )
        {
            return
                instance
                .Concat(value2)
                .Concat(setterMethod);
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitConditional(ConditionalResult result)
        {
            return
                VisitExpr(result.Condition)
                .Concat(new TeuchiUdonAssembly[] { new Assembly_JUMP_IF_FALSE(new AssemblyAddress_CODE_LABEL(result.Labels[0])) })
                .Concat(VisitExpr(result.Expr1))
                .Concat(new TeuchiUdonAssembly[]
                    {
                        new Assembly_JUMP(new AssemblyAddress_CODE_LABEL(result.Labels[1])),
                        new Assembly_LABEL(result.Labels[0])
                    })
                .Concat(VisitExpr(result.Expr2))
                .Concat(new TeuchiUdonAssembly[] { new Assembly_LABEL(result.Labels[1]) });
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitLetInBind(LetInBindResult result)
        {
            return
                VisitExpr(result.VarBind.Expr)
                .Concat(result.VarBind.Vars.Reverse().SelectMany(x =>
                    x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)),
                        new Assembly_COPY()
                    }
                ))
                .Concat(VisitExpr(result.Expr));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitFunc(FuncResult result)
        {
            return new TeuchiUdonAssembly[]
            {
                new Assembly_PUSH(new AssemblyAddress_INDIRECT_LABEL(result.Func))
            };
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitMethod(MethodResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        public IEnumerable<TeuchiUdonAssembly> DeclIndirectAddresses(IEnumerable<(TeuchiUdonIndirect indirect, uint address)> pairs)
        {
            return pairs.SelectMany(x => new TeuchiUdonAssembly[]
            {
                new Assembly_DECL_DATA(x.indirect, TeuchiUdonType.UInt, new AssemblyLiteral_NULL())
            });
        }
    }
}
