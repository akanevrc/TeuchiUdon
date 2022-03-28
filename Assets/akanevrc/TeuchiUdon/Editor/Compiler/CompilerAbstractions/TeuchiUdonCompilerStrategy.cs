using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonCompilerStrategy : TeuchiUdonCompilerAbstraction
    {
        public static TeuchiUdonCompilerStrategy Instance { get; } = new TeuchiUdonCompilerStrategy();

        protected TeuchiUdonCompilerStrategy()
        {
        }

        public override void Init()
        {
        }

        protected override IEnumerable<TeuchiUdonAssembly> Debug(IEnumerable<TeuchiUdonAssembly> asm)
        {
            return EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { asm }, Enumerable.Empty<IDataLabel>(), TeuchiUdonMethod.GetDebugMethod());
        }

        protected override IEnumerable<TeuchiUdonAssembly> ExportData(IDataLabel label)
        {
            return new TeuchiUdonDataLabelWrapper
            (
                label,
                false,
                l => new TeuchiUdonAssembly[]
                    {
                        new Assembly_EXPORT_DATA(l)
                    },
                (l, thisObj) => l.IterateAssemblyLabels().SelectMany(x => thisObj.VisitDataLabel(x))
            )
            .Compile();
        }

        protected override IEnumerable<TeuchiUdonAssembly> SyncData(IDataLabel label, TeuchiUdonSyncMode mode)
        {
            return new TeuchiUdonDataLabelWrapper
            (
                label,
                false,
                l => new TeuchiUdonAssembly[]
                    {
                        new Assembly_SYNC_DATA(l, TeuchiUdonAssemblySyncMode.Create(mode))
                    },
                (l, thisObj) => l.IterateAssemblyLabels().SelectMany(x => thisObj.VisitDataLabel(x))
            )
            .Compile();
        }

        protected override IEnumerable<TeuchiUdonAssembly> DeclData(IDataLabel label, TeuchiUdonAssemblyLiteral literal)
        {
            return new TeuchiUdonDataLabelWrapper
            (
                label,
                false,
                l => new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(l, l.Type, literal)
                    },
                (l, thisObj) => l.IterateAssemblyLabels().SelectMany(x => thisObj.VisitDataLabel(x))
            )
            .Compile();
        }

        protected override IEnumerable<TeuchiUdonAssembly> Pop(TeuchiUdonType type)
        {
            return
                type.LogicalTypeNameEquals(TeuchiUdonType.Tuple) ?
                type.GetArgsAsTuple().SelectMany(x => Pop(x)) :
                new TeuchiUdonAssembly[]
                {
                    new Assembly_POP()
                };
        }

        protected override IEnumerable<TeuchiUdonAssembly> Get(IDataLabel label)
        {
            return new TeuchiUdonDataLabelWrapper
            (
                label,
                false,
                l => new TeuchiUdonAssembly[]
                    {
                        new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(l))
                    },
                (l, thisObj) => l.IterateAssemblyLabels().SelectMany(x => thisObj.VisitDataLabel(x))
            )
            .Compile();
        }

        protected override IEnumerable<TeuchiUdonAssembly> Set(IDataLabel label)
        {
            return new TeuchiUdonDataLabelWrapper
            (
                label,
                true,
                l => new TeuchiUdonAssembly[]
                    {
                        new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(l)),
                        new Assembly_COPY()
                    },
                (l, thisObj) => l.IterateAssemblyLabels().SelectMany(x => thisObj.VisitDataLabel(x))
            )
            .Compile();
        }

        protected override IEnumerable<TeuchiUdonAssembly> Indirect(ICodeLabel label)
        {
            return new TeuchiUdonAssembly[]
            {
                new Assembly_PUSH(new AssemblyAddress_INDIRECT_LABEL(label))
            };
        }

        protected override IEnumerable<TeuchiUdonAssembly> Jump(IDataLabel label)
        {
            return new TeuchiUdonAssembly[]
            {
                new Assembly_JUMP_INDIRECT(new AssemblyAddress_DATA_LABEL(label))
            };
        }

        protected override IEnumerable<TeuchiUdonAssembly> Func(TeuchiUdonFunc func)
        {
            return new TeuchiUdonAssembly[]
            {
                new Assembly_LABEL (func),
                new Assembly_INDENT(1),
            }
            .Concat(Set(func.Return))
            .Concat(func.Vars.Reverse().SelectMany(x => Set(x)))
            .Concat(VisitExpr(func.Expr))
            .Concat(new TeuchiUdonAssembly[]
            {
                new Assembly_JUMP_INDIRECT(new AssemblyAddress_DATA_LABEL(func.Return)),
                new Assembly_INDENT(-1)
            });
        }

        protected override IEnumerable<TeuchiUdonAssembly> Event(string varName, string eventName, TeuchiUdonMethod ev, List<TopStatementResult> stats)
        {
            var v =
                TeuchiUdonTables.Instance.Vars.ContainsKey(new TeuchiUdonVar(TeuchiUdonQualifier.Top, varName)) ?
                TeuchiUdonTables.Instance.Vars[new TeuchiUdonVar(TeuchiUdonQualifier.Top, varName)] :
                null;
            return
                new TeuchiUdonAssembly[]
                {
                    new Assembly_EXPORT_CODE(new TextCodeLabel(eventName)),
                    new Assembly_LABEL      (new TextCodeLabel(eventName)),
                    new Assembly_INDENT(1)
                }
                .Concat(stats.SelectMany(x => VisitTopStatement(x)))
                .Concat(v?.Type.IsFunc() ?? false ?
                    EvalFunc
                    (
                        ev.OutParamUdonNames
                            .Select(x => TeuchiUdonTables.Instance.Vars[new TeuchiUdonVar(TeuchiUdonQualifier.Top, TeuchiUdonTables.GetEventParamName(varName, x))])
                            .Select(x => Get(x)),
                        new TextCodeLabel($"topcall[{eventName}]"),
                        v
                    ) :
                    Enumerable.Empty<TeuchiUdonAssembly>()
                )
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_JUMP(new AssemblyAddress_NUMBER(0xFFFFFFFC)),
                    new Assembly_INDENT(-1)
                });
        }

        protected override IEnumerable<TeuchiUdonAssembly> EvalFunc
        (
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> args,
            ICodeLabel evalFunc,
            IDataLabel funcAddress
        )
        {
            return
                args.SelectMany(x => x)
                .Concat(Indirect(evalFunc))
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_JUMP_INDIRECT(new AssemblyAddress_DATA_LABEL(funcAddress)),
                    new Assembly_LABEL(evalFunc)
                });
        }

        protected override IEnumerable<TeuchiUdonAssembly> EvalMethod
        (
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> inValues,
            IEnumerable<IDataLabel> outValues,
            TeuchiUdonMethod method
        )
        {
            return
                inValues.SelectMany(x => x)
                .Concat(outValues.SelectMany(x => Get(x)))
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(method)
                })
                .Concat(outValues.SelectMany(x => Get(x)));
        }

        protected override IEnumerable<TeuchiUdonAssembly> CallMethod
        (
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> inValues,
            IEnumerable<IDataLabel> outValues,
            TeuchiUdonMethod method
        )
        {
            return
                inValues.SelectMany(x => x)
                .Concat(outValues.SelectMany(x => Get(x)))
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(method)
                });
        }

        protected override IEnumerable<TeuchiUdonAssembly> EvalAssign(IEnumerable<TeuchiUdonAssembly> value1, IEnumerable<TeuchiUdonAssembly> value2)
        {
            return
                value2
                .Concat(value1)
                .Concat(new TeuchiUdonAssembly[] { new Assembly_COPY() });
        }

        protected override IEnumerable<TeuchiUdonAssembly> EvalSetterAssign
        (
            IEnumerable<TeuchiUdonAssembly> instance,
            IEnumerable<TeuchiUdonAssembly> value2,
            TeuchiUdonMethod setterMethod
        )
        {
            return
                instance
                .Concat(value2)
                .Concat(new TeuchiUdonAssembly[] { new Assembly_EXTERN(setterMethod) });
        }

        protected override IEnumerable<TeuchiUdonAssembly> IfElse
        (
            IEnumerable<TeuchiUdonAssembly> condition,
            IEnumerable<TeuchiUdonAssembly> thenPart,
            IEnumerable<TeuchiUdonAssembly> elsePart,
            ICodeLabel label1,
            ICodeLabel label2
        )
        {
            return
                condition
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_JUMP_IF_FALSE(new AssemblyAddress_CODE_LABEL(label1))
                })
                .Concat(thenPart)
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_JUMP(new AssemblyAddress_CODE_LABEL(label2)),
                    new Assembly_LABEL(label1)
                })
                .Concat(elsePart)
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_LABEL(label2)
                });
        }

        protected override IEnumerable<TeuchiUdonAssembly> IfElif
        (
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> conditions,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> thenParts,
            IEnumerable<ICodeLabel> labels
        )
        {
            var ifs  = conditions.Zip(thenParts, (c, t) => (c, t));
            var head = ifs.First();
            var tail =
                ifs
                .Skip(1)
                .Zip(labels        , (x, pl) => (x.c, x.t,   pl))
                .Zip(labels.Skip(1), (x, l ) => (x.c, x.t, x.pl, l));
            var firstLabel = labels.First();
            var lastLabel  = labels.Last();
            return
                tail
                .Aggregate
                (
                    head.c
                    .Concat(new TeuchiUdonAssembly[]
                    {
                        new Assembly_JUMP_IF_FALSE(new AssemblyAddress_CODE_LABEL(firstLabel))
                    })
                    .Concat(head.t),
                    (acc, x) =>
                        acc
                        .Concat(new TeuchiUdonAssembly[]
                        {
                            new Assembly_JUMP(new AssemblyAddress_CODE_LABEL(lastLabel)),
                            new Assembly_LABEL(x.pl)
                        })
                        .Concat(x.c)
                        .Concat(new TeuchiUdonAssembly[]
                        {
                            new Assembly_JUMP_IF_FALSE(new AssemblyAddress_CODE_LABEL(x.l))
                        })
                        .Concat(x.t)
                )
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_LABEL(lastLabel)
                });
        }

        protected override IEnumerable<TeuchiUdonAssembly> IfElifElse
        (
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> conditions,
            IEnumerable<IEnumerable<TeuchiUdonAssembly>> thenParts,
            IEnumerable<TeuchiUdonAssembly> elsePart,
            IEnumerable<ICodeLabel> labels
        )
        {
            var ifs =
                conditions
                .Zip(thenParts, (c, t) => (c, t))
                .Zip(labels   , (x, l) => (x.c, x.t, l));
            var lastLabel = labels.Last();
            return
                ifs
                .Aggregate
                (
                    Enumerable.Empty<TeuchiUdonAssembly>(),
                    (acc, x) =>
                        acc
                        .Concat(x.c)
                        .Concat(new TeuchiUdonAssembly[]
                        {
                            new Assembly_JUMP_IF_FALSE(new AssemblyAddress_CODE_LABEL(x.l))
                        })
                        .Concat(x.t)
                        .Concat(new TeuchiUdonAssembly[]
                        {
                            new Assembly_JUMP(new AssemblyAddress_CODE_LABEL(lastLabel)),
                            new Assembly_LABEL(x.l)
                        })
                        
                )
                .Concat(elsePart)
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_LABEL(lastLabel)
                });
        }

        protected override IEnumerable<TeuchiUdonAssembly> EmptyArrayCtor
        (
            TeuchiUdonLiteral zero,
            TeuchiUdonOutValue array,
            TeuchiUdonMethod ctor
        )
        {
            return EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(zero) }, new IDataLabel[] { array }, ctor);
        }

        protected override IEnumerable<TeuchiUdonAssembly> ArrayElementsCtor
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
        )
        {
            return
                CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(length) }, new IDataLabel[] { array }, ctor)
                .Concat(Get(zero))
                .Concat(Set(key))
                .Concat(elements.SelectMany(x =>
                            CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(array), Get(key), x }, Enumerable.Empty<IDataLabel>(), setter)
                    .Concat(CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(key), Get(one) }, new IDataLabel[] { key }, addition))
                ))
                .Concat(Get(array));
        }

        protected override IEnumerable<TeuchiUdonAssembly> ArrayRangeCtor
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
        )
        {
            return
                firstExpr
                .Concat(Set(value))
                .Concat(lastExpr)
                .Concat(Set(valueLimit))
                .Concat
                (
                    IfElse
                    (
                        EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(value), Get(valueLimit) }, new IDataLabel[] { condition }, valueLessThanOrEqual),
                        (
                            type.LogicalTypeEquals(TeuchiUdonType.Int) ?
                                    CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(valueLimit ), Get(value) }, new IDataLabel[] { length      }, valueSubtraction) :
                                    CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(valueLimit ), Get(value) }, new IDataLabel[] { valueLength }, valueSubtraction)
                            .Concat(CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(valueLength)             }, new IDataLabel[] { length      }, valueToKey))
                        )
                        .Concat(CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(length), Get(one) }, new IDataLabel[] { length }, keyAddition)),
                        Get(zero).Concat(Set(length)),
                        branchLabel1,
                        branchLabel2
                    )
                )
                .Concat(CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(length) }, new IDataLabel[] { array }, ctor))
                .Concat(Get(zero))
                .Concat(Set(key))
                .Concat(EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(value), Get(valueLimit) }, new IDataLabel[] { condition }, valueLessThanOrEqual))
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_JUMP_IF_FALSE(new AssemblyAddress_CODE_LABEL(loopLabel2)),
                    new Assembly_LABEL(loopLabel1)
                })
                .Concat(CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(array), Get(key), Get(value) }, Enumerable.Empty<IDataLabel>(), setter))
                .Concat(CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(key  ), Get(one       ) }, new IDataLabel[] { key       }, keyAddition))
                .Concat(CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(value), Get(step      ) }, new IDataLabel[] { value     }, valueAddition))
                .Concat(EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(value), Get(valueLimit) }, new IDataLabel[] { condition }, valueGreaterThan))
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_JUMP_IF_FALSE(new AssemblyAddress_CODE_LABEL(loopLabel1)),
                    new Assembly_LABEL(loopLabel2)
                })
                .Concat(Get(array));
        }

        protected override IEnumerable<TeuchiUdonAssembly> ArraySteppedRangeCtor
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
        )
        {
            return
                firstExpr
                .Concat(Set(value))
                .Concat(lastExpr)
                .Concat(Set(valueLimit))
                .Concat(stepExpr)
                .Concat(Set(valueStep))
                .Concat
                (
                    IfElse
                    (
                        EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(valueStep), Get(valueZero) }, new IDataLabel[] { isUpTo }, valueEquality),
                        EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(zero     )                 }, new IDataLabel[] { array  }, ctor),
                        CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(valueStep), Get(valueZero) }, new IDataLabel[] { isUpTo }, valueGreaterThan)
                        .Concat
                        (
                            type.LogicalTypeEquals(TeuchiUdonType.Int) ?
                                    CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(valueLimit ), Get(value    ) }, new IDataLabel[] { length      }, valueSubtraction)
                            .Concat(CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(length     ), Get(valueStep) }, new IDataLabel[] { length      }, valueDivision)) :
                                    CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(valueLimit ), Get(value    ) }, new IDataLabel[] { valueLength }, valueSubtraction)
                            .Concat(CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(valueLength), Get(valueStep) }, new IDataLabel[] { valueLength }, valueDivision))
                            .Concat(CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(valueLength)                 }, new IDataLabel[] { length      }, valueToKey))
                        )
                        .Concat(CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(length), Get(one) }, new IDataLabel[] { length }, keyAddition))
                        .Concat
                        (
                            IfElse
                            (
                                EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(zero), Get(length) }, new IDataLabel[] { condition }, keyGreaterThan),
                                CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(zero)              }, new IDataLabel[] { array     }, ctor),
                                CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(length)            }, new IDataLabel[] { array     }, ctor),
                                branchLabel1,
                                branchLabel2
                            )
                        )
                        .Concat(Get(zero))
                        .Concat(Set(key))
                        .Concat
                        (
                            IfElse
                            (
                                Get(isUpTo),
                                EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(value), Get(valueLimit) }, new IDataLabel[] { condition }, valueLessThanOrEqual),
                                EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(valueLimit), Get(value) }, new IDataLabel[] { condition }, valueLessThanOrEqual),
                                branchLabel3,
                                branchLabel4
                            )
                        )
                        .Concat(new TeuchiUdonAssembly[]
                        {
                            new Assembly_JUMP_IF_FALSE(new AssemblyAddress_CODE_LABEL(loopLabel2)),
                            new Assembly_LABEL(loopLabel1)
                        })
                        .Concat(CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(array), Get(key), Get(value) }, Enumerable.Empty<IDataLabel>(), setter))
                        .Concat(CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(key  ), Get(one      ) }, new IDataLabel[] { key   }, keyAddition))
                        .Concat(CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(value), Get(valueStep) }, new IDataLabel[] { value }, valueAddition))
                        .Concat
                        (
                            IfElse
                            (
                                Get(isUpTo),
                                EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(value), Get(valueLimit) }, new IDataLabel[] { condition }, valueGreaterThan),
                                EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(valueLimit), Get(value) }, new IDataLabel[] { condition }, valueGreaterThan),
                                branchLabel5,
                                branchLabel6
                            )
                        )
                        .Concat(new TeuchiUdonAssembly[]
                        {
                            new Assembly_JUMP_IF_FALSE(new AssemblyAddress_CODE_LABEL(loopLabel1)),
                            new Assembly_LABEL(loopLabel2)
                        })
                        .Concat(Get(array)),
                        branchLabel7,
                        branchLabel8
                    )
                );
        }

        protected override IEnumerable<TeuchiUdonAssembly> ArraySpreadCtor
        (
            IEnumerable<TeuchiUdonAssembly> expr,
            TeuchiUdonOutValue array,
            TeuchiUdonMethod clone
        )
        {
            return EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { expr }, new IDataLabel[] { array }, clone);
        }
    }
}
