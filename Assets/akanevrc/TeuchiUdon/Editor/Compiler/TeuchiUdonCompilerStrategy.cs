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

        protected override IEnumerable<TeuchiUdonAssembly> ExportData(ITypedLabel label)
        {
            return FilterUnitType(label.Type, new TeuchiUdonAssembly[]
            {
                new Assembly_EXPORT_DATA(label)
            });
        }

        protected override IEnumerable<TeuchiUdonAssembly> SyncData(ITypedLabel label, TeuchiUdonSyncMode mode)
        {
            return FilterUnitType(label.Type, new TeuchiUdonAssembly[]
            {
                new Assembly_SYNC_DATA(label, TeuchiUdonAssemblySyncMode.Create(mode))
            });
        }

        protected override IEnumerable<TeuchiUdonAssembly> DeclData(ITypedLabel label, TeuchiUdonAssemblyLiteral literal)
        {
            return FilterUnitType(label.Type, new TeuchiUdonAssembly[]
            {
                new Assembly_DECL_DATA(label, label.Type, literal)
            });
        }

        protected override IEnumerable<TeuchiUdonAssembly> Pop()
        {
            return new TeuchiUdonAssembly[]
            {
                new Assembly_POP()
            };
        }

        protected override IEnumerable<TeuchiUdonAssembly> Get(ITeuchiUdonLabel label)
        {
            return label is ITypedLabel t ?
                FilterUnitType(t.Type, new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(t))
                }) :
                new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(label))
                };
        }

        protected override IEnumerable<TeuchiUdonAssembly> Indirect(ITeuchiUdonLabel label)
        {
            return new TeuchiUdonAssembly[]
            {
                new Assembly_PUSH(new AssemblyAddress_INDIRECT_LABEL(label))
            };
        }

        protected override IEnumerable<TeuchiUdonAssembly> Set(ITeuchiUdonLabel label)
        {
            return label is ITypedLabel t ?
                FilterUnitType(t.Type, new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(t)),
                    new Assembly_COPY()
                }) :
                new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(label)),
                    new Assembly_COPY()
                };
        }

        protected override IEnumerable<TeuchiUdonAssembly> Jump(ITeuchiUdonLabel label)
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
            .Concat(func.Vars.Reverse().SelectMany(y => FilterUnitType(y.Type, Set(y))))
            .Concat(VisitResult(func.Expr))
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
                    new Assembly_EXPORT_CODE(new TextLabel(eventName)),
                    new Assembly_LABEL      (new TextLabel(eventName)),
                    new Assembly_INDENT(1)
                }
                .Concat(stats.SelectMany(y => VisitResult(y)))
                .Concat(v?.Type.LogicalTypeNameEquals(TeuchiUdonType.Func) ?? false ?
                    EvalFunc
                    (
                        ev.OutParamUdonNames.Select(y => Get(new TextLabel(TeuchiUdonTables.GetEventParamName(varName, y)))),
                        new TextLabel($"topcall[{eventName}]"),
                        v
                    ) :
                    new TeuchiUdonAssembly[0]
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
            ITeuchiUdonLabel evalFunc,
            ITypedLabel funcAddress
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

        protected override IEnumerable<TeuchiUdonAssembly> IfElse
        (
            IEnumerable<TeuchiUdonAssembly> condition,
            IEnumerable<TeuchiUdonAssembly> truePart,
            IEnumerable<TeuchiUdonAssembly> falsePart,
            ITeuchiUdonLabel label1,
            ITeuchiUdonLabel label2
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

        private IEnumerable<TeuchiUdonAssembly> FilterUnitType(TeuchiUdonType type, IEnumerable<TeuchiUdonAssembly> asm)
        {
            return type.LogicalTypeEquals(TeuchiUdonType.Unit) ? new TeuchiUdonAssembly[0] : asm;
        }
    }
}
