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

        protected override IEnumerable<TeuchiUdonAssembly> Pop()
        {
            return new TeuchiUdonAssembly[]
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
                .Concat(v?.Type.LogicalTypeNameEquals(TeuchiUdonType.Func) ?? false ?
                    EvalFunc
                    (
                        ev.OutParamUdonNames
                            .Zip(ev.OutTypes, (n, t) => (n, t))
                            .Select(x => Get(new TextDataLabel(TeuchiUdonTables.GetEventParamName(varName, x.n), x.t))),
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
            IEnumerable<TeuchiUdonOutValue> outValues,
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
            IEnumerable<TeuchiUdonAssembly> truePart,
            IEnumerable<TeuchiUdonAssembly> falsePart,
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
                .Concat(truePart)
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_JUMP(new AssemblyAddress_CODE_LABEL(label2)),
                    new Assembly_LABEL(label1)
                })
                .Concat(falsePart)
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_LABEL(label2)
                });
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
                CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(length) }, new TeuchiUdonOutValue[] { array }, ctor)
                .Concat(Get(zero))
                .Concat(Set(key))
                .Concat(elements.SelectMany(y =>
                            CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(array), Get(key), y }, Enumerable.Empty<TeuchiUdonOutValue>(), setter)
                    .Concat(CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(key), Get(one) }, new TeuchiUdonOutValue[] { key }, addition))
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
            TeuchiUdonOutValue array,
            TeuchiUdonOutValue key,
            TeuchiUdonMethod ctor,
            TeuchiUdonMethod setter,
            TeuchiUdonMethod valueLessThanOrEqual,
            TeuchiUdonMethod valueGreaterThan,
            TeuchiUdonMethod keyAddition,
            TeuchiUdonMethod valueAddition,
            TeuchiUdonMethod valueSubtraction,
            ICodeLabel branchLabel1,
            ICodeLabel branchLabel2,
            ICodeLabel loopLabel1,
            ICodeLabel loopLabel2
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
                        EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(value), Get(valueLimit) }, new TeuchiUdonOutValue[] { condition }, valueLessThanOrEqual),
                                CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(valueLimit), Get(value) }, new TeuchiUdonOutValue[] { length }, valueSubtraction)
                        .Concat(EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(length), Get(one) }, new TeuchiUdonOutValue[] { length }, valueAddition)),
                        Get(zero),
                        branchLabel1,
                        branchLabel2
                    )
                )
                .Concat(CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(length) }, new TeuchiUdonOutValue[] { array }, ctor))
                .Concat(Get(zero))
                .Concat(Set(key))
                .Concat(EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(value), Get(valueLimit) }, new TeuchiUdonOutValue[] { condition }, valueLessThanOrEqual))
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_JUMP_IF_FALSE(new AssemblyAddress_CODE_LABEL(loopLabel2)),
                    new Assembly_LABEL(loopLabel1)
                })
                .Concat(CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(array), Get(key), Get(value) }, Enumerable.Empty<TeuchiUdonOutValue>(), setter))
                .Concat(CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(key  ), Get(one       ) }, new TeuchiUdonOutValue[] { key       }, keyAddition))
                .Concat(CallMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(value), Get(step      ) }, new TeuchiUdonOutValue[] { value     }, valueAddition))
                .Concat(EvalMethod(new IEnumerable<TeuchiUdonAssembly>[] { Get(value), Get(valueLimit) }, new TeuchiUdonOutValue[] { condition }, valueGreaterThan))
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_JUMP_IF_FALSE(new AssemblyAddress_CODE_LABEL(loopLabel1)),
                    new Assembly_LABEL(loopLabel2)
                })
                .Concat(Get(array));
        }
    }
}
