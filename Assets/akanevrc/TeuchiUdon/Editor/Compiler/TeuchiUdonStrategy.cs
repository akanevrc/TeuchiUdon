using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonStrategy
    {
        public static TeuchiUdonStrategy Instance { get; } = new TeuchiUdonStrategy();

        protected TeuchiUdonStrategy()
        {
        }

        public void Init()
        {
        }

        public IEnumerable<TeuchiUdonAssembly> GetDataPartFromTables()
        {
            return
                TeuchiUdonTables.Instance.ExportedVars.Keys.SelectMany(x => x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_EXPORT_DATA(new TextLabel(x.Name))
                    })
                .Concat(TeuchiUdonTables.Instance.SyncedVars.SelectMany(x => x.Key.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_SYNC_DATA(new TextLabel(x.Key.Name), TeuchiUdonAssemblySyncMode.Create(x.Value))
                    }))
                .Concat(TeuchiUdonTables.Instance.Vars.Values.SelectMany(x =>
                    x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(x, x.Type, new AssemblyLiteral_NULL())
                    }))
                .Concat(TeuchiUdonTables.Instance.OutValues.Values.SelectMany(x =>
                    x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(x, x.Type, new AssemblyLiteral_NULL())
                    }))
                .Concat(TeuchiUdonTables.Instance.Literals.Values.Except(TeuchiUdonTables.Instance.ExportedVars.Values).SelectMany(x =>
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
            else if (result is TopBindResult          topBind         ) return VisitTopBind         (topBind);
            else if (result is TopExprResult          topExpr         ) return VisitTopExpr         (topExpr);
            else if (result is InitVarAttrResult      initVarAttr     ) return VisitInitVarAttr     (initVarAttr);
            else if (result is ExportVarAttrResult    exportVarAttr   ) return VisitExportVarAttr   (exportVarAttr);
            else if (result is SyncVarAttrResult      syncVarAttr     ) return VisitSyncVarAttr     (syncVarAttr);
            else if (result is InitExprAttrResult     initExprAttr    ) return VisitInitExprAttr    (initExprAttr);
            else if (result is VarBindResult          varBind         ) return VisitVarBind         (varBind);
            else if (result is VarDeclResult          varDecl         ) return VisitVarDecl         (varDecl);
            else if (result is IdentifierResult       identifier      ) return VisitIdentifier      (identifier);
            else if (result is JumpResult             jump            ) return VisitJump            (jump);
            else if (result is LetBindResult          letBind         ) return VisitLetBind         (letBind);
            else if (result is ExprResult             expr            ) return VisitExpr            (expr);
            else if (result is BottomResult           bottom          ) return VisitBottom          (bottom);
            else if (result is UnknownTypeResult      unknownType     ) return VisitUnknownType     (unknownType);
            else if (result is UnitResult             unit            ) return VisitUnit            (unit);
            else if (result is BlockResult            block           ) return VisitBlock           (block);
            else if (result is ParenResult            paren           ) return VisitParen           (paren);
            else if (result is LiteralResult          literal         ) return VisitLiteral         (literal);
            else if (result is ThisResult             ths             ) return VisitThis            (ths);
            else if (result is EvalVarResult          evalVar         ) return VisitEvalVar         (evalVar);
            else if (result is EvalTypeResult         evalType        ) return VisitEvalType        (evalType);
            else if (result is EvalQualifierResult    evalQualifier   ) return VisitEvalQualifier   (evalQualifier);
            else if (result is EvalGetterResult       evalGetter      ) return VisitEvalGetter      (evalGetter);
            else if (result is EvalGetterSetterResult evalGetterSetter) return VisitEvalGetterSetter(evalGetterSetter);
            else if (result is EvalFuncResult         evalFunc        ) return VisitEvalFunc        (evalFunc);
            else if (result is EvalMethodResult       evalMethod      ) return VisitEvalMethod      (evalMethod);
            else if (result is EvalVarCandidateResult evalVarCandidate) return VisitEvalVarCandidate(evalVarCandidate);
            else if (result is PrefixResult           prefix          ) return VisitPrefix          (prefix);
            else if (result is PostfixResult          postfix         ) return VisitPostfix         (postfix);
            else if (result is InfixResult            infix           ) return VisitInfix           (infix);
            else if (result is LetInBindResult        letInBind       ) return VisitLetInBind       (letInBind);
            else if (result is FuncResult             func            ) return VisitFunc            (func);
            else if (result is MethodResult           method          ) return VisitMethod          (method);
            else if (result is SetterResult           setter          ) return VisitSetter          (setter);
            else throw new InvalidOperationException("unsupported parser result type");
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitBody(BodyResult result)
        {
            var topFuncs = result.TopStatements
                .Where(x => x is TopBindResult topBind && topBind.VarBind.Vars.Length == 1 && topBind.VarBind.Vars[0].Type.LogicalTypeNameEquals(TeuchiUdonType.Func))
                .Select(x => (name: ((TopBindResult)x).VarBind.Vars[0].Name, x: (TopBindResult)x));
            var topStats = result.TopStatements
                .Where(x => !(x is TopBindResult topBind && topBind.VarBind.Vars.Length == 1 && topBind.VarBind.Vars[0].Type.LogicalTypeNameEquals(TeuchiUdonType.Func)))
                .Select(x => x is TopBindResult topBind ? (name: topBind.Init, x) : x is TopExprResult topExpr ? (name: topExpr.Init, x) : (name: null, x))
                .Where(x => x.name != null);

            var topFuncStats = new Dictionary<string, List<TopStatementResult>>();
            foreach (var func in topFuncs)
            {
                if (func.name == "_start" || func.x.Export)
                {
                    topFuncStats.Add(func.name, new List<TopStatementResult>() { func.x });
                }
            }
            foreach (var stat in topStats)
            {
                if (!topFuncStats.ContainsKey(stat.name))
                {
                    topFuncStats.Add(stat.name, new List<TopStatementResult>());
                }
                topFuncStats[stat.name].Add(stat.x);
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
                    .Concat(x.Value.SelectMany(y => VisitResult(y)))
                    .Concat(TeuchiUdonTables.Instance.Vars.ContainsKey(new TeuchiUdonVar(TeuchiUdonQualifier.Top, x.Key)) ?
                        new TeuchiUdonAssembly[]
                        {
                            new Assembly_PUSH(new AssemblyAddress_INDIRECT_LABEL(new TextLabel($"topcall[{x.Key}]")))
                        }
                        .Concat(new TeuchiUdonAssembly[]
                            {
                                new Assembly_JUMP_INDIRECT(new AssemblyAddress_DATA_LABEL(TeuchiUdonTables.Instance.Vars[new TeuchiUdonVar(TeuchiUdonQualifier.Top, x.Key)])),
                                new Assembly_LABEL(new TextLabel($"topcall[{x.Key}]"))
                            }
                        ) :
                        new TeuchiUdonAssembly[0]
                    )
                    .Concat(new TeuchiUdonAssembly[]
                    {
                        new Assembly_JUMP(new AssemblyAddress_NUMBER(0xFFFFFFFC)),
                        new Assembly_INDENT(-1)
                    }))
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

        protected IEnumerable<TeuchiUdonAssembly> VisitExportVarAttr(ExportVarAttrResult result)
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
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalGetterSetter(EvalGetterSetterResult result)
        {
            return new TeuchiUdonAssembly[0];
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
                result.Method.SortAlongParams
                (
                    result.Args.Select(x => VisitExpr(x)),
                    result.OutValues.Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) })
                )
                .SelectMany(x => x)
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
                        EvalPrefixMethod
                        (
                            result.Methods[0],
                            new TeuchiUdonAssembly[][]
                            {
                                VisitExpr(result.Expr).ToArray()
                            },
                            result.OutValuess[0].Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) })
                        );
                case "~":
                    return
                        result.Methods[0] == null || result.Literals[0] == null ? new TeuchiUdonAssembly[0] :
                        EvalPrefixMethod
                        (
                            result.Methods[0],
                            new TeuchiUdonAssembly[][]
                            {
                                VisitExpr(result.Expr).ToArray(),
                                new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(result.Literals[0])) }
                            },
                            result.OutValuess[0].Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) })
                        );
                case "++":
                case "--":
                    return
                        result.Methods[0] == null || result.Literals[0] == null ? new TeuchiUdonAssembly[0] :
                        EvalPrefixAssignMethod
                        (
                            result.Methods[0],
                            new TeuchiUdonAssembly[][]
                            {
                                VisitExpr(result.Expr).ToArray(),
                                new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(result.Literals[0])) }
                            },
                            result.OutValuess[0].Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) }),
                            result.Expr.Inner.LeftValues.Select(x => new TeuchiUdonAssembly[]
                                {
                                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)),
                                    new Assembly_COPY()
                                }
                            )
                        );
                default:
                    return new TeuchiUdonAssembly[0];
            }
        }

        private IEnumerable<TeuchiUdonAssembly> EvalPrefixMethod
        (
            TeuchiUdonMethod method,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> inValues,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> outValues
        )
        {
            return
                method.SortAlongParams(inValues, outValues)
                .SelectMany(x => x)
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(method)
                })
                .Concat(outValues.SelectMany(x => x));
        }

        private IEnumerable<TeuchiUdonAssembly> EvalPrefixAssignMethod
        (
            TeuchiUdonMethod method,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> inValues,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> outValues,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> setValues
        )
        {
            return
                method.SortAlongParams
                (
                    inValues,
                    outValues
                )
                .SelectMany(x => x)
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(method)
                })
                .Concat(outValues.SelectMany(x => x))
                .Concat(setValues.SelectMany(x => x))
                .Concat(outValues.SelectMany(x => x));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitPostfix(PostfixResult result)
        {
            switch (result.Op)
            {
                case "++":
                case "--":
                    return
                        result.Methods[0] == null || result.Literals[0] == null ? new TeuchiUdonAssembly[0] :
                        EvalPostfixAssignMethod
                        (
                            result.Methods[0],
                            new TeuchiUdonAssembly[][]
                            {
                                VisitExpr(result.Expr).ToArray()
                            },
                            new TeuchiUdonAssembly[][]
                            {
                                new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(result.Literals[0])) }
                            },
                            result.TmpValuess[0].Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) }),
                            result.OutValuess[0].Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) }),
                            result.Expr.Inner.LeftValues.Select(x => new TeuchiUdonAssembly[]
                                {
                                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)),
                                    new Assembly_COPY()
                                }
                            )
                        );
                default:
                    return new TeuchiUdonAssembly[0];
            }
        }

        private IEnumerable<TeuchiUdonAssembly> EvalPostfixAssignMethod
        (
            TeuchiUdonMethod method,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> inValues,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> argValues,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> tmpValues,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> outValues,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> setValues
        )
        {
            return
                inValues
                .Zip(tmpValues, (i, t) => (i, t))
                .SelectMany(x => x.i.Concat(x.t).Concat(new TeuchiUdonAssembly[] { new Assembly_COPY() }))
                .Concat(method.SortAlongParams(tmpValues.Concat(argValues), outValues).SelectMany(x => x))
                .Concat(new TeuchiUdonAssembly[]
                    {
                        new Assembly_EXTERN(method)
                    })
                .Concat(outValues.SelectMany(x => x))
                .Concat(setValues.SelectMany(x => x))
                .Concat(tmpValues.SelectMany(x => x));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitInfix(InfixResult result)
        {
            switch (result.Op)
            {
                case ".":
                case "?.":
                   return VisitExpr(result.Expr1).Concat(VisitExpr(result.Expr2));
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
                        EvalInfixMethod
                        (
                            result.Methods[0],
                            new TeuchiUdonAssembly[][]
                            {
                                VisitExpr(result.Expr1).ToArray(),
                                VisitExpr(result.Expr2).ToArray()
                            },
                            result.OutValuess[0].Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) })
                        );
                case "==":
                case "!=":
                    return
                        result.Methods.Length == 1 && result.Methods[0] == null || result.Literals[0] == null ? new TeuchiUdonAssembly[0] :
                        result.Methods.Length == 0 ? new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(result.Literals[0])) } :
                        EvalInfixMethod
                        (
                            result.Methods[0],
                            new TeuchiUdonAssembly[][]
                            {
                                VisitExpr(result.Expr1).ToArray(),
                                VisitExpr(result.Expr2).ToArray()
                            },
                            result.OutValuess[0].Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) })
                        );
                case "&&":
                    return
                        EvalInfixConditionalAndMethod
                        (
                            VisitExpr(result.Expr1).ToArray(),
                            VisitExpr(result.Expr2).ToArray(),
                            new TeuchiUdonAssembly[] { new Assembly_PUSH         (new AssemblyAddress_DATA_LABEL(result.Literals[0])) },
                            new TeuchiUdonAssembly[] { new Assembly_JUMP_IF_FALSE(new AssemblyAddress_CODE_LABEL(result.Labels[0])) },
                            new TeuchiUdonAssembly[] { new Assembly_JUMP         (new AssemblyAddress_CODE_LABEL(result.Labels[1])) },
                            new TeuchiUdonAssembly[] { new Assembly_LABEL        (result.Labels[0]) },
                            new TeuchiUdonAssembly[] { new Assembly_LABEL        (result.Labels[1]) }
                        );
                case "||":
                    return
                        result.Methods[0] == null || result.Literals[0] == null ? new TeuchiUdonAssembly[0] :
                        EvalInfixConditionalOrMethod
                        (
                            result.Methods[0],
                            VisitExpr(result.Expr1).ToArray(),
                            VisitExpr(result.Expr2).ToArray(),
                            new TeuchiUdonAssembly[] { new Assembly_PUSH         (new AssemblyAddress_DATA_LABEL(result.OutValuess[0][0])) },
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
                        EvalInfixCoalescingMethod
                        (
                            result.Methods[0],
                            VisitExpr(result.Expr1).ToArray(),
                            VisitExpr(result.Expr2).ToArray(),
                            new TeuchiUdonAssembly[] { new Assembly_PUSH         (new AssemblyAddress_DATA_LABEL(result.OutValuess[0][0])) },
                            new TeuchiUdonAssembly[] { new Assembly_PUSH         (new AssemblyAddress_DATA_LABEL(result.OutValuess[1][0])) },
                            new TeuchiUdonAssembly[] { new Assembly_PUSH         (new AssemblyAddress_DATA_LABEL(result.Literals[0])) },
                            new TeuchiUdonAssembly[] { new Assembly_JUMP_IF_FALSE(new AssemblyAddress_CODE_LABEL(result.Labels[0])) },
                            new TeuchiUdonAssembly[] { new Assembly_JUMP         (new AssemblyAddress_CODE_LABEL(result.Labels[1])) },
                            new TeuchiUdonAssembly[] { new Assembly_LABEL        (result.Labels[0]) },
                            new TeuchiUdonAssembly[] { new Assembly_LABEL        (result.Labels[1]) }
                        );
                default:
                    return new TeuchiUdonAssembly[0];
            }
        }

        private IEnumerable<TeuchiUdonAssembly> EvalInfixMethod
        (
            TeuchiUdonMethod method,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> inValues,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> outValues
        )
        {
            return
                method.SortAlongParams(inValues, outValues)
                .SelectMany(x => x)
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(method)
                })
                .Concat(outValues.SelectMany(x => x));
        }

        private IEnumerable<TeuchiUdonAssembly> EvalInfixConditionalAndMethod
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

        private IEnumerable<TeuchiUdonAssembly> EvalInfixConditionalOrMethod
        (
            TeuchiUdonMethod method,
            IEnumerable<TeuchiUdonAssembly> value1,
            IEnumerable<TeuchiUdonAssembly> value2,
            IEnumerable<TeuchiUdonAssembly> outValue,
            IEnumerable<TeuchiUdonAssembly> literal,
            IEnumerable<TeuchiUdonAssembly> jumpIfFalse,
            IEnumerable<TeuchiUdonAssembly> jump,
            IEnumerable<TeuchiUdonAssembly> label1,
            IEnumerable<TeuchiUdonAssembly> label2
        )
        {
            return
                method.SortAlongParams(new TeuchiUdonAssembly[][] { value1.ToArray() }, new TeuchiUdonAssembly[][] { outValue.ToArray() })
                .SelectMany(x => x)
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(method)
                })
                .Concat(outValue)
                .Concat(jumpIfFalse)
                .Concat(value2)
                .Concat(jump)
                .Concat(label1)
                .Concat(literal)
                .Concat(label2);
        }

        private IEnumerable<TeuchiUdonAssembly> EvalInfixCoalescingMethod
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
                .Concat
                (
                    method.SortAlongParams(new TeuchiUdonAssembly[][] { tmpValue.ToArray(), literal.ToArray() }, new TeuchiUdonAssembly[][] { outValue.ToArray() })
                    .SelectMany(x => x)
                )
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

        protected IEnumerable<TeuchiUdonAssembly> VisitSetter(SetterResult result)
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
