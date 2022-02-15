using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonStrategySimple : TeuchiUdonStrategy
    {
        public override IEnumerable<TeuchiUdonAssembly> GetDataPartFromTables()
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

        public override IEnumerable<TeuchiUdonAssembly> GetCodePartFromTables()
        {
            return VisitTables();
        }

        public override IEnumerable<TeuchiUdonAssembly> GetCodePartFromResult(TeuchiUdonParserResult result)
        {
            return VisitResult(result);
        }

        protected override IEnumerable<TeuchiUdonAssembly> Pop()
        {
            return new TeuchiUdonAssembly[]
            {
                new Assembly_POP()
            };
        }

        protected override IEnumerable<TeuchiUdonAssembly> Get(TeuchiUdonAssemblyDataAddress address)
        {
            return new TeuchiUdonAssembly[]
            {
                new Assembly_PUSH(address)
            };
        }

        protected override IEnumerable<TeuchiUdonAssembly> Set(TeuchiUdonAssemblyDataAddress address)
        {
            return new TeuchiUdonAssembly[]
            {
                new Assembly_PUSH(address),
                new Assembly_COPY()
            };
        }

        protected override IEnumerable<TeuchiUdonAssembly> Prepare(TeuchiUdonAssemblyDataAddress address, out TeuchiUdonAssemblyDataAddress prepared)
        {
            prepared = address;
            return new TeuchiUdonAssembly[0];
        }

        protected override IEnumerable<TeuchiUdonAssembly> Retain(int count, out IExpectHolder holder)
        {
            holder = new ExpectHolder(count);
            return new TeuchiUdonAssembly[0];
        }

        protected override IEnumerable<TeuchiUdonAssembly> Expect(IExpectHolder holder, int count)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected override IEnumerable<TeuchiUdonAssembly> Release(IExpectHolder holder)
        {
            return new TeuchiUdonAssembly[0];
        }

        protected class ExpectHolder : IExpectHolder
        {
            public int Count { get; }

            public ExpectHolder(int count)
            {
                Count = count;
            }
        }

        public override IEnumerable<TeuchiUdonAssembly> DeclIndirectAddresses(IEnumerable<(TeuchiUdonIndirect indirect, uint address)> pairs)
        {
            return pairs.SelectMany(x => new TeuchiUdonAssembly[]
            {
                new Assembly_DECL_DATA(x.indirect, TeuchiUdonType.UInt, new AssemblyLiteral_NULL())
            });
        }
    }
}
