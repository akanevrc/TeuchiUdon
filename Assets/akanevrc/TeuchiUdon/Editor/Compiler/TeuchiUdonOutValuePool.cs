using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonOutValuePool
    {
        public static TeuchiUdonOutValuePool Instance { get; } = new TeuchiUdonOutValuePool();
        public static int OutValueCount { get; } = 512;

        public Dictionary<int, TeuchiUdonOutValue> UsedOutValues { get; set; }
        private SortedDictionary<int, TeuchiUdonOutValue> OutValuePool { get; set; }
        private Stack<Dictionary<int, TeuchiUdonOutValue>> RetainedOutValues { get; set; }
        private TeuchiUdonOutValue InvalidOutValue { get; set; }

        protected TeuchiUdonOutValuePool()
        {
        }

        public void Init()
        {
            UsedOutValues = new Dictionary<int, TeuchiUdonOutValue>();

            var dic           = Enumerable.Range(0, OutValueCount).ToDictionary(x => x, x => new TeuchiUdonOutValue(x));
            OutValuePool      = new SortedDictionary<int, TeuchiUdonOutValue>(dic);
            RetainedOutValues = new Stack<Dictionary<int, TeuchiUdonOutValue>>();
            InvalidOutValue   = new TeuchiUdonOutValue(OutValueCount);
        }

        public void PushScope()
        {
            RetainedOutValues.Push(new Dictionary<int, TeuchiUdonOutValue>());
        }

        public void PopScope()
        {
            var outValues = RetainedOutValues.Pop();
            foreach (var o in outValues)
            {
                OutValuePool.Add(o.Key, o.Value);
            }
        }

        public IEnumerable<TeuchiUdonOutValue> RetainOutValues(int count)
        {
            if (OutValuePool.Count < count)
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(null, $"outvalue cannot be retained");
                return Enumerable.Repeat(InvalidOutValue, count);
            }

            var outValues = OutValuePool.Values.Take(count).ToArray();
            foreach (var o in outValues)
            {
                OutValuePool.Remove(o.Index);
                if (!UsedOutValues.ContainsKey(o.Index)) UsedOutValues.Add(o.Index, o);
                if (RetainedOutValues.Count > 0 && !RetainedOutValues.Peek().ContainsKey(o.Index)) RetainedOutValues.Peek().Add(o.Index, o);
            }

            return outValues;
        }
    }
}
