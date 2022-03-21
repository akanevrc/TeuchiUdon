using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonOutValuePool
    {
        public static TeuchiUdonOutValuePool Instance { get; } = new TeuchiUdonOutValuePool();
        private static TeuchiUdonOutValue InvalidOutValue = new TeuchiUdonOutValue(TeuchiUdonQualifier.Top, TeuchiUdonType.Invalid, -1);
        private static int MaxCount { get; } = 64;

        public Dictionary<TeuchiUdonOutValue, TeuchiUdonOutValue> OutValues { get; private set; }
        private Dictionary<TeuchiUdonQualifier, Stack<Dictionary<TeuchiUdonType, SortedList<int, TeuchiUdonOutValue>>>> Pool { get; set; }

        protected TeuchiUdonOutValuePool()
        {
        }

        public void Init()
        {
            OutValues = new Dictionary<TeuchiUdonOutValue , TeuchiUdonOutValue>();
            Pool      = new Dictionary<TeuchiUdonQualifier, Stack<Dictionary<TeuchiUdonType, SortedList<int, TeuchiUdonOutValue>>>>();
        }

        public void PushScope(TeuchiUdonQualifier qualifier)
        {
            if (!Pool.ContainsKey(qualifier))
            {
                var stack = new Stack<Dictionary<TeuchiUdonType, SortedList<int, TeuchiUdonOutValue>>>();
                stack.Push(new Dictionary<TeuchiUdonType, SortedList<int, TeuchiUdonOutValue>>());
                Pool.Add(qualifier, stack);
            }
            if (Pool[qualifier].Count == 0)
            {
                Pool[qualifier].Push(new Dictionary<TeuchiUdonType, SortedList<int, TeuchiUdonOutValue>>());
            }
            else
            {
                Pool[qualifier].Push(Pool[qualifier].Peek().ToDictionary(x => x.Key, x => new SortedList<int, TeuchiUdonOutValue>(x.Value)));
            }
        }

        public void PopScope(TeuchiUdonQualifier qualifier)
        {
            if (Pool.ContainsKey(qualifier) && Pool[qualifier].Count >= 1)
            {
                Pool[qualifier].Pop();
            }
        }

        public IEnumerable<TeuchiUdonOutValue> RetainOutValues(TeuchiUdonQualifier qualifier, IEnumerable<TeuchiUdonType> types)
        {
            var typeArray = types.ToArray();
            if (!Pool.ContainsKey(qualifier)) return Enumerable.Repeat(InvalidOutValue, typeArray.Length);

            var dic       = Pool[qualifier].Peek();
            var outValues = new List<TeuchiUdonOutValue>();
            foreach (var t in typeArray)
            {
                if (!dic.ContainsKey(t))
                {
                    var unused = Enumerable.Range(0, MaxCount).Select(x => new TeuchiUdonOutValue(qualifier, t, x));
                    var l      = new SortedList<int, TeuchiUdonOutValue>();
                    foreach (var u in unused)
                    {
                        l.Add(u.Index, u);
                    }
                    dic.Add(t, l);
                }
                var list = dic[t];

                if (list.Count == 0)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(null, $"outvalue pool is empty");
                    return Enumerable.Repeat(InvalidOutValue, typeArray.Length);
                }
                
                var o = list.Values[0];
                list.RemoveAt(0);
                if (!OutValues.ContainsKey(o))
                {
                    OutValues.Add(o, o);
                }
                outValues.Add(o);
            }
            return outValues;
        }

        public void ReleaseOutValues(IEnumerable<TeuchiUdonOutValue> released)
        {
            foreach (var r in released)
            {
                Pool[r.Qualifier].Peek()[r.Type].Add(r.Index, r);
            }
        }
    }
}
