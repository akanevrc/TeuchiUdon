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

            var retList = retDic[type];
            var relList = relDic[type];
            if (relList.Count == 0)
            {
                var o = new TeuchiUdonOutValue(qualifier, type, retList.Count);
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
                var o = relList.Values[0];
                retList.Add   (o.Index, o);
                relList.Remove(o.Index);
                return o;
            }
        }

        public void RetainReleasedOutValue(TeuchiUdonOutValue outValue)
        {
            AddToTable     (Retained, outValue);
            RemoveFromTable(Released, outValue);
        }

        public void ReleaseOutValue(TeuchiUdonOutValue outValue)
        {
            RemoveFromTable(Retained, outValue);
            AddToTable     (Released, outValue);
        }

        private void AddToTable
        (
            Dictionary<TeuchiUdonQualifier, Stack<Dictionary<TeuchiUdonType, SortedList<int, TeuchiUdonOutValue>>>> table,
            TeuchiUdonOutValue outValue
        )
        {
            if
            (
                table.ContainsKey(outValue.Qualifier) &&
                table[outValue.Qualifier].Store(out var stack).Count != 0
            )
            {
                if (!stack.Peek().Store(out var dic).ContainsKey(outValue.Type))
                {
                    dic.Add(outValue.Type, new SortedList<int, TeuchiUdonOutValue>());
                }
                if (!dic[outValue.Type].Store(out var list).ContainsKey(outValue.Index))
                {
                    list.Add(outValue.Index, outValue);
                }
            }
        }

        private void RemoveFromTable
        (
            Dictionary<TeuchiUdonQualifier, Stack<Dictionary<TeuchiUdonType, SortedList<int, TeuchiUdonOutValue>>>> table,
            TeuchiUdonOutValue outValue
        )
        {
            if
            (
                table.ContainsKey(outValue.Qualifier) &&
                table[outValue.Qualifier].Store(out var stack).Count != 0 &&
                stack.Peek().Store(out var dic).ContainsKey(outValue.Type) &&
                dic[outValue.Type].Store(out var list).ContainsKey(outValue.Index)
            )
            {
                list.Remove(outValue.Index);
            }
        }
    }
}
