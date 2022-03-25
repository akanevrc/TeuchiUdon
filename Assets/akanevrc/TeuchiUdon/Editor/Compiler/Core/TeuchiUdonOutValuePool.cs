using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonOutValuePool
    {
        public static TeuchiUdonOutValuePool Instance { get; } = new TeuchiUdonOutValuePool();
        private static TeuchiUdonOutValue InvalidOutValue = new TeuchiUdonOutValue(TeuchiUdonQualifier.Top, TeuchiUdonType.Invalid, -1);

        public Dictionary<TeuchiUdonOutValue, TeuchiUdonOutValue> OutValues { get; private set; }
        private Dictionary<TeuchiUdonQualifier, Stack<Dictionary<TeuchiUdonType, SortedList<int, TeuchiUdonOutValue>>>> Retained { get; set; }
        private Dictionary<TeuchiUdonQualifier, Stack<Dictionary<TeuchiUdonType, SortedList<int, TeuchiUdonOutValue>>>> Released { get; set; }

        protected TeuchiUdonOutValuePool()
        {
        }

        public void Init()
        {
            OutValues = new Dictionary<TeuchiUdonOutValue , TeuchiUdonOutValue>();
            Retained  = new Dictionary<TeuchiUdonQualifier, Stack<Dictionary<TeuchiUdonType, SortedList<int, TeuchiUdonOutValue>>>>();
            Released  = new Dictionary<TeuchiUdonQualifier, Stack<Dictionary<TeuchiUdonType, SortedList<int, TeuchiUdonOutValue>>>>();
        }

        public void PushScope(TeuchiUdonQualifier qualifier)
        {
            PushScopeToTable(Retained, qualifier);
            PushScopeToTable(Released, qualifier);
        }

        private void PushScopeToTable
        (
            Dictionary<TeuchiUdonQualifier, Stack<Dictionary<TeuchiUdonType, SortedList<int, TeuchiUdonOutValue>>>> table,
            TeuchiUdonQualifier qualifier
        )
        {
            if (!table.ContainsKey(qualifier))
            {
                var stack = new Stack<Dictionary<TeuchiUdonType, SortedList<int, TeuchiUdonOutValue>>>();
                table.Add(qualifier, stack);
            }
            if (table[qualifier].Count == 0)
            {
                table[qualifier].Push(new Dictionary<TeuchiUdonType, SortedList<int, TeuchiUdonOutValue>>(TeuchiUdonTypeLogicalEqualityComparer.Instance));
            }
            else
            {
                table[qualifier].Push(table[qualifier].Peek()
                    .ToDictionary(x => x.Key, x => new SortedList<int, TeuchiUdonOutValue>(x.Value), TeuchiUdonTypeLogicalEqualityComparer.Instance));
            }
        }

        public void PopScope(TeuchiUdonQualifier qualifier)
        {
            PopScopeFromTable(Retained, qualifier);
            PopScopeFromTable(Released, qualifier);
        }

        private void PopScopeFromTable
        (
            Dictionary<TeuchiUdonQualifier, Stack<Dictionary<TeuchiUdonType, SortedList<int, TeuchiUdonOutValue>>>> table,
            TeuchiUdonQualifier qualifier
        )
        {
            if (table.ContainsKey(qualifier) && table[qualifier].Count >= 1)
            {
                table[qualifier].Pop();
            }
        }

        public TeuchiUdonOutValue RetainOutValue(TeuchiUdonQualifier qualifier, TeuchiUdonType type)
        {
            return RetainOutValueWithoutInvalids(qualifier, type, Enumerable.Empty<TeuchiUdonOutValue>());
        }

        public TeuchiUdonOutValue RetainOutValueWithoutInvalids(TeuchiUdonQualifier qualifier, TeuchiUdonType type, IEnumerable<TeuchiUdonOutValue> invalids)
        {
            if (!Retained.ContainsKey(qualifier) || !Released.ContainsKey(qualifier)) return InvalidOutValue;

            var retDic = Retained[qualifier].Peek();
            if (!retDic.ContainsKey(type))
            {
                retDic.Add(type, new SortedList<int, TeuchiUdonOutValue>());
            }

            var relDic = Released[qualifier].Peek();
            if (!relDic.ContainsKey(type))
            {
                relDic.Add(type, new SortedList<int, TeuchiUdonOutValue>());
            }

            var currentInvalids = invalids.Where(x => x.Type.LogicalTypeEquals(type)).Select(x => (k: x.Index, v: x)).ToArray();

            var retList = retDic[type];
            var relList = relDic[type];
            var valids  = new SortedList<int, TeuchiUdonOutValue>
            (
                relList.Select(x => (k: x.Key, v: x.Value))
                .Except(currentInvalids)
                .ToDictionary(x => x.k, x => x.v)
            );

            if (valids.Count == 0)
            {
                var used = new SortedList<int, TeuchiUdonOutValue>
                (
                    retList.Select(x => (k: x.Key, v: x.Value))
                    .Union(currentInvalids)
                    .ToDictionary(x => x.k, x => x.v)
                );
                var o = new TeuchiUdonOutValue(qualifier, type, used.Count);
                if (!retList.ContainsKey(o.Index))
                {
                    retList.Add(o.Index, o);
                }
                if (!OutValues.ContainsKey(o))
                {
                    OutValues.Add(o, o);
                }
                return o;
            }
            else
            {
                var o = valids.Values[0];
                retList.Add   (o.Index, o);
                relList.Remove(o.Index);
                return o;
            }
        }

        public void ReleaseOutValue(TeuchiUdonOutValue released)
        {
            if
            (
                Retained.ContainsKey(released.Qualifier) &&
                Retained[released.Qualifier].Store(out var retStack).Count != 0 &&
                retStack.Peek().Store(out var retDic).ContainsKey(released.Type) &&
                retDic[released.Type].Store(out var retList).ContainsKey(released.Index)
            )
            {
                retList.Remove(released.Index);
            }
            
            if
            (
                Released.ContainsKey(released.Qualifier) &&
                Released[released.Qualifier].Store(out var relStack).Count != 0 &&
                relStack.Peek().Store(out var relDic).ContainsKey(released.Type) &&
                !relDic[released.Type].Store(out var relList).ContainsKey(released.Index)
            )
            {
                relList.Add(released.Index, released);
            }
        }
    }
}
