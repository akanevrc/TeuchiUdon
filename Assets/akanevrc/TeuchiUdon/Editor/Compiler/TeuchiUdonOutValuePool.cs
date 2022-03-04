using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonOutValuePool
    {
        public static TeuchiUdonOutValuePool Instance { get; } = new TeuchiUdonOutValuePool();

        public Dictionary<TeuchiUdonOutValue, TeuchiUdonOutValue> OutValues { get; private set; }
        private Dictionary<TeuchiUdonQualifier, Stack<int>> Counters { get; set; }

        protected TeuchiUdonOutValuePool()
        {
        }

        public void Init()
        {
            OutValues = new Dictionary<TeuchiUdonOutValue , TeuchiUdonOutValue>();
            Counters  = new Dictionary<TeuchiUdonQualifier, Stack<int>>();
        }

        public void PushScope(TeuchiUdonQualifier qualifier)
        {
            if (!Counters.ContainsKey(qualifier))
            {
                var stack = new Stack<int>();
                stack.Push(0);
                Counters.Add(qualifier, stack);
            }
            Counters[qualifier].Push(Counters[qualifier].Peek());
        }

        public void PopScope(TeuchiUdonQualifier qualifier)
        {
            Counters[qualifier].Pop();
        }

        public IEnumerable<TeuchiUdonOutValue> RetainOutValues(TeuchiUdonQualifier qualifier, int count)
        {
            var index = Counters[qualifier].Pop();
            Counters[qualifier].Push(index + count);

            var outValues = Enumerable.Range(index, count).Select(x => new TeuchiUdonOutValue(qualifier, x)).ToArray();
            foreach (var o in outValues)
            {
                if (!OutValues.ContainsKey(o))
                {
                    OutValues.Add(o, o);
                }
            }
            return outValues;
        }
    }
}
