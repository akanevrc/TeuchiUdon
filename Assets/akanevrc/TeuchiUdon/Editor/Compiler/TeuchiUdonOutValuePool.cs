using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonOutValuePool
    {
        public static TeuchiUdonOutValuePool Instance { get; } = new TeuchiUdonOutValuePool();
        private static TeuchiUdonOutValue InvalidOutValue = new TeuchiUdonOutValue(TeuchiUdonQualifier.Top, TeuchiUdonType.Invalid, -1);

        public Dictionary<TeuchiUdonOutValue, TeuchiUdonOutValue> OutValues { get; private set; }
        private Dictionary<TeuchiUdonQualifier, Stack<Dictionary<TeuchiUdonType, int>>> Counters { get; set; }

        protected TeuchiUdonOutValuePool()
        {
        }

        public void Init()
        {
            OutValues = new Dictionary<TeuchiUdonOutValue , TeuchiUdonOutValue>();
            Counters  = new Dictionary<TeuchiUdonQualifier, Stack<Dictionary<TeuchiUdonType, int>>>();
        }

        public void PushScope(TeuchiUdonQualifier qualifier)
        {
            if (!Counters.ContainsKey(qualifier))
            {
                var stack = new Stack<Dictionary<TeuchiUdonType, int>>();
                stack.Push(new Dictionary<TeuchiUdonType, int>());
                Counters.Add(qualifier, stack);
            }
            if (Counters[qualifier].Count == 0)
            {
                Counters[qualifier].Push(new Dictionary<TeuchiUdonType, int>());
            }
            else
            {
                Counters[qualifier].Push(new Dictionary<TeuchiUdonType, int>(Counters[qualifier].Peek()));
            }
        }

        public void PopScope(TeuchiUdonQualifier qualifier)
        {
            if (Counters.ContainsKey(qualifier) && Counters[qualifier].Count >= 1)
            {
                Counters[qualifier].Pop();
            }
        }

        public IEnumerable<TeuchiUdonOutValue> RetainOutValues(TeuchiUdonQualifier qualifier, IEnumerable<TeuchiUdonType> types)
        {
            var typeArray = types.ToArray();
            if (!Counters.ContainsKey(qualifier)) return Enumerable.Repeat(InvalidOutValue, typeArray.Length);

            var dic       = Counters[qualifier].Peek();
            var outValues = new List<TeuchiUdonOutValue>();
            foreach (var t in typeArray)
            {
                if (!dic.ContainsKey(t))
                {
                    dic.Add(t, 0);
                }
                var index = dic[t]++;
                
                var o = new TeuchiUdonOutValue(qualifier, t, index);
                if (!OutValues.ContainsKey(o))
                {
                    OutValues.Add(o, o);
                }
                outValues.Add(o);
            }
            return outValues;
        }
    }
}
