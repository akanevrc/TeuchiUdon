using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonStrategySimple : TeuchiUdonStrategy
    {
        public override IEnumerable<TeuchiUdonAssembly> GetDataPartFromTables(TeuchiUdonTables tables)
        {
            return
                tables.Vars.Values.SelectMany(x =>
                    x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(x, x.Type, new AssemblyLiteral_NULL())
                    })
                .Concat(tables.OutValues.Values.SelectMany(x =>
                    x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(x, x.Type, new AssemblyLiteral_NULL())
                    }))
                .Concat(tables.Literals.Values.Except(tables.Exports.Values).SelectMany(x =>
                    x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(x, x.Type, new AssemblyLiteral_NULL())
                    }))
                .Concat(tables.This.Values.SelectMany(x =>
                    x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(x, x.Type, new AssemblyLiteral_THIS())
                    }))
                .Concat(tables.Funcs.Values.SelectMany(x =>
                    x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[0] :
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(x.ReturnAddress, x.Type, new AssemblyLiteral_NULL())
                    }));
        }

        public override IEnumerable<TeuchiUdonAssembly> GetCodePartFromTables(TeuchiUdonTables tables)
        {
            return
                tables.Funcs.Values.Count == 0 ? new TeuchiUdonAssembly[0] :
                tables.Funcs.Values.Select(x =>
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_LABEL (x),
                        new Assembly_INDENT(1),
                        new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x.ReturnAddress)),
                        new Assembly_COPY()
                    }
                    .Concat(x.Vars.Reverse().SelectMany(y =>
                        y.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                        new TeuchiUdonAssembly[0] :
                        new TeuchiUdonAssembly[]
                        {
                            new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(y)),
                            new Assembly_COPY()
                        })
                    )
                    .Concat(Visit(x.Expr))
                    .Concat(new TeuchiUdonAssembly[]
                        {
                            new Assembly_JUMP_INDIRECT(new AssemblyAddress_DATA_LABEL(x.ReturnAddress)),
                            new Assembly_INDENT(-1)
                        }
                    )
                )
                .Aggregate((acc, x) => acc
                    .Concat(new TeuchiUdonAssembly[] { new Assembly_NEW_LINE() })
                    .Concat(x));
        }

        public override IEnumerable<TeuchiUdonAssembly> GetDataPartFromBody(BodyResult result)
        {
            var exports = result.TopStatements
                .Select(x => x is TopBindResult topBind ? topBind : null)
                .Where(x => x != null && x.Export);
            var syncs = result.TopStatements
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

        public override IEnumerable<TeuchiUdonAssembly> GetCodePartFromBody(BodyResult result)
        {
            var topFuncs = result.TopStatements
                .Where(x => x is TopBindResult topBind && topBind.VarBind.Vars.Length == 1 && topBind.VarBind.Vars[0].Type.LogicalTypeNameEquals(TeuchiUdonType.Func))
                .Select(x => (name: ((TopBindResult)x).VarBind.Vars[0].Name, x: (TopBindResult)x));
            var topStats = result.TopStatements
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
                .Concat(x.Value.SelectMany(y => Visit(y)))
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

        public override IEnumerable<TeuchiUdonAssembly> VisitTopBind(TopBindResult result)
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

        public override IEnumerable<TeuchiUdonAssembly> VisitTopExpr(TopExprResult result)
        {
            return VisitExpr(result.Expr);
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitInitVarAttr(InitVarAttrResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitExportVarAttr(ExportVarAttrResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitSyncVarAttr(SyncVarAttrResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitInitExprAttr(InitExprAttrResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitVarBind(VarBindResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitVarDecl(VarDeclResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitIdentifier(IdentifierResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitJump(JumpResult result)
        {
            return
                VisitExpr(result.Value)
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_JUMP_INDIRECT(new AssemblyAddress_DATA_LABEL(result.Label))
                });
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitLetBind(LetBindResult result)
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

        public override IEnumerable<TeuchiUdonAssembly> VisitExpr(ExprResult result)
        {
            return
                Visit(result.Inner)
                .Concat
                (
                    result.ReturnsValue || result.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                        new TeuchiUdonAssembly[0] :
                        new TeuchiUdonAssembly[] { new Assembly_POP() }
                );
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitBottom(BottomResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitUnknownType(UnknownTypeResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitUnit(UnitResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitBlock(BlockResult result)
        {
            return result.Statements.SelectMany(x => Visit(x)).Concat(VisitExpr(result.Expr));
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitParen(ParenResult result)
        {
            return VisitExpr(result.Expr);
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitLiteral(LiteralResult result)
        {
            return
                result.Literal.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                new TeuchiUdonAssembly[0] :
                new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(result.Literal))
                };
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitThis(ThisResult result)
        {
            return
                result.This.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                new TeuchiUdonAssembly[0] :
                new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(result.This))
                };
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitEvalVar(EvalVarResult result)
        {
            return
                result.Var.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                new TeuchiUdonAssembly[0] :
                new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(result.Var))
                };
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitEvalType(EvalTypeResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitEvalQualifier(EvalQualifierResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitEvalFunc(EvalFuncResult result)
        {
            return
                result.Args.SelectMany(x => VisitExpr(x))
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_INDIRECT_LABEL(result.EvalFunc)),
                    new Assembly_JUMP_INDIRECT(new AssemblyAddress_DATA_LABEL(result.Var)),
                    new Assembly_LABEL(result.EvalFunc)
                });
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitEvalMethod(EvalMethodResult result)
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

        public override IEnumerable<TeuchiUdonAssembly> VisitEvalQualifierCandidate(EvalQualifierCandidateResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitEvalMethodCandidate(EvalMethodCandidateResult result)
        {
            return new TeuchiUdonAssembly[0];
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitPrefix(PrefixResult result)
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
                        EvalPrefixMethod
                        (
                            result.Methods[0],
                            new TeuchiUdonAssembly[][]
                            {
                                VisitExpr(result.Expr).ToArray(),
                                new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(result.Literals[0])) }
                            },
                            new TeuchiUdonAssembly[][]
                            {
                                VisitExpr(result.Expr).ToArray()
                            }
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
                method.SortAlongParams(inValues, outValues).SelectMany(x => x)
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(method)
                })
                .Concat(outValues.SelectMany(x => x));
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitPostfix(PostfixResult result)
        {
            switch (result.Op)
            {
                case "++":
                case "--":
                    return
                        result.Methods[0] == null || result.Literals[0] == null ? new TeuchiUdonAssembly[0] :
                        EvalPostfixMethod
                        (
                            result.Methods[0],
                            new TeuchiUdonAssembly[][]
                            {
                                VisitExpr(result.Expr).ToArray(),
                                new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(result.Literals[0])) }
                            },
                            new TeuchiUdonAssembly[][]
                            {
                                VisitExpr(result.Expr).ToArray()
                            },
                            result.OutValuess[0].Select(x => new TeuchiUdonAssembly[] { new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x)) })
                        );
                default:
                    return new TeuchiUdonAssembly[0];
            }
        }

        private IEnumerable<TeuchiUdonAssembly> EvalPostfixMethod
        (
            TeuchiUdonMethod method,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> inValues,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> outValues,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> tmpValues
        )
        {
            return
                outValues.Zip(tmpValues, (o, t) => (o, t)).SelectMany(x => x.o.Concat(x.t).Concat(new TeuchiUdonAssembly[] { new Assembly_COPY() }))
                .Concat(method.SortAlongParams(inValues, outValues).SelectMany(x => x))
                .Concat(new TeuchiUdonAssembly[]
                    {
                        new Assembly_EXTERN(method)
                    })
                .Concat(tmpValues.SelectMany(x => x));
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitInfix(InfixResult result)
        {
            return VisitExpr(result.Expr1).Concat(VisitExpr(result.Expr2));
        }

        public override IEnumerable<TeuchiUdonAssembly> VisitLetInBind(LetInBindResult result)
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

        public override IEnumerable<TeuchiUdonAssembly> VisitFunc(FuncResult result)
        {
            return new TeuchiUdonAssembly[]
            {
                new Assembly_PUSH(new AssemblyAddress_INDIRECT_LABEL(result.Func))
            };
        }
    }
}
