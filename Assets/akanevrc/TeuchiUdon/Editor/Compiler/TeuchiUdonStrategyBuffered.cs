using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonStrategyBuffered : TeuchiUdonStrategy
    {
        public TextLabel VarBuffer { get; } = new TextLabel("buffer[vars]");
        public TextLabel LiteralBuffer { get; } = new TextLabel("buffer[literals]");
        public TextLabel ReturnBuffer { get; } = new TextLabel("buffer[returns]");
        public TextLabel IndirectBuffer { get; } = new TextLabel("buffer[indirects]");
        public TextLabel TmpStack { get; } = new TextLabel("buffer[tmpstack]");
        public TextLabel TmpStackHead { get; } = new TextLabel("index[tmpstack]");
        public TextLabel Tmp_Get { get; } = new TextLabel("tmp[get]");
        public TextLabel Tmp_Set { get; } = new TextLabel("tmp[set]");
        public TextLabel Tmp_Prepare { get; } = new TextLabel("tmp[prepare]");
        public TextLabel Tmp_Index { get; } = new TextLabel("tmp[index]");
        public TextLabel One { get; } = new TextLabel("literal[1]");

        public TextLabel[] Tmps { get; private set; }

        private TeuchiUdonMethod DebugLogMethod
        {
            get
            {
                var type    = TeuchiUdonType.Type.ApplyArgAsType(new TeuchiUdonType("UnityEngineDebug"));
                var inTypes = new TeuchiUdonType[] { TeuchiUdonType.Object };
                var qm      = new TeuchiUdonMethod(type, "Log", inTypes);
                if (!TeuchiUdonTables.Instance.Methods.ContainsKey(qm)) throw new InvalidOperationException("'UnityEngine.Debug.Log' is not defined");
                return TeuchiUdonTables.Instance.Methods[qm];
            }
        }

        private TeuchiUdonMethod GetBufferMethod
        {
            get
            {
                var type    = TeuchiUdonType.Buffer;
                var inTypes = new TeuchiUdonType[] { TeuchiUdonType.Buffer, TeuchiUdonType.Int };
                var qm      = new TeuchiUdonMethod(type, "GetValue", inTypes);
                if (!TeuchiUdonTables.Instance.Methods.ContainsKey(qm)) throw new InvalidOperationException("'buffer.GetValue' is not defined");
                return TeuchiUdonTables.Instance.Methods[qm];
            }
        }

        private TeuchiUdonMethod SetBufferMethod
        {
            get
            {
                var type    = TeuchiUdonType.Buffer;
                var inTypes = new TeuchiUdonType[] { TeuchiUdonType.Buffer, TeuchiUdonType.Object, TeuchiUdonType.Int };
                var qm      = new TeuchiUdonMethod(type, "SetValue", inTypes);
                if (!TeuchiUdonTables.Instance.Methods.ContainsKey(qm)) throw new InvalidOperationException("'buffer.SetValue' is not defined");
                return TeuchiUdonTables.Instance.Methods[qm];
            }
        }

        private TeuchiUdonMethod LogicalOrMethod
        {
            get
            {
                var type    = TeuchiUdonType.Type.ApplyArgAsType(TeuchiUdonType.Int);
                var inTypes = new TeuchiUdonType[] { TeuchiUdonType.Int, TeuchiUdonType.Int };
                var qm      = new TeuchiUdonMethod(type, "op_LogicalOr", inTypes);
                if (!TeuchiUdonTables.Instance.Methods.ContainsKey(qm)) throw new InvalidOperationException("'int.op_LogicalOr' is not defined");
                return TeuchiUdonTables.Instance.Methods[qm];
            }
        }

        private TeuchiUdonMethod AdditionMethod
        {
            get
            {
                var type    = TeuchiUdonType.Type.ApplyArgAsType(TeuchiUdonType.Int);
                var inTypes = new TeuchiUdonType[] { TeuchiUdonType.Int, TeuchiUdonType.Int };
                var qm      = new TeuchiUdonMethod(type, "op_Addition", inTypes);
                if (!TeuchiUdonTables.Instance.Methods.ContainsKey(qm)) throw new InvalidOperationException("'int.op_Addition' is not defined");
                return TeuchiUdonTables.Instance.Methods[qm];
            }
        }

        private TeuchiUdonMethod SubtractionMethod
        {
            get
            {
                var type    = TeuchiUdonType.Type.ApplyArgAsType(TeuchiUdonType.Int);
                var inTypes = new TeuchiUdonType[] { TeuchiUdonType.Int, TeuchiUdonType.Int };
                var qm      = new TeuchiUdonMethod(type, "op_Subtraction", inTypes);
                if (!TeuchiUdonTables.Instance.Methods.ContainsKey(qm)) throw new InvalidOperationException("'int.op_Subtraction' is not defined");
                return TeuchiUdonTables.Instance.Methods[qm];
            }
        }

        private ITeuchiUdonLabel GetBuffer(IIndexedLabel label)
        {
            if (label is TeuchiUdonVar      v       ) return VarBuffer;
            if (label is TeuchiUdonLiteral  literal ) return LiteralBuffer;
            if (label is TeuchiUdonReturn   ret     ) return ReturnBuffer;
            if (label is TeuchiUdonIndirect indirect) return IndirectBuffer;
            return InvalidLabel.Instance;
        }

        private int GetBufferIndex(IIndexedLabel label)
        {
            if (label is TeuchiUdonVar      v       ) return v       .Index;
            if (label is TeuchiUdonLiteral  literal ) return literal .Index;
            if (label is TeuchiUdonReturn   ret     ) return ret     .Index;
            if (label is TeuchiUdonIndirect indirect) return indirect.Index;
            return 0;
        }

        private IEnumerable<TeuchiUdonAssembly> GetCode_GetNumber(int index)
        {
            var bits = ToBits(index).ToArray();
            if (bits.Length == 1)
            {
                return new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(bits[0]))
                };
            }
            else
            {
                return bits.SelectMany(x => new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(x))
                })
                .Concat(bits.Skip(1).SelectMany(x => new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Tmp_Index)),
                    new Assembly_EXTERN(LogicalOrMethod),
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Tmp_Index))
                }));
            }
        }

        private IEnumerable<TextLabel> ToBits(int num)
        {
            if (num == 0)
            {
                yield return new TextLabel($"literal[0]");
                yield break;
            }

            for (var i = 0; i < 31; i++)
            {
                if ((num >> i & 0x1) != 0) yield return new TextLabel($"literal[{0x1 << i}]");
            }
        }

        private AssemblyAddress_INDIRECT_LABEL GetCode_CreateIndirectAddress(string labelText, out TeuchiUdonIndirect indirect)
        {
            var label = new AssemblyAddress_INDIRECT_LABEL(new TextLabel(labelText));
            indirect  = label.Indirect;
            return label;
        }

        public override IEnumerable<TeuchiUdonAssembly> GetDataPartFromTables()
        {
            Tmps = Enumerable.Range(0, TeuchiUdonTables.Instance.ExpectCount).Select(x => new TextLabel($"tmp[{x}]")).ToArray();

            return
                (TeuchiUdonTables.Instance.ExportedVars.Keys.SelectMany(x => !x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_EXPORT_DATA(new TextLabel(x.Name))
                    } :
                    new TeuchiUdonAssembly[0]
                ))
                .Concat(TeuchiUdonTables.Instance.SyncedVars.SelectMany(x => !x.Key.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_SYNC_DATA(new TextLabel(x.Key.Name), TeuchiUdonAssemblySyncMode.Create(x.Value))
                    } :
                    new TeuchiUdonAssembly[0]
                ))
                .Concat(TeuchiUdonTables.Instance.UnbufferedVars.Values.SelectMany(x => !x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(x, x.Type, new AssemblyLiteral_NULL())
                    } :
                    new TeuchiUdonAssembly[0]
                ))
                .Concat(new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(VarBuffer, TeuchiUdonType.Buffer, new AssemblyLiteral_NULL())
                    }
                )
                .Concat(new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(LiteralBuffer, TeuchiUdonType.Buffer, new AssemblyLiteral_NULL())
                    }
                )
                .Concat(new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(ReturnBuffer, TeuchiUdonType.Buffer, new AssemblyLiteral_NULL())
                    } 
                )
                .Concat(new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(IndirectBuffer, TeuchiUdonType.Buffer, new AssemblyLiteral_NULL())
                    }
                )
                .Concat(TeuchiUdonTables.Instance.This.Values.SelectMany(x => !x.Type.LogicalTypeEquals(TeuchiUdonType.Unit) ?
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(x, x.Type, new AssemblyLiteral_THIS())
                    } :
                    new TeuchiUdonAssembly[0]
                ))
                .Concat(Enumerable.Range(0, 32).SelectMany(x =>
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(new TextLabel($"literal[{0x40000000 >> x}]"), TeuchiUdonType.Int, new AssemblyLiteral_RAW($"{0x40000000 >> x}"))
                    }
                ))
                .Concat(TeuchiUdonTables.Instance.OutValues.Values.SelectMany(x =>
                    new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(x, x.Type, new AssemblyLiteral_NULL())
                    }
                ))
                .Concat(new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(TmpStack    , TeuchiUdonType.Buffer, new AssemblyLiteral_NULL()),
                        new Assembly_DECL_DATA(TmpStackHead, TeuchiUdonType.Int   , new AssemblyLiteral_RAW("0")),
                        new Assembly_DECL_DATA(Tmp_Get     , TeuchiUdonType.Object, new AssemblyLiteral_NULL()),
                        new Assembly_DECL_DATA(Tmp_Set     , TeuchiUdonType.Object, new AssemblyLiteral_NULL()),
                        new Assembly_DECL_DATA(Tmp_Prepare , TeuchiUdonType.Object, new AssemblyLiteral_NULL()),
                        new Assembly_DECL_DATA(Tmp_Index   , TeuchiUdonType.Object, new AssemblyLiteral_NULL())
                    }
                )
                .Concat(Tmps.SelectMany(x => new TeuchiUdonAssembly[]
                    {
                        new Assembly_DECL_DATA(x, TeuchiUdonType.Object, new AssemblyLiteral_NULL())
                    }
                ));
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
                new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(TmpStackHead)),
                new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(One)),
                new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(TmpStackHead)),
                new Assembly_EXTERN(SubtractionMethod)
            };
        }

        protected override IEnumerable<TeuchiUdonAssembly> Get(TeuchiUdonAssemblyDataAddress address)
        {
            var label = (IIndexedLabel)       null;
            var dummy = (TeuchiUdonAssembly[])null;
            if (address is AssemblyAddress_DATA_LABEL data && data.Label is IIndexedLabel indexed)
            {
                label = indexed;
                dummy = new TeuchiUdonAssembly[0];
            }
            else if (address is AssemblyAddress_INDIRECT_LABEL indirect)
            {
                label = indirect.Indirect;
                dummy = new TeuchiUdonAssembly[]
                {
                    new Assembly_DUMMY(address)
                };
            }
            else
            {
                return new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(TmpStack)),
                    new Assembly_PUSH(address),
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(TmpStackHead)),
                    new Assembly_EXTERN(SetBufferMethod),
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(TmpStackHead)),
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(One)),
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(TmpStackHead)),
                    new Assembly_EXTERN(AdditionMethod)
                };
            }

            if 
            (
                label is TeuchiUdonVar v && (TeuchiUdonTables.Instance.ExportedVars.ContainsKey(v) || TeuchiUdonTables.Instance.SyncedVars.ContainsKey(v)) ||
                label is TeuchiUdonOutValue
            )
            {
                return new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(TmpStack)),
                    new Assembly_PUSH(address),
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(TmpStackHead)),
                    new Assembly_EXTERN(SetBufferMethod),
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(TmpStackHead)),
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(One)),
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(TmpStackHead)),
                    new Assembly_EXTERN(AdditionMethod)
                };
            }
            else
            {
                return dummy
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(GetBuffer(label)))
                })
                .Concat(GetCode_GetNumber(GetBufferIndex(label)))
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Tmp_Get)),
                    new Assembly_EXTERN(GetBufferMethod),
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(TmpStack)),
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Tmp_Get)),
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(TmpStackHead)),
                    new Assembly_EXTERN(SetBufferMethod),
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(TmpStackHead)),
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(One)),
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(TmpStackHead)),
                    new Assembly_EXTERN(AdditionMethod)
                });
            }
        }

        protected override IEnumerable<TeuchiUdonAssembly> Set(TeuchiUdonAssemblyDataAddress address)
        {
            var label = (IIndexedLabel)       null;
            var dummy = (TeuchiUdonAssembly[])null;
            if (address is AssemblyAddress_DATA_LABEL data && data.Label is IIndexedLabel indexed)
            {
                label = indexed;
                dummy = new TeuchiUdonAssembly[0];
            }
            else if (address is AssemblyAddress_INDIRECT_LABEL indirect)
            {
                label = indirect.Indirect;
                dummy = new TeuchiUdonAssembly[]
                {
                    new Assembly_DUMMY(address)
                };
            }
            else
            {
                return new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(address),
                    new Assembly_COPY()
                };
            }

            if
            (
                label is TeuchiUdonVar v && (TeuchiUdonTables.Instance.ExportedVars.ContainsKey(v) || TeuchiUdonTables.Instance.SyncedVars.ContainsKey(v)) ||
                label is TeuchiUdonOutValue
            )
            {
                return new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(address),
                    new Assembly_COPY()
                };
            }
            else
            {
                return dummy
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Tmp_Set)),
                    new Assembly_COPY(),
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(GetBuffer(label))),
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Tmp_Set))
                })
                .Concat(GetCode_GetNumber(GetBufferIndex(label)))
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_EXTERN(SetBufferMethod)
                });
            }
        }

        protected override IEnumerable<TeuchiUdonAssembly> Prepare(TeuchiUdonAssemblyDataAddress address, out TeuchiUdonAssemblyDataAddress prepared)
        {
            var label = (IIndexedLabel)       null;
            var dummy = (TeuchiUdonAssembly[])null;
            if (address is AssemblyAddress_DATA_LABEL data && data.Label is IIndexedLabel indexed)
            {
                label = indexed;
                dummy = new TeuchiUdonAssembly[0];
            }
            else if (address is AssemblyAddress_INDIRECT_LABEL indirect)
            {
                label = indirect.Indirect;
                dummy = new TeuchiUdonAssembly[]
                {
                    new Assembly_DUMMY(address)
                };
            }
            else
            {
                prepared = address;
                return new TeuchiUdonAssembly[0];
            }

            if
            (
                label is TeuchiUdonVar v && (TeuchiUdonTables.Instance.ExportedVars.ContainsKey(v) || TeuchiUdonTables.Instance.SyncedVars.ContainsKey(v)) ||
                label is TeuchiUdonOutValue
            )
            {
                prepared = address;
                return new TeuchiUdonAssembly[0];
            }
            else
            {
                prepared = new AssemblyAddress_DATA_LABEL(Tmp_Prepare);
                return dummy
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(GetBuffer(label)))
                })
                .Concat(GetCode_GetNumber(GetBufferIndex(label)))
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Tmp_Prepare)),
                    new Assembly_EXTERN(GetBufferMethod)
                });
            }
        }

        protected override IEnumerable<TeuchiUdonAssembly> Retain(int count, out IExpectHolder holder)
        {
            var concrete = new ExpectHolder(this, count);
            holder       = concrete;
            return concrete.Retain();
        }

        protected override IEnumerable<TeuchiUdonAssembly> Expect(IExpectHolder holder, int count)
        {
            if (holder is ExpectHolder concrete)
            {
                return concrete.Expect(count);
            }
            else
            {
                return new TeuchiUdonAssembly[0];
            }
        }

        protected override IEnumerable<TeuchiUdonAssembly> Release(IExpectHolder holder)
        {
            if (holder is ExpectHolder concrete)
            {
                return concrete.Release();
            }
            else
            {
                return new TeuchiUdonAssembly[0];
            }
        }

        protected class ExpectHolder : IExpectHolder
        {
            private TeuchiUdonStrategyBuffered Parent { get; }
            public int Count { get; }
            public int Current { get; set; }
            private ITeuchiUdonLabel[] Labels { get; }

            public ExpectHolder(TeuchiUdonStrategyBuffered parent, int count)
            {
                Parent  = parent;
                Count   = count;
                Current = 0;
                Labels  = parent.Tmps.Take(count).ToArray();
            }

            public IEnumerable<TeuchiUdonAssembly> Retain()
            {
                return new TeuchiUdonAssembly[0];
            }

            public IEnumerable<TeuchiUdonAssembly> Expect(int count)
            {
                var basis  = Count - Current;
                var offset = Current;
                Current += count;

                return Enumerable.Range(0, count).SelectMany(x => new TeuchiUdonAssembly[]
                    {
                        new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Parent.TmpStackHead))
                    }
                    .Concat(Parent.GetCode_GetNumber(basis - x))
                    .Concat(new TeuchiUdonAssembly[]
                    {
                        new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Labels[offset + x])),
                        new Assembly_EXTERN(Parent.SubtractionMethod),
                        new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Parent.TmpStack)),
                        new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Labels[offset + x])),
                        new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Labels[offset + x])),
                        new Assembly_EXTERN(Parent.GetBufferMethod),
                        new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Labels[offset + x]))
                    })
                );
            }

            public IEnumerable<TeuchiUdonAssembly> Release()
            {
                if (Count != Current) throw new InvalidOperationException("expect not consumed all");
                return new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Parent.TmpStackHead))
                }
                .Concat(Parent.GetCode_GetNumber(Count))
                .Concat(new TeuchiUdonAssembly[]
                {
                    new Assembly_PUSH(new AssemblyAddress_DATA_LABEL(Parent.TmpStackHead)),
                    new Assembly_EXTERN(Parent.SubtractionMethod)
                });
            }
        }

        public override IEnumerable<TeuchiUdonAssembly> DeclIndirectAddresses(IEnumerable<(TeuchiUdonIndirect indirect, uint address)> pairs)
        {
            return new TeuchiUdonAssembly[0];
        }
    }
}
