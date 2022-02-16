using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public abstract class TeuchiUdonStrategy
    {
        public static TeuchiUdonStrategy Instance { get; private set; }

        public static void SetStrategy<T>() where T : TeuchiUdonStrategy, new()
        {
            Instance = new T();
        }

        public abstract IEnumerable<TeuchiUdonAssembly> GetDataPartFromTables();
        public abstract IEnumerable<TeuchiUdonAssembly> GetCodePartFromTables();
        public abstract IEnumerable<TeuchiUdonAssembly> GetCodePartFromResult(TeuchiUdonParserResult result);

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
                    .Concat(Retain(x.Vars.Count(y => !y.Type.LogicalTypeEquals(TeuchiUdonType.Unit)) + 1, out var holder))
                    .Concat(Expect(holder, holder.Count))
                    .Concat(Set(new AssemblyAddress_DATA_LABEL(x.Return)))
                    .Concat(x.Vars.Reverse().SelectMany(y =>
                        y.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                        new TeuchiUdonAssembly[0] :
                        Set(new AssemblyAddress_DATA_LABEL(y))
                    ))
                    .Concat(Release(holder))
                    .Concat(VisitResult(x.Expr))
                    .Concat(Prepare(new AssemblyAddress_DATA_LABEL(x.Return), out var address))
                    .Concat(new TeuchiUdonAssembly[]
                        {
                            new Assembly_JUMP_INDIRECT(address),
                            new Assembly_INDENT(-1)
                        }
                    )
                )
                .Aggregate((acc, x) => acc
                    .Concat(new TeuchiUdonAssembly[] { new Assembly_NEW_LINE() })
                    .Concat(x));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitResult(TeuchiUdonParserResult result)
        {
                 if (result is BodyResult                   body                  ) return VisitBody                  (body);
            else if (result is TopBindResult                topBind               ) return VisitTopBind               (topBind);
            else if (result is TopExprResult                topExpr               ) return VisitTopExpr               (topExpr);
            else if (result is InitVarAttrResult            initVarAttr           ) return VisitInitVarAttr           (initVarAttr);
            else if (result is ExportVarAttrResult          exportVarAttr         ) return VisitExportVarAttr         (exportVarAttr);
            else if (result is SyncVarAttrResult            syncVarAttr           ) return VisitSyncVarAttr           (syncVarAttr);
            else if (result is InitExprAttrResult           initExprAttr          ) return VisitInitExprAttr          (initExprAttr);
            else if (result is VarBindResult                varBind               ) return VisitVarBind               (varBind);
            else if (result is VarDeclResult                varDecl               ) return VisitVarDecl               (varDecl);
            else if (result is IdentifierResult             identifier            ) return VisitIdentifier            (identifier);
            else if (result is JumpResult                   jump                  ) return VisitJump                  (jump);
            else if (result is LetBindResult                letBind               ) return VisitLetBind               (letBind);
            else if (result is ExprResult                   expr                  ) return VisitExpr                  (expr);
            else if (result is BottomResult                 bottom                ) return VisitBottom                (bottom);
            else if (result is UnknownTypeResult            unknownType           ) return VisitUnknownType           (unknownType);
            else if (result is UnitResult                   unit                  ) return VisitUnit                  (unit);
            else if (result is BlockResult                  block                 ) return VisitBlock                 (block);
            else if (result is ParenResult                  paren                 ) return VisitParen                 (paren);
            else if (result is LiteralResult                literal               ) return VisitLiteral               (literal);
            else if (result is ThisResult                   ths                   ) return VisitThis                  (ths);
            else if (result is EvalVarResult                evalVar               ) return VisitEvalVar               (evalVar);
            else if (result is EvalTypeResult               evalType              ) return VisitEvalType              (evalType);
            else if (result is EvalQualifierResult          evalQualifier         ) return VisitEvalQualifier         (evalQualifier);
            else if (result is EvalFuncResult               evalFunc              ) return VisitEvalFunc              (evalFunc);
            else if (result is EvalMethodResult             evalMethod            ) return VisitEvalMethod            (evalMethod);
            else if (result is EvalQualifierCandidateResult evalQualifierCandidate) return VisitEvalQualifierCandidate(evalQualifierCandidate);
            else if (result is EvalMethodCandidateResult    evalMethodCandidate   ) return VisitEvalMethodCandidate   (evalMethodCandidate);
            else if (result is PrefixResult                 prefix                ) return VisitPrefix                (prefix);
            else if (result is PostfixResult                postfix               ) return VisitPostfix               (postfix);
            else if (result is InfixResult                  infix                 ) return VisitInfix                 (infix);
            else if (result is LetInBindResult              letInBind             ) return VisitLetInBind             (letInBind);
            else if (result is FuncResult                   func                  ) return VisitFunc                  (func);
            else throw new InvalidOperationException("unsupported parser result type");
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitBody(BodyResult result)
        {
            var topFuncs = result.TopStatements
                .Where(x => x is TopBindResult topBind && topBind.VarBind.Vars.Length == 1 && topBind.VarBind.Vars[0].Type.LogicalTypeNameEquals(TeuchiUdonType.Func))
                .Select(x => (name: ((TopBindResult)x).VarBind.Vars[0].Name, x: (TopBindResult)x));
            var topStats = result.TopStatements
                .Where(x => !(x is TopBindResult topBind))
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
                    .Concat(x.Value.SelectMany(y => VisitResult(y)))
                    .Concat(TeuchiUdonTables.Instance.Vars.ContainsKey(new TeuchiUdonVar(TeuchiUdonQualifier.Top, x.Key)) ?
                        Get(new AssemblyAddress_INDIRECT_LABEL(new TextLabel($"topcall[{x.Key}]")))
                        .Concat(Prepare(new AssemblyAddress_DATA_LABEL(TeuchiUdonTables.Instance.Vars[new TeuchiUdonVar(TeuchiUdonQualifier.Top, x.Key)]), out var address))
                        .Concat(new TeuchiUdonAssembly[]
                            {
                                new Assembly_JUMP_INDIRECT(address),
                                new Assembly_LABEL(new TextLabel($"topcall[{x.Key}]")),
                                new Assembly_JUMP(new AssemblyAddress_NUMBER(0xFFFFFFFC)),
                                new Assembly_INDENT(-1)
                            }
                        ) :
                        new TeuchiUdonAssembly[0]
                    ))
                .Aggregate((acc, x) => acc
                    .Concat(new TeuchiUdonAssembly[] { new Assembly_NEW_LINE() })
                    .Concat(x));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitTopBind(TopBindResult result)
        {
            return
                VisitExpr(result.VarBind.Expr)
                .Concat(Retain(result.VarBind.Vars.Count(x => !x.Type.LogicalTypeEquals(TeuchiUdonType.Unit)), out var holder))
                .Concat(Expect(holder, holder.Count))
                .Concat(result.VarBind.Vars.Reverse().SelectMany(x =>
                    x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    Set(new AssemblyAddress_DATA_LABEL(x))
                ))
                .Concat(Release(holder));
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
                .Concat(Prepare(new AssemblyAddress_DATA_LABEL(result.Label), out var address))
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_JUMP_INDIRECT(address)
                });
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitLetBind(LetBindResult result)
        {
            return
                VisitExpr(result.VarBind.Expr)
                .Concat(Retain(result.VarBind.Vars.Count(x => !x.Type.LogicalTypeEquals(TeuchiUdonType.Unit)), out var holder))
                .Concat(Expect(holder, holder.Count))
                .Concat(result.VarBind.Vars.Reverse().SelectMany(x =>
                    x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    Set(new AssemblyAddress_DATA_LABEL(x))
                ))
                .Concat(Release(holder));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitExpr(ExprResult result)
        {
            return
                VisitResult(result.Inner)
                .Concat
                (
                    result.ReturnsValue || result.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                        new TeuchiUdonAssembly[0] :
                        Pop()
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
                Get(new AssemblyAddress_DATA_LABEL(result.Literal));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitThis(ThisResult result)
        {
            return
                result.This.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                new TeuchiUdonAssembly[0] :
                Get(new AssemblyAddress_DATA_LABEL(result.This));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalVar(EvalVarResult result)
        {
            return
                result.Var.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                new TeuchiUdonAssembly[0] :
                Get(new AssemblyAddress_DATA_LABEL(result.Var));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalType(EvalTypeResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalQualifier(EvalQualifierResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalFunc(EvalFuncResult result)
        {
            return
                result.Args.SelectMany(x => VisitExpr(x))
                .Concat(Get(new AssemblyAddress_INDIRECT_LABEL(result.EvalFunc)))
                .Concat(Prepare(new AssemblyAddress_DATA_LABEL(result.Var), out var address))
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_JUMP_INDIRECT(address),
                    new Assembly_LABEL(result.EvalFunc)
                });
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalMethod(EvalMethodResult result)
        {
            return
                Retain(result.Args.Length, out var holder)
                .Concat
                (
                    this is TeuchiUdonStrategySimple ?
                    result.Method.SortAlongParams
                    (
                        result.Args.Select(x => VisitExpr(x).Concat(Expect(holder, 1))).ToArray(),
                        result.OutValues.Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) })
                    )
                    .SelectMany(x => x)
                    :
                    result.Args.SelectMany(x => VisitExpr(x))
                    .Concat
                    (
                        result.Method.SortAlongParams
                        (
                            result.Args.Select(_ => Expect(holder, 1)).ToArray(),
                            result.OutValues.Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) })
                        )
                        .SelectMany(x => x)
                    )
                )
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(result.Method)
                })
                .Concat(Release(holder))
                .Concat(result.OutValues.SelectMany(x => Get(new AssemblyAddress_DATA_LABEL(x))));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalQualifierCandidate(EvalQualifierCandidateResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitEvalMethodCandidate(EvalMethodCandidateResult result)
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
                            result.OutValuess[0].Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) }),
                            result.OutValuess[0].Select(x => Get(new AssemblyAddress_DATA_LABEL(x)))
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
                                Get(new AssemblyAddress_DATA_LABEL(result.Literals[0])).ToArray()
                            },
                            result.OutValuess[0].Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) }),
                            result.OutValuess[0].Select(x => Get(new AssemblyAddress_DATA_LABEL(x)))
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
                                Get(new AssemblyAddress_DATA_LABEL(result.Literals[0])).ToArray()
                            },
                            result.OutValuess[0].Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) }),
                            result.Expr.Inner.LeftValues.Select(x => Set(new AssemblyAddress_DATA_LABEL(x))),
                            result.OutValuess[0].Select(x => Get(new AssemblyAddress_DATA_LABEL(x)))
                        );
                default:
                    return new TeuchiUdonAssembly[0];
            }
        }

        private IEnumerable<TeuchiUdonAssembly> EvalPrefixMethod
        (
            TeuchiUdonMethod method,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> inValues,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> outValues,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> retValues
        )
        {
            return
                Retain(inValues.Count(), out var holder)
                .Concat
                (
                    this is TeuchiUdonStrategySimple ?
                    method.SortAlongParams
                    (
                        inValues.Select(x => x.Concat(Expect(holder, 1))).ToArray(),
                        outValues
                    )
                    .SelectMany(x => x)
                    :
                    inValues.SelectMany(x => x)
                    .Concat
                    (
                        method.SortAlongParams
                        (
                            inValues.Select(_ => Expect(holder, 1)).ToArray(),
                            outValues
                        )
                        .SelectMany(x => x)
                    )
                )
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(method)
                })
                .Concat(Release(holder))
                .Concat(retValues.SelectMany(x => x));
        }

        private IEnumerable<TeuchiUdonAssembly> EvalPrefixAssignMethod
        (
            TeuchiUdonMethod method,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> inValues,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> outValues,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> setValues,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> retValues
        )
        {
            return
                Retain(inValues.Count(), out var holder)
                .Concat
                (
                    this is TeuchiUdonStrategySimple ?
                    method.SortAlongParams
                    (
                        inValues.Select(x => x.Concat(Expect(holder, 1))).ToArray(),
                        outValues
                    )
                    .SelectMany(x => x)
                    :
                    inValues.SelectMany(x => x)
                    .Concat
                    (
                        method.SortAlongParams
                        (
                            inValues.Select(_ => Expect(holder, 1)).ToArray(),
                            outValues
                        )
                        .SelectMany(x => x)
                    )
                )
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(method)
                })
                .Concat(Release(holder))
                .Concat(outValues.SelectMany(x => x))
                .Concat(setValues.SelectMany(x => x))
                .Concat(retValues.SelectMany(x => x));
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
                                Get(new AssemblyAddress_DATA_LABEL(result.Literals[0])).ToArray()
                            },
                            result.TmpValuess[0].Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) }),
                            result.OutValuess[0].Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) }),
                            result.Expr.Inner.LeftValues.Select(x => Set(new AssemblyAddress_DATA_LABEL(x))),
                            result.TmpValuess[0].Select(x => Get(new AssemblyAddress_DATA_LABEL(x)))
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
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> setValues,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> retValues
        )
        {
            return
                inValues
                    .Zip(tmpValues, (i, t) => (i, t))
                    .SelectMany(x =>
                        Retain(1, out var h)
                        .Concat(x.i)
                        .Concat(Expect(h, 1))
                        .Concat(x.t)
                        .Concat(new TeuchiUdonAssembly[] { new Assembly_COPY() })
                        .Concat(Release(h))
                    )
                .Concat(Retain(argValues.Count(), out var holder))
                .Concat
                (
                    this is TeuchiUdonStrategySimple ?
                    method.SortAlongParams
                    (
                        tmpValues.Concat(argValues.Select(x => x.Concat(Expect(holder, 1))).ToArray()),
                        outValues
                    )
                    .SelectMany(x => x)
                    :
                    argValues.SelectMany(x => x)
                    .Concat
                    (
                        method.SortAlongParams
                        (
                            tmpValues.Concat(argValues.Select(_ => Expect(holder, 1)).ToArray()),
                            outValues
                        )
                        .SelectMany(x => x)
                    )
                )
                .Concat(new TeuchiUdonAssembly[]
                    {
                        new Assembly_EXTERN(method)
                    })
                .Concat(Release(holder))
                .Concat(outValues.SelectMany(x => x))
                .Concat(setValues.SelectMany(x => x))
                .Concat(retValues.SelectMany(x => x));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitInfix(InfixResult result)
        {
            return VisitExpr(result.Expr1).Concat(VisitExpr(result.Expr2));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitLetInBind(LetInBindResult result)
        {
            return
                VisitExpr(result.VarBind.Expr)
                .Concat(Retain(result.VarBind.Vars.Count(x => !x.Type.LogicalTypeEquals(TeuchiUdonType.Unit)), out var holder))
                .Concat(Expect(holder, holder.Count))
                .Concat(result.VarBind.Vars.Reverse().SelectMany(x =>
                    x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    Set(new AssemblyAddress_DATA_LABEL(x))
                ))
                .Concat(Release(holder))
                .Concat(VisitExpr(result.Expr));
        }

        protected IEnumerable<TeuchiUdonAssembly> VisitFunc(FuncResult result)
        {
            return Get(new AssemblyAddress_INDIRECT_LABEL(result.Func));
        }

        protected abstract IEnumerable<TeuchiUdonAssembly> Pop();
        protected abstract IEnumerable<TeuchiUdonAssembly> Get(TeuchiUdonAssemblyDataAddress address);
        protected abstract IEnumerable<TeuchiUdonAssembly> Set(TeuchiUdonAssemblyDataAddress address);
        protected abstract IEnumerable<TeuchiUdonAssembly> Prepare(TeuchiUdonAssemblyDataAddress address, out TeuchiUdonAssemblyDataAddress prepared);

        protected abstract IEnumerable<TeuchiUdonAssembly> Retain(int count, out IExpectHolder holder);
        protected abstract IEnumerable<TeuchiUdonAssembly> Expect(IExpectHolder holder, int count);
        protected abstract IEnumerable<TeuchiUdonAssembly> Release(IExpectHolder holder);

        protected interface IExpectHolder
        {
            int Count { get; }
        }

        public abstract IEnumerable<TeuchiUdonAssembly> DeclIndirectAddresses(IEnumerable<(TeuchiUdonIndirect indirect, uint address)> pairs);
    }
}
