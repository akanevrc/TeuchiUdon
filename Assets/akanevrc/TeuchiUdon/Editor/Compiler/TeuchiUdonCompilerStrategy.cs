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
            ).Compile();
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

        protected override IEnumerable<TeuchiUdonAssembly> ListCtor
        (
            IEnumerable<(IEnumerable<TeuchiUdonAssembly> init, IEnumerable<IEnumerable<TeuchiUdonAssembly>> elements)> listExprs,
            TeuchiUdonLiteral zero,
            TeuchiUdonLiteral one,
            TeuchiUdonLiteral two,
            TeuchiUdonLiteral nullValue,
            TeuchiUdonOutValue outValue1,
            TeuchiUdonOutValue outValue2,
            TeuchiUdonOutValue outValue3,
            TeuchiUdonMethod ctor,
            TeuchiUdonMethod setter
        )
        {
            var i = 0;
            return listExprs.SelectMany(x => x.elements.SelectMany(y =>
            {
                var ret = (IEnumerable<TeuchiUdonAssembly>)null;
                if (i == 0)
                {
                    ret =
                        new TeuchiUdonAssembly[]
                        {
                            new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(two)),
                            new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(outValue1)),
                            new Assembly_EXTERN(ctor),
                            new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(outValue1)),
                            new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(zero)),
                        }
                        .Concat(y)
                        .Concat(new TeuchiUdonAssembly[]
                        {
                            new Assembly_EXTERN(setter),
                        });
                }
                else
                {
                    ret =
                        new TeuchiUdonAssembly[]
                        {
                            new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(two)),
                            new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(i % 2 == 0 ? outValue3 : outValue2)),
                            new Assembly_EXTERN(ctor),
                            new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(i == 1 ? outValue1 : i % 2 == 0 ? outValue2 : outValue3)),
                            new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(one)),
                            new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(i % 2 == 0 ? outValue3 : outValue2)),
                            new Assembly_EXTERN(setter),
                            new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(i % 2 == 0 ? outValue3 : outValue2)),
                            new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(zero)),
                        }
                        .Concat(y)
                        .Concat(new TeuchiUdonAssembly[]
                        {
                            new Assembly_EXTERN(setter),
                        });
                }
                i++;
                return ret;
            }))
            .ToArray()
            .Concat
            (
                new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(i == 0 ? (IDataLabel)nullValue : (IDataLabel)outValue1))
                }
            );
        }
    }
}
